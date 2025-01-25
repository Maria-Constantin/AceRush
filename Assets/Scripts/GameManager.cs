using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    PreFlop,
    Flop,
    Turn,
    River,
    Showdown
}

public class GameManager : MonoBehaviour
{
    public Sprite backCardSprite;
    public List<Card> deck = new List<Card>();

    public List<Card> playerHand = new List<Card>();
    public List<Card> botHand = new List<Card>();
    public List<Card> botHand2 = new List<Card>();
    public List<Card> botHand3 = new List<Card>();
    public List<Card> botHand4 = new List<Card>();
    public List<Card> botHand5 = new List<Card>();

    public List<Card> communityCards = new List<Card>();

    public GameObject playerHandObject;
    public GameObject botHandObject;
    public GameObject botHand2Object;
    public GameObject botHand3Object;
    public GameObject botHand4Object;
    public GameObject botHand5Object;
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

    public int pot = 0;
    public TMP_Text potTextField;
   
    public int playerBet = 1000;
    public int playerChips = 10000;
    public TMP_Text playerChipsText;

    public int raiseAmount = 2000;
    public int previousBet = 0;

    private int currentBet;
    private int minimumBet;

    public int blindAmount = 1000;

    public int currentPlayerIndex = 0;

    public List<List<Card>> allPlayers = new List<List<Card>>();
    public List<GameBot> bots = new List<GameBot>();

    public GameState currentGameState;
    public int currentTurn;

    private void Start()
    {
        InitialiseDeck();
        ShuffleDeck();
        AssignSprites();

        UpdatePotText();

        UpdateUserData();
        AddPlayerHands();

        currentGameState = GameState.PreFlop;
        currentTurn = 0;
;
        PerformBlindBetting();
        PreFlop();
    }

    void UpdatePotText()
    {
        if (potTextField != null)
        {
            potTextField.text = $"Pot: G {pot}";
        }
    }

    private void UpdatePlayerChipsText()
    {
        if (playerChipsText != null) 
        {
            playerChipsText.text = $"G {playerChips}";
        }
        else
        {
            Debug.LogError("Player chips text field is not assigned.");
        }
    }

    public void UpdateUserData()
    {
        gameData.UpdatePlayerUserData();
        gameData.UpdateBotUserData(gameData.botusername, gameData.botcurrency);
        gameData.UpdateBotUserData(gameData.botusername2, gameData.botcurrency2);
        gameData.UpdateBotUserData(gameData.botusername3, gameData.botcurrency3);
        gameData.UpdateBotUserData(gameData.botusername4, gameData.botcurrency4);
        gameData.UpdateBotUserData(gameData.botusername5, gameData.botcurrency5);
    }

    public void AddPlayerHands()
    {
        allPlayers.Add(playerHand);
        allPlayers.Add(botHand);
        allPlayers.Add(botHand2);
        allPlayers.Add(botHand3);
        allPlayers.Add(botHand4);
        allPlayers.Add(botHand5);
    }

    private void InitialiseButtons()
    {
        callButton.onClick.RemoveAllListeners();

        callButton.onClick.AddListener(Call);
        raiseButton.onClick.AddListener(Raise);
        foldButton.onClick.AddListener(Fold);
    }


    private void PreFlop()
    {
        communityCards.Clear();
        CardsToHands();

        playerBet = 0;
        currentPlayerIndex = 1;

        playerChips -= blindAmount;
        pot += blindAmount;
        playerBet = blindAmount;

        UpdatePotText();
        Flop();
    }

    public void CardsToHands()
    {
        DealCards(2, playerHand);
        DealCards(2, botHand);
        DealCards(2, botHand2);
        DealCards(2, botHand3);
        DealCards(2, botHand4);
        DealCards(2, botHand5);

        AssignCardsToSlots(playerHand, playerHandObject, false);
        AssignCardsToSlots(botHand, botHandObject, true);
        AssignCardsToSlots(botHand2, botHand2Object, true);
        AssignCardsToSlots(botHand3, botHand3Object, true);
        AssignCardsToSlots(botHand4, botHand4Object, true);
        AssignCardsToSlots(botHand5, botHand5Object, true);
        AssignCardsToSlots(communityCards, communityCardsObject, false);
    }

    public void StartBettingRound()
    {
        while (currentTurn < allPlayers.Count)
        {
            if (currentTurn == 0)
            {
                // Player action
                InitialiseButtons();
            }
            else // Bot turn
            {
                GameBot bot = bots[currentTurn - 1];
                bot.MakeDecision(currentBet, minimumBet);
            }

            currentTurn++; 
        }

        if (currentTurn >= allPlayers.Count)
        {
            currentTurn = 0; 
            ProgressGameState();
        }
    }

    private void ProgressGameState()
    {
        switch (currentGameState)
        {
            case GameState.PreFlop:
                currentGameState = GameState.Flop;
                Flop();
                break;

            case GameState.Flop:
                currentGameState = GameState.Turn;
                Turn();
                break;

            case GameState.Turn:
                currentGameState = GameState.River;
                River();
                break;

            case GameState.River:
                currentGameState = GameState.Showdown;
                DetermineWinner();
                break;

            case GameState.Showdown:
                StartNewHand();
                break;
        }
    }

    private void Flop()
    {
        DealCards(3, communityCards);

        currentPlayerIndex = 0;
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

    public void DetermineWinner()
    {
        handEvaluation = GetComponent<HandEvaluation>();

        HandRank playerHandRank = handEvaluation.EvaluateHandRank(communityCards.Concat(playerHand).ToList());
        HandRank botHandRank = handEvaluation.EvaluateHandRank(communityCards.Concat(botHand).ToList());
        HandRank botHandRank2 = handEvaluation.EvaluateHandRank(communityCards.Concat(botHand2).ToList());
        HandRank botHandRank3 = handEvaluation.EvaluateHandRank(communityCards.Concat(botHand3).ToList());
        HandRank botHandRank4 = handEvaluation.EvaluateHandRank(communityCards.Concat(botHand4).ToList());
        HandRank botHandRank5 = handEvaluation.EvaluateHandRank(communityCards.Concat(botHand5).ToList());
    }

    public void Call()
    {
        int callAmount = 1000;

        if (playerChips >= callAmount)
        {
            playerChips -= callAmount;
            pot += callAmount;
            UpdatePlayerChipsText();
            Debug.Log($"Player called for 1,000 chips. Pot is now {pot}.");
        }
        else
        {
            Debug.Log("Player doesn't have enough chips to call.");
        }
    }

    private int GetCallAmount()
    {
        return previousBet - playerBet;
    }


    public void Raise()
    {
        int totalBet = GetTotalBetAmount();

        if (playerChips >= totalBet)
        {
            playerChips -= totalBet;
            pot += totalBet;
            previousBet = totalBet;

            UpdatePlayerChipsText();
            UpdatePotText();
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

        StartNewHand();
    }


    private void PerformBlindBetting()
    {
        playerChips -= blindAmount;
        pot += blindAmount;

        Debug.Log($"Player's chips after blind deduction: {playerChips}");
        UpdatePotText();
    }

    public void ClearHands()
    {
        ShuffleDeck();
        playerHand.Clear();
        botHand.Clear();
        botHand2.Clear();
        botHand3.Clear();
        botHand4.Clear();
        botHand5.Clear();
        communityCards.Clear();
    }

    private void StartNewHand()
    {
        ShuffleDeck();
        PreFlop();
    }

    private int GetTotalBetAmount()
    {
        return raiseAmount + GetCallAmount();
    }


    int GetCardIndex(Card card)
    {
        int rankIndex = (int)card.rank;
        int suitIndex = (int)card.suit;

        if (rankIndex < 0 || rankIndex >= 8 || suitIndex < 0 || suitIndex >= 4)
        {
            Debug.LogError("Invalid card properties: Rank or suit out of range!");
            return -1;
        }

        int index = rankIndex + suitIndex * 8;

        return index;
    }

    void InitialiseDeck()
    {
        deck.Clear();
        int cardIndex = 0;

        foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
        {
            foreach (CardRank rank in Enum.GetValues(typeof(CardRank)))
            {
                GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position + Vector3.right, Quaternion.identity);
                SpriteRenderer spriteRenderer = cardObject.GetComponent<SpriteRenderer>();
                Card cardScript = cardObject.GetComponent<Card>();

                if (cardScript != null)
                {
                    cardScript.rank = rank;
                    cardScript.suit = suit;
                    cardScript.colour = (suit == CardSuit.Diamonds || suit == CardSuit.Hearts) ? CardColour.Red : CardColour.Black;

                    //Debug.Log($"Created card with rank: {rank}, suit: {suit}, color: {cardScript.colour}");

                    deck.Add(cardScript);
                }
                else
                {
                    Debug.LogError("Card component not found on card object!");
                }

                cardIndex++;
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

    void DealCards(int numCards, List<Card> hand)
    {
        for (int i = 0; i < numCards; i++)
        {
            if (deck.Count > 0)
            {
                Card dealtCard = deck[0];
                deck.RemoveAt(0);
                hand.Add(dealtCard);
                //Debug.Log($"Dealt {dealtCard.rank} of {dealtCard.suit} to hand.");
            }
            else
            {
                //Debug.LogWarning("Deck is empty, cannot deal more cards.");
                break;
            }
        }

        //Debug.Log("Hand now contains:");
        //foreach (var card in hand)
        //{
        //    Debug.Log($"{card.rank} of {card.suit}");
        //}
    }
    void AssignCardsToSlots(List<Card> hand, GameObject handObject, bool isBot)
    {
        Transform[] cardSlots = handObject.GetComponentsInChildren<Transform>(true);

        for (int i = 1; i < cardSlots.Length; i++)
        {
            SpriteRenderer slotSpriteRenderer = cardSlots[i].GetComponent<SpriteRenderer>();

            if (slotSpriteRenderer != null)
            {
                if (i - 1 < hand.Count)
                {
                    if (isBot)
                    {
                        slotSpriteRenderer.sprite = backCardSprite;
                    }
                    else
                    {
                        slotSpriteRenderer.sprite = cardSprites[GetCardIndex(hand[i - 1])];
                    }
                }
                else
                {
                    slotSpriteRenderer.sprite = null; // Clear unused slots
                }
            }
            else
            {
                Debug.LogError($"SpriteRenderer not found in slot {cardSlots[i].name}");
            }
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
}