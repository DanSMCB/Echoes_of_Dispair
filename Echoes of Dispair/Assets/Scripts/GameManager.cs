using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public CameraChange cameraChange;
    public HandManager handManager;
    public BoardSlot[] boardSlots;

    private GameObject selectedCard = null;

    void Awake()
    {
        Instance = this;
    }

    public bool HasSelectedCard()
    {
        return selectedCard != null;
    }

    public GameObject GetSelectedCard()
    {
        return selectedCard;
    }

    public void SelectCard(GameObject card)
    {
        if (!TurnManager.Instance.isPlayerTurn)
            return;

        if (selectedCard != null)
            return;

        CardData cardData = card.GetComponent<CardData>();
        if (cardData != null)
            cardData.SetSelected(true);

        selectedCard = card;

        cameraChange.SwitchToBoardView();
        ShowAvailableSlots();
    }

    public void PlaceSelectedCard(BoardSlot slot)
    {
        if (selectedCard == null)
            return;

        if (slot == null || !slot.IsEmpty())
            return;

        selectedCard.transform.SetParent(slot.transform);
        selectedCard.transform.localPosition = Vector3.zero;
        selectedCard.transform.localRotation = Quaternion.Euler(-90f, 90f, 0f);
        selectedCard.transform.localScale = new Vector3(3f, 3f, 3f);

        CardData cardData = selectedCard.GetComponent<CardData>();
        if (cardData != null) { 
            cardData.SetPlacedOnBoard(slot);
            cardData.SetSelected(false);
        }
            

        slot.PlaceCard(selectedCard);

        selectedCard = null;

        HideAllSlots();
        handManager.RefreshHand();
        cameraChange.SwitchToPlayerView();
    }

    public void CancelSelection()
    {
        if (selectedCard != null)
        {
            CardData cardData = selectedCard.GetComponent<CardData>();
            if (cardData != null)
                cardData.SetSelected(false);
        }

        selectedCard = null;
        HideAllSlots();
        cameraChange.SwitchToPlayerView();
    }

    private void ShowAvailableSlots()
    {
        foreach (BoardSlot slot in boardSlots)
        {
            slot.SetAvailable(slot.IsEmpty());
        }
    }

    private void HideAllSlots()
    {
        foreach (BoardSlot slot in boardSlots)
        {
            slot.SetAvailable(false);
        }
    }
}