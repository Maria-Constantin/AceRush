using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum HandRank
{
    Unknown,
    HighCard,
    OnePair,
    TwoPair,
    ThreeOfAKind,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush,
    RoyalFlush
}

public class HandEvaluation : MonoBehaviour
{
    public HandRank EvaluateHandRank(List<Card> hand)
    {
        if (hand.Count < 5)
        {
            Debug.LogError("Hand does not have enough cards to evaluate hand rank.");
            return HandRank.Unknown;
        }

        hand.Sort((a, b) => b.rank.CompareTo(a.rank));

        if (IsRoyalFlush(hand))
            return HandRank.RoyalFlush;
        if (IsStraightFlush(hand))
            return HandRank.StraightFlush;
        if (IsFourOfAKind(hand))
            return HandRank.FourOfAKind;
        if (IsFullHouse(hand))
            return HandRank.FullHouse;
        if (IsFlush(hand))
            return HandRank.Flush;
        if (IsStraight(hand))
            return HandRank.Straight;
        if (IsThreeOfAKind(hand))
            return HandRank.ThreeOfAKind;
        if (IsTwoPair(hand))
            return HandRank.TwoPair;
        if (IsOnePair(hand))
            return HandRank.OnePair;

        return HandRank.HighCard;
    }

    private bool IsRoyalFlush(List<Card> hand)
    {
        return IsStraightFlush(hand) && hand.Any(card => card.rank == CardRank.Ace);
    }

    private bool IsStraightFlush(List<Card> hand)
    {
        return IsFlush(hand) && IsStraight(hand);
    }

    private bool IsFourOfAKind(List<Card> hand)
    {
        return hand.GroupBy(card => card.rank).Any(group => group.Count() == 4);
    }

    private bool IsFullHouse(List<Card> hand)
    {
        var groups = hand.GroupBy(card => card.rank);
        return groups.Any(group => group.Count() == 3) && groups.Any(group => group.Count() == 2);
    }

    private bool IsFlush(List<Card> hand)
    {
        return hand.GroupBy(card => card.suit).Any(group => group.Count() >= 5);
    }

    private bool IsStraight(List<Card> hand)
    {
        for (int i = 0; i < hand.Count - 4; i++)
        {
            bool isStraight = true;
            for (int j = i + 1; j < i + 5; j++)
            {
                if (hand[j].rank != hand[j - 1].rank - 1)
                {
                    isStraight = false;
                    break;
                }
            }
            if (isStraight)
                return true;
        }
        return false;
    }

    private bool IsThreeOfAKind(List<Card> hand)
    {
        return hand.GroupBy(card => card.rank).Any(group => group.Count() == 3);
    }

    private bool IsTwoPair(List<Card> hand)
    {
        int pairsCount = hand.GroupBy(card => card.rank).Count(group => group.Count() == 2);
        return pairsCount == 2;
    }

    private bool IsOnePair(List<Card> hand)
    {
        return hand.GroupBy(card => card.rank).Any(group => group.Count() == 2);
    }
}
