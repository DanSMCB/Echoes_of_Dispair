using UnityEngine;
using UnityEngine.InputSystem;

public class CardHoverSystem : MonoBehaviour
{
    CardData currentHovered;
    public LayerMask cardLayer;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
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