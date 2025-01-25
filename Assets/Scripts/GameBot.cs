using System.Collections.Generic;
using UnityEngine;

public class GameBot : MonoBehaviour
{
    public int currentBet;
    public int minimumBet;
    public int chips = 10000;

    public List<Card> hand;
    public HandEvaluation handEvaluation;

    private void Start()
    {
        handEvaluation = GetComponent<HandEvaluation>();
    }

    public void ReceiveHand(List<Card> cards)
    {
        hand = cards;
    }


    public void MakeDecision(int currentBet, int minimumBet)
    {
        this.currentBet = currentBet;
        this.minimumBet = minimumBet;

        HandRank handRank = handEvaluation.EvaluateHandRank(hand);

        switch (handRank)
        {
            case HandRank.HighCard:
                HandleHighCardDecision(); 
                break;
            case HandRank.OnePair:
                HandleOnePairDecision();
                break;
            case HandRank.TwoPair:
                HandleTwoPairDecision(); 
                break;
            case HandRank.ThreeOfAKind:
                HandleThreeOfAKindDecision();
                break;
            case HandRank.Straight:
                HandleStraightDecision();
                break;
            case HandRank.Flush:
                HandleFlushDecision(); 
                break;
            case HandRank.FullHouse:
                HandleFullHouseDecision(); 
                break;
            case HandRank.FourOfAKind:
                HandleFourOfAKindDecision();
                break;
            case HandRank.StraightFlush:
            case HandRank.RoyalFlush:
                HandleStraightFlushDecision();
                break;
        }
    }

    private void HandleHighCardDecision()
    {
        if (currentBet > 2 * minimumBet)
        {
            Fold();
        }
        else
        {
            CallOrRaiseRandomly();
        }
    }

    private void HandleOnePairDecision()
    {
        CallOrRaiseRandomly();
    }

    private void HandleTwoPairDecision()
    {
        if (currentBet > 3 * minimumBet)
        {
            Fold();
        }
        else
        {
            Raise();
        }
    }

    private void HandleThreeOfAKindDecision()
    {
        if (currentBet > 4 * minimumBet)
        {
            Call();
        }
        else
        {
            Raise();
        }
    }

    private void HandleStraightDecision()
    {
        Raise();
    }

    private void HandleFlushDecision()
    {
        Raise();
    }

    private void HandleFullHouseDecision()
    {
        Raise();
    }

    private void HandleFourOfAKindDecision()
    {
        Raise();
    }

    private void HandleStraightFlushDecision()
    {
        Raise();
    }

    private void CallOrRaiseRandomly()
    {
        if (Random.Range(0, 2) == 0)
        {
            Call();
        }
        else
        {
            Raise();
        }
    }

    private void Call()
    {
        if (chips >= currentBet)
        {
            chips -= currentBet;
        }
        else
        {
            Debug.Log("Bot doesn't have enough chips to call. Folding.");
            Fold();
        }
    }

    private void Raise()
    {
        int raiseAmount = currentBet + minimumBet;

        if (chips >= raiseAmount)
        {
            chips -= raiseAmount;
        }
        else
        {
            Debug.Log("Bot doesn't have enough chips to raise. Calling.");
            Call(); 
        }
    }

    public void Fold()
    {
        Debug.Log("Bot folds.");
    }

}
