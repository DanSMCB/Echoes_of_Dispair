using System.Collections.Generic;
using UnityEngine;

public class DisasterCombinationManager : MonoBehaviour
{
    public static DisasterCombinationManager Instance;

    private Dictionary<string, string> cardToElement = new Dictionary<string, string>();
    private Dictionary<string, string> combinationTable = new Dictionary<string, string>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        SetupCardMapping();
        SetupCombinations();
    }

    private void SetupCardMapping()
    {
        cardToElement["prometheus"] = "Fire";
        cardToElement["boreas"] = "Wind";
        cardToElement["enceladus"] = "Earth";
        cardToElement["naiads"] = "Water";
    }

    private void SetupCombinations()
    {
        AddCombination("Fire", "Water", "Extreme Humidity");
        AddCombination("Fire", "Wind", "Wild Fire");
        AddCombination("Water", "Water", "Flood");
        AddCombination("Water", "Earth", "Tsunami");
        AddCombination("Water", "Wind", "Tropical Storm");
        AddCombination("Wind", "Wind", "Weather Depression");
    }

    private void AddCombination(string a, string b, string result)
    {
        combinationTable[a + "_" + b] = result;
        combinationTable[b + "_" + a] = result;
    }

    public void CheckAdjacentCombination(BoardSlot placedSlot)
    {
        Debug.Log("checking.");

        if (placedSlot == null || placedSlot.currentCard == null)
            return;

        string placedSlotName = placedSlot.gameObject.name;

        if (!IsFrontRowSlot(placedSlotName))
            return;

        Debug.Log("name: " + placedSlotName);

        CardData placedCard = placedSlot.currentCard.GetComponent<CardData>();
        if (placedCard == null)
            return;

        Debug.Log(placedCard.cardName);

        BoardSlot adjacentSlot = GetAdjacentFrontSlotByName(placedSlotName);

        if (adjacentSlot == null || adjacentSlot.currentCard == null)
            return;

        Debug.Log(adjacentSlot);

        CardData adjacentCard = adjacentSlot.currentCard.GetComponent<CardData>();
        if (adjacentCard == null)
            return;

        string elementA = GetElementFromCardName(placedCard.cardName);
        string elementB = GetElementFromCardName(adjacentCard.cardName);

        Debug.Log(elementA + " | " + elementB);

        if (string.IsNullOrEmpty(elementA) || string.IsNullOrEmpty(elementB))
            return;

        string key = elementA + "_" + elementB;

        if (combinationTable.TryGetValue(key, out string combinationName))
        {
            Debug.Log($"Combinação ativada entre {placedCard.cardName} e {adjacentCard.cardName}: {combinationName}");
            ActivateEffect(combinationName, placedSlot, adjacentSlot);
        }
    }

    private string GetElementFromCardName(string cardName)
    {
        if (cardToElement.TryGetValue(cardName, out string element))
            return element;

        return null;
    }

    private bool IsFrontRowSlot(string slotName)
    {
        return slotName == "Slot1" ||
               slotName == "Slot2" ||
               slotName == "Slot5" ||
               slotName == "Slot6" ||
               slotName == "Slot9" ||
               slotName == "Slot10";
    }

    private BoardSlot GetAdjacentFrontSlotByName(string slotName)
    {
        string adjacentName = "";

        switch (slotName)
        {
            case "Slot1":
                adjacentName = "Slot2";
                break;
            case "Slot2":
                adjacentName = "Slot1";
                break;
            case "Slot5":
                adjacentName = "Slot6";
                break;
            case "Slot6":
                adjacentName = "Slot5";
                break;
            case "Slot9":
                adjacentName = "Slot10";
                break;
            case "Slot10":
                adjacentName = "Slot9";
                break;
            default:
                return null;
        }

        foreach (BoardSlot slot in GameManager.Instance.boardSlots)
        {
            if (slot != null && slot.gameObject.name == adjacentName)
                return slot;
        }

        return null;
    }

    private void ActivateEffect(string combinationName, BoardSlot slotA, BoardSlot slotB)
    {
        switch (combinationName)
        {
            case "Extreme Humidity":
                Debug.Log("Ativar efeito de Extreme Humidity");
                break;

            case "Wild Fire":
                Debug.Log("Ativar efeito de Wild Fire");
                break;

            case "Flood":
                Debug.Log("Ativar efeito de Flood");
                break;

            case "Tsunami":
                Debug.Log("Ativar efeito de Tsunami");
                break;

            case "Tropical Storm":
                Debug.Log("Ativar efeito de Tropical Storm");
                break;

            case "Weather Depression":
                Debug.Log("Ativar efeito de Weather Depression");
                break;

            default:
                Debug.Log("Combinação sem efeito definido: " + combinationName);
                break;
        }
    }
}