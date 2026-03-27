using UnityEngine;

public class BoardSlot : MonoBehaviour
{
    public int slotIndex;
    public GameObject highlightVisual;

    [HideInInspector] public bool isAvailable = false;
    [HideInInspector] public GameObject currentCard = null;

    public bool IsEmpty()
    {
        return currentCard == null;
    }

    public void SetAvailable(bool value)
    {
        isAvailable = value;

        if (highlightVisual != null)
            highlightVisual.SetActive(value);
    }

    public void PlaceCard(GameObject card)
    {
        currentCard = card;
    }

    public void RemoveCard()
    {
        currentCard = null;
    }
}