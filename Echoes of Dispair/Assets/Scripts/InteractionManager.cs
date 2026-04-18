using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    public LayerMask handCardLayer;
    public LayerMask boardSlotLayer;
    public LayerMask populationLayer;

    void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (GameManager.Instance.IsEvacuationMode())
        {
            if (Physics.Raycast(ray, out RaycastHit hitPop, 100f, populationLayer))
            {
                CardData popCard = hitPop.transform.GetComponentInParent<CardData>();
                if (popCard != null && popCard.cardType == CardType.Population)
                {
                    GameManager.Instance.SelectPopulationForEvacuation(popCard.gameObject);
                    return;
                }
            }

            if (Physics.Raycast(ray, out RaycastHit hitSlot, 100f, boardSlotLayer))
            {
                BoardSlot slot = hitSlot.transform.GetComponentInParent<BoardSlot>();
                if (slot != null && slot.isAvailable)
                {
                    GameManager.Instance.PlaceEvacuatedPopulation(slot);
                    return;
                }
            }

            return;
        }

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