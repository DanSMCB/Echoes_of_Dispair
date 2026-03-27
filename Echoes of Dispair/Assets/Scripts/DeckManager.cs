using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeckManager : MonoBehaviour
{
    public List<GameObject> cardPrefabs = new List<GameObject>();
    public float cardSpacing = 0.01f;
    public HandManager handManager;
    public int startingCards = 5;
    private GameObject[] deckVisuals;

    void Start()
    {
        deckVisuals = new GameObject[cardPrefabs.Count];

        for (int i = 0; i < cardPrefabs.Count; i++)
        {
            GameObject card = Instantiate(cardPrefabs[i], transform);
            card.transform.localPosition = new Vector3(0, i * cardSpacing, 0);
            card.transform.localRotation = Quaternion.Euler(90f, -90f, 0f);
            deckVisuals[i] = card;
        }

        for (int i = 0; i < startingCards; i++)
        {
            DrawCard();
        }

        //ShuffleDeck();
    }

    /*void ShuffleDeck() {
        for (int i = 0; i < deck.Count; i++)
        {
            CardData temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }*/

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    DrawCard();
                }
            }
        }
    }

    public void DrawCard()
    {
        if (cardPrefabs.Count == 0)
            return;

        int randomIndex = UnityEngine.Random.Range(0, cardPrefabs.Count);
        GameObject drawnCardPrefab = cardPrefabs[randomIndex];
        handManager.AddCard(drawnCardPrefab);

        cardPrefabs.RemoveAt(randomIndex);

        int visualIndex = cardPrefabs.Count;
        if (visualIndex >= 0 && visualIndex < deckVisuals.Length)
        {
            deckVisuals[visualIndex].SetActive(false);
        }
    }
}
