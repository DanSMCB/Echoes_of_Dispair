using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public List<GameObject> enemyDeck;
    public BoardSlot[] enemySlots;

    public void PlayTurn()
    {
        Debug.Log("Inimigo a jogar");

        BoardSlot emptySlot = GetFirstEmptySlot();

        if (emptySlot != null && enemyDeck.Count > 0)
        {
            int randomIndex = Random.Range(0, enemyDeck.Count);
            GameObject cardPrefab = enemyDeck[randomIndex];
            enemyDeck.RemoveAt(randomIndex);

            GameObject card = Instantiate(cardPrefab);
            card.transform.SetParent(emptySlot.transform);
            card.transform.localPosition = Vector3.zero;
            card.transform.localRotation = Quaternion.Euler(-90f, -90f, 0f);
            card.transform.localScale = new Vector3(3f, 3f, 3f);

            emptySlot.PlaceCard(card);

            CardData data = card.GetComponent<CardData>();
            if (data != null)
                data.SetPlacedOnBoard(emptySlot);
        }

        TurnManager.Instance.EndEnemyTurn();
    }

    BoardSlot GetFirstEmptySlot()
    {
        foreach (BoardSlot slot in enemySlots)
        {
            if (slot.IsEmpty())
                return slot;
        }

        return null;
    }
}