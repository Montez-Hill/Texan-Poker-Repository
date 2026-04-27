using UnityEngine;

public enum Suit { Clubs, Diamonds, Hearts, Spades }

[System.Serializable]
public struct CardData
{
    public Suit suit;
    public int rank;          // 2..14 (11=J, 12=Q, 13=K, 14=Ace)
    public Sprite faceSprite; // image for this card
}
