using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    public LayerMask handCardLayer;
    public LayerMask boardSlotLayer;

    void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, handCardLayer))
        {
            CardData card = hit.transform.GetComponentInParent<CardData>();
            if (card != null && card.isInHand)
            {
                GameManager.Instance.SelectCard(card.gameObject);
                return;
            }
        }

        if (Physics.Raycast(ray, out hit, 100f, boardSlotLayer))
        {
            BoardSlot slot = hit.transform.GetComponent<BoardSlot>();
            if (slot != null && slot.isAvailable)
            {
                GameManager.Instance.PlaceSelectedCard(slot);
                return;
            }
        }

        if (GameManager.Instance.HasSelectedCard())
        {
            GameManager.Instance.CancelSelection();
        }
    }
}