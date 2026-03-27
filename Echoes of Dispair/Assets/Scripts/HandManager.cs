using UnityEngine;

public class HandManager : MonoBehaviour
{
    private HandDisplay handDisplay;

    void Start()
    {
        handDisplay = GetComponent<HandDisplay>();
    }

    public void AddCard(GameObject cardPrefab)
    {
        GameObject card = Instantiate(cardPrefab, transform);
        card.transform.localScale = Vector3.one;

        CardData cardData = card.GetComponent<CardData>();
        if (cardData != null)
            cardData.SetBackToHand();

        handDisplay.UpdateHand();
    }

    public void RemoveCard(GameObject card)
    {
        Destroy(card);
        handDisplay.UpdateHand();
    }

    public void RefreshHand()
    {
        handDisplay.UpdateHand();
    }
}