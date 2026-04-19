using UnityEngine;
using UnityEngine.InputSystem;

public class CardHoverSystem : MonoBehaviour
{
    CardData currentHovered;
    public LayerMask cardLayer;

    public LayerMask ignoreLayer;

    void Update()
    {
        if (TurnManager.Instance != null && TurnManager.Instance.gameEnded)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());


        int layerMask = ~ignoreLayer;

        if (Physics.Raycast(ray, out RaycastHit hit, 20f, layerMask))
        {
            CardData card = hit.transform.GetComponent<CardData>();

            if (card != null)
            {
                if (currentHovered != card)
                {
                    if (currentHovered != null)
                        currentHovered.SetHover(false);

                    currentHovered = card;
                    currentHovered.SetHover(true);
                }

                return;
            }
        }

        if (currentHovered != null)
        {
            currentHovered.SetHover(false);
            currentHovered = null;
        }
    }
}