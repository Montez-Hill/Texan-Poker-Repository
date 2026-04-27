using System.Collections.Generic;
using UnityEngine;

public class HandEvaluator
{
    public static string EvaluateHand(List<CardData> cards)
    {
        if (cards == null || cards.Count < 5)
        {
            return "Not enough cards to evaluate";
        }

        Dictionary<int, int> rankCounts = new Dictionary<int, int>();
        Dictionary<Suit, int> suitCounts = new Dictionary<Suit, int>();
        List<int> ranks = new List<int>();

        for (int i = 0; i < cards.Count; i++)
        {
            int rank = cards[i].rank;
            Suit suit = cards[i].suit;
            if (rankCounts.ContainsKey(rank))
            {
                rankCounts[rank]++;
            }
            else
            {
                rankCounts.Add(rank, 1);
            }

            if (suitCounts.ContainsKey(suit)) {
                suitCounts[suit]++;
            }
            else
            {
                suitCounts.Add(suit, 1);
            }

            if (!ranks.Contains(rank)) {
                ranks.Add(rank);
            }

        }
        ranks.Sort();

        bool flush = false;
        foreach (var suitE in suitCounts)
        {
            if (suitE.Value >= 5)
            {
                flush = true;
                break;
            }
        }

        bool straight = HasStraight(ranks);
        int pairs = 0;
        bool threeOfKind = false;
        bool fourOfKind = false;

        foreach (var rankE in rankCounts)
        {
            if (rankE.Value == 2)
                pairs++;
            else if (rankE.Value == 3)
                threeOfKind = true;
            else if (rankE.Value == 4)
                fourOfKind = true;
        }

        if (straight && flush)
        {
            return "Straight Flush";
        }
        if (fourOfKind)
        {
            return "Four of a Kind";
        }
        if (threeOfKind && pairs >= 1)
        {
            return "Full House";
        }
        if (flush)
        {
            return "Flush";
        }
        if (straight)
        {
            return "Straight";
        }
        if (threeOfKind)
        {
            return "Three of a Kind";
        }
        if (pairs >= 2)
        {
            return "Two Pair";
        }
        if (pairs == 1)
        {
            return "Pair";
        }
        return "High Card";
    }
    private static bool HasStraight(List<int> ranks)
    {
        if (ranks.Count < 5)
        {
            return false;
        }
        int count = 1;
        for (int i = 1; i < ranks.Count; i++)
        {
            if (ranks[i] == ranks[i - 1] + 1)
            {
                count++;
                if (count >= 5)
                {
                    return true;
                }
            }
            else if (ranks[i] != ranks[i - 1])
            {
                count = 1;
            }
        }
        if (ranks.Contains(14) && ranks.Contains(2) && ranks.Contains(3) && ranks.Contains(4) && ranks.Contains(5))
        {
            return true; // Ace-low straight
        }
        return false;
    }
}
