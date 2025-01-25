using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public enum CardRank
{
    Seven,
    Eight, 
    Nine, 
    Ten, 
    Ace,
    Jack,
    King,
    Queen
}

public enum CardSuit
{
    Clubs,
    Diamonds,
    Hearts,
    Spades
}

public enum CardColour
{
    Red,
    Black
}

public class Card : MonoBehaviour
{
    public bool isFaceUp;
    public Sprite frontSprite;
    public Sprite backCardSprite;
    public SpriteRenderer spriteRenderer;

    public CardRank rank;
    public CardSuit suit;
    public CardColour colour;

    public Card(CardRank rank, CardSuit suit)
    {
        this.rank = rank;
        this.suit = suit;

        if (suit == CardSuit.Diamonds || suit == CardSuit.Hearts)
        {
            this.colour = CardColour.Red;
        }
        else
        {
            this.colour = CardColour.Black;
        }
    }
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    public void FlipCard()
    {
        isFaceUp = !isFaceUp;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isFaceUp ? frontSprite : backCardSprite;
        }
        else
        {
            Debug.LogWarning("SpriteRenderer is not set!");
        }
    }

}