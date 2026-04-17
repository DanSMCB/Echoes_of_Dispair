using Unity.VisualScripting;
using UnityEngine;

public class CardData : MonoBehaviour
{
    public Sprite cardSprite;
    public string cardName;

    public int currentHealth = 0;
    public int maxHealth = 0;

    public CardType cardType;
    public NatureCardType natureCardType;
    public HumanCardType humanCardType;

    private bool isHovered = false;

    [HideInInspector] public bool isInHand = true;
    [HideInInspector] public BoardSlot currentSlot = null;
    public GameObject selectionHighlight;

    public void SetHover(bool state)
    {
        isHovered = state;

        if (state)
        {
            CardPreview.Instance.ShowCard(cardSprite);
            transform.localPosition = transform.localPosition + new Vector3(0, 0.1f, 0);
        }
        else 
        { 
            CardPreview.Instance.HideCard();
            transform.localPosition = transform.localPosition - new Vector3(0, 0.1f, 0);
        }
            
    }

    public void SetPlacedOnBoard(BoardSlot slot)
    {
        isInHand = false;
        currentSlot = slot;
    }

    public void SetBackToHand()
    {
        isInHand = true;
        currentSlot = null;
    }

    public void SetSelected(bool value)
    {
        if (selectionHighlight != null)
            selectionHighlight.SetActive(value);
    }
}