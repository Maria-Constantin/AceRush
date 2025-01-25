using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class PracticeMode: MonoBehaviour
{
    public TextMeshProUGUI playerHandRankText;
    public TextMeshProUGUI botHandRankText;

    public List<Card> deck = new List<Card>();

    public List<Card> playerHand = new List<Card>();
    public List<Card> botHand = new List<Card>();

    public List<Card> communityCards = new List<Card>();

    public GameObject playerHandObject;
    public GameObject botHandObject;
    public GameObject communityCardsObject;

    public Sprite[] cardSprites;
    public GameObject cardPrefab;
    public Transform cardSpawnPoint;

    private HandEvaluation handEvaluation;

    public Button callButton;
    public Button raiseButton;
    public Button foldButton;

    public GameData gameData;
    public DataManager dataManager;
    private List<string> possibleUsernames = new List<string>();

    private string playerUsername;
    private int playerCurrency;

    private int pot = 0;
    private int playerBet = 0;
    private int playerChips = 10000;

    private int raiseAmount = 2000;
    private int previousBet = 0;

    public int smallBlindAmount = 1000;
    public int bigBlindAmount = 2000;

    private int currentPlayerIndex = 0; 

    private List<List<Card>> allPlayers = new List<List<Card>>();
    public List<GameBot> bots = new List<GameBot>();

    private void Start()
    {
        GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position + Vector3.right, Quaternion.identity);
        Card cardScript = cardObject.AddComponent<Card>();

        handEvaluation = GetComponent<HandEvaluation>();
        if (handEvaluation == null)
        {
            Debug.LogError("HandEvaluation component not found. Add it to the GameObject.");
        }

        InitialiseDeck();
        ShuffleDeck();
        AssignSprites();

        callButton.onClick.AddListener(Call);
        raiseButton.onClick.AddListener(Raise);
        foldButton.onClick.AddListener(Fold);

        gameData.UpdatePlayerUserData();

        gameData.UpdateBotUserData(gameData.botusername, gameData.botcurrency);

        allPlayers.Add(playerHand);
        allPlayers.Add(botHand);

        DealCards(5, communityCards);

        HandRank playerHandRank = handEvaluation.EvaluateHandRank(communityCards.Concat(playerHand).ToList());
        HandRank botHandRank = handEvaluation.EvaluateHandRank(communityCards.Concat(botHand).ToList());
        playerHandRankText.text = $"{playerHandRank}";
        botHandRankText.text = $"{botHandRank}";

        PreFlop();

        if (cardScript == null)
        {
            Debug.LogError("Card component not found on card object!");
            cardScript = cardObject.AddComponent<Card>();
        }

    }

    private void PreFlop()
    {
        playerChips -= smallBlindAmount;
        pot += smallBlindAmount;
        playerBet = smallBlindAmount;
        currentPlayerIndex = 1;

        Flop();
    }

    private void Flop()
    {
        DealCards(5, communityCards);
        DealCards(2, playerHand);
        DealCards(2, botHand);

        AssignCardsToSlots(playerHand, playerHandObject);
        AssignCardsToSlots(botHand, botHandObject);
        AssignCardsToSlots(communityCards, communityCardsObject);

        currentPlayerIndex = 0; // Start with the player
        StartBettingRound();
    }

    private void Turn()
    {
        DealCards(1, communityCards);
        StartBettingRound();
    }

    private void River()
    {
        DealCards(1, communityCards);
        StartBettingRound();
    }

    public void StartBettingRound()
    {
        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (currentPlayerIndex == 0)
            {
                // Human player's turn
            }
            else
            {
                // Bot's turn
                bots[currentPlayerIndex - 1].MakeDecision(playerBet, GetCallAmount());
            }
        }
    }

    public void DetermineWinner()
    {
        handEvaluation = GetComponent<HandEvaluation>();

        HandRank playerHandRank = handEvaluation.EvaluateHandRank(communityCards.Concat(playerHand).ToList());
        HandRank botHandRank = handEvaluation.EvaluateHandRank(communityCards.Concat(botHand).ToList());
        playerHandRankText.text = $"{playerHandRank}";
    }

    public void Call()
    {
        int callAmount = GetCallAmount();

        if (playerChips >= callAmount)
        {
            playerChips -= callAmount;
            pot += callAmount;
            Debug.Log($"Player called for {callAmount} chips.");
        }
        else
        {
            Debug.Log("Not enough chips to call.");
        }
    }


    public void Raise()
    {
        int totalBet = GetTotalBetAmount();

        if (playerChips >= totalBet)
        {
            playerChips -= totalBet;
            pot += totalBet;
            previousBet = totalBet;
            Debug.Log($"Player raised to {totalBet} chips.");
        }
        else
        {
            Debug.Log("Not enough chips to raise.");
        }
    }

    public void Fold()
    {
        Debug.Log("Player folded.");

        ResetGameState();

        StartNewHand();
    }

    private void InitializeButtons()
    {
        callButton.onClick.AddListener(Call);
        raiseButton.onClick.AddListener(Raise);
        foldButton.onClick.AddListener(Fold);
    }

    private void PerformBlindBetting()
    {
        playerChips -= smallBlindAmount;
        pot += smallBlindAmount;
        playerBet = smallBlindAmount;
    }

    private void MoveToNextPlayer()
    {
        currentPlayerIndex = 1; // Move to the next player (in this case, the bot)
    }

    private void ResetGameState()
    {
        playerHand.Clear();
        botHand.Clear();
        communityCards.Clear();
        pot = 0;
        playerBet = 0;
        currentPlayerIndex = 0;
    }

    private void StartNewHand()
    {
        ShuffleDeck();
    }

    private int GetCallAmount()
    {
        // Calculate the call amount
        return previousBet - playerBet;
    }

    private int GetTotalBetAmount()
    {
        // Calculate the total bet amount
        return raiseAmount + GetCallAmount();
    }


    void AssignCardsToSlots(List<Card> hand, GameObject handObject)
    {
        Transform[] cardSlots = handObject.GetComponentsInChildren<Transform>(true); // Include inactive objects

        for (int i = 1; i < cardSlots.Length; i++)
        {
            SpriteRenderer slotSpriteRenderer = cardSlots[i].GetComponent<SpriteRenderer>();

            if (slotSpriteRenderer != null)
            {
                if (i - 1 < hand.Count)
                {
                    slotSpriteRenderer.sprite = cardSprites[GetCardIndex(hand[i - 1])];
                }
                else
                {
                    Debug.LogError("Invalid card index for slot: " + cardSlots[i].name);
                }
            }
            else
            {
                Debug.LogError("SpriteRenderer component not found on card slot: " + cardSlots[i].name);
            }
        }
    }

    int GetCardIndex(Card card)
    {
        int rankIndex = (int)card.rank;
        int suitIndex = (int)card.suit;

        if (rankIndex < 0 || rankIndex >= 8 || suitIndex < 0 || suitIndex >= 4)
        {
            Debug.LogError("Invalid card properties: Rank or suit out of range!");
            return -1; // Return an invalid index
        }

        int index = rankIndex + suitIndex * 8;

        //Debug.Log("Card Rank: " + card.rank + ", Card Suit: " + card.suit + ", Sprite Index: " + index);

        return index;
    }

    void InitialiseDeck()
    {
        int cardIndex = 0;

        foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
        {
            foreach (CardRank rank in Enum.GetValues(typeof(CardRank)))
            {
                // Instantiate card GameObject
                GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position + Vector3.right, Quaternion.identity);
                SpriteRenderer spriteRenderer = cardObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    // Set the sprite of the card GameObject
                    spriteRenderer.sprite = cardSprites[cardIndex];

                    // Get the Card component attached to the card GameObject
                    Card cardScript = cardObject.GetComponent<Card>();
                    if (cardScript != null)
                    {
                        // Initialize the Card component with the rank, suit, and sprite index
                        cardScript.rank = rank;
                        cardScript.suit = suit;
                        cardScript.colour = (suit == CardSuit.Diamonds || suit == CardSuit.Hearts) ? CardColour.Red : CardColour.Black;

                        // Add the card to the deck
                        deck.Add(cardScript);
                    }
                    else
                    {
                        Debug.LogError("Card component not found on card object!");
                    }
                }
                else
                {
                    Debug.LogError("SpriteRenderer component not found on card object!");
                }

                cardIndex++; // Move to the next card sprite
            }
        }
    }

    void ShuffleDeck()
    {
        System.Random rng = new System.Random();
        int n = deck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card value = deck[k];
            deck[k] = deck[n];
            deck[n] = value;
        }
    }

    void AssignSprites()
    {
        SpriteUpdater spriteUpdater = GetComponent<SpriteUpdater>();
        if (spriteUpdater != null)
        {
            foreach (Transform cardTransform in cardSpawnPoint)
            {
                SpriteRenderer spriteRenderer = cardTransform.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Card cardScript = cardTransform.GetComponent<Card>();
                    if (cardScript != null)
                    {
                        spriteUpdater.UpdateSprite(cardScript, spriteRenderer);
                    }
                    else
                    {
                        Debug.LogError("Card component not found on card object!");
                    }
                }
                else
                {
                    Debug.LogError("SpriteRenderer component not found on card object!");
                }
            }
        }
        else
        {
            Debug.LogError("SpriteUpdater component not found on GameManager!");
        }
    }

    void DealCards(int numCards, List<Card> hand)
    {
        for (int i = 0; i < numCards; i++)
        {
            if (deck.Count > 0)
            {
                Card dealtCard = deck[0];
                deck.RemoveAt(0);
                hand.Add(dealtCard);
            }
            else
            {
                Debug.LogWarning("Deck is empty, cannot deal more cards.");
                break;
            }
        }
    }
}