using UnityEngine;

public class SpriteUpdater : MonoBehaviour
{
    public Sprite[] cardSprites; // Array of sprites for each card

    public void UpdateSprite(Card card, SpriteRenderer spriteRenderer)
    {
        int index = GetSpriteIndex(card);

        if (index >= 0 && index < cardSprites.Length)
        {
            spriteRenderer.sprite = cardSprites[index];
        }
        else
        {
            Debug.LogError("Sprite index out of range!");
        }
    }

    private int GetSpriteIndex(Card card)
    {
        int rankIndex = (int)card.rank - 7;
        int suitIndex = (int)card.suit;
        int index = rankIndex + suitIndex * 8;

        return index;
    }
}
