using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public List<CardData> cards = new List<CardData>(52);

    private readonly List<CardData> _deck = new List<CardData>();

    public void ResetDeck()
    {
        _deck.Clear();
        _deck.AddRange(cards);

        if (_deck.Count == 0)
        {
            Debug.LogError("DeckManager: 'cards' list is empty. Add 52 cards in the Inspector.");
            return;
        }

        Shuffle();
    }

    public void Shuffle()
    {
        for (int i = 0; i < _deck.Count; i++)
        {
            int j = Random.Range(i, _deck.Count);
            (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
        }
    }

    public CardData Draw()
    {
        if (_deck.Count == 0)
        {
            Debug.LogWarning("Deck empty. Resetting deck.");
            ResetDeck();
        }

        CardData top = _deck[^1];
        _deck.RemoveAt(_deck.Count - 1);
        return top;
    }

    public int CardsRemaining() => _deck.Count;
}
