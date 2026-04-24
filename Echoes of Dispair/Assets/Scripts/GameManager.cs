using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public CameraChange cameraChange;
    public HandManager handManager;
    public BoardSlot[] boardSlots;

    private GameObject selectedCard = null;

    private bool isEvacuationMode = false;
    private int evacuationSourceCityIndex = -1;
    private GameObject selectedPopulationToMove = null;
    private GameObject pendingEvacuationCard = null;

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
        if (TurnManager.Instance != null && TurnManager.Instance.gameEnded)
            return;

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

        GameObject cardToPlace = selectedCard;

        selectedCard.transform.SetParent(slot.transform);
        selectedCard.transform.localPosition = Vector3.zero;
        selectedCard.transform.localRotation = Quaternion.Euler(-90f, 90f, 0f);
        selectedCard.transform.localScale = new Vector3(3f, 3f, 3f);

        CardData cardData = selectedCard.GetComponent<CardData>();
        if (cardData != null) { 
            cardData.SetPlacedOnBoard(slot);
            cardData.SetSelected(false);

            if (cardData.cardType == CardType.Human)
            {
                if (cardData.humanCardType == HumanCardType.Evacuation)
                {
                    if (CityManager.Instance.IsBlackoutActive(slot.cityIndex))
                    {

                        if (InstructionUI.Instance != null)
                        {
                            InstructionUI.Instance.ShowInstruction("Evacuation is unavailable in this city due to a blackout.");
                            cardData.SetBackToHand();
                        }

                        return;
                    }

                    slot.PlaceCard(cardToPlace);

                    selectedCard = null;
                    HideAllSlots();
                    handManager.RefreshHand();

                    StartEvacuationMode(cardToPlace, slot.cityIndex);
                    return;
                }
                else
                {
                    bool wasEffective = false;

                    if (TurnManager.Instance.battleMode == BattleMode.Nature)
                    {
                        wasEffective = CityManager.Instance.WasHumanCardEffective(cardData, slot.cityIndex);
                        CityManager.Instance.ResolveHumanCard(cardData, slot.cityIndex);
                    }
                    else if (TurnManager.Instance.battleMode == BattleMode.Plague)
                    {
                        wasEffective = PlagueCityManager.Instance.WasHumanPlagueCardEffective(cardData, slot.cityIndex);
                        PlagueCityManager.Instance.ResolveHumanPlagueCard(cardData, slot.cityIndex);
                    }

                    if (LearningTracker.Instance != null)
                    {
                        LearningTracker.Instance.RegisterCardPlay(wasEffective);
                    }

                    slot.PlaceCard(cardToPlace);
                    TurnManager.Instance.playerCardsPlayedThisTurn++;
                    CheckAutoEndTurn();

                    selectedCard = null;
                    HideAllSlots();
                    handManager.RefreshHand();
                    return;
                }
            }
        }
            
        selectedCard = null;
    }

    public List<CardData> GetCardsInCity(int cityIndex)
    {
        List<CardData> cards = new List<CardData>();

        foreach (BoardSlot slot in boardSlots)
        {
            if (slot.cityIndex != cityIndex)
                continue;

            if (slot.currentCard == null)
                continue;

            CardData card = slot.currentCard.GetComponent<CardData>();
            if (card != null)
                cards.Add(card);
        }

        return cards;
    }

    public void CancelSelection()
    {
        if (isEvacuationMode)
        {
            isEvacuationMode = false;
            evacuationSourceCityIndex = -1;
            selectedPopulationToMove = null;

            if (pendingEvacuationCard != null)
            {
                CardData evacCardData = pendingEvacuationCard.GetComponent<CardData>();
                if (evacCardData != null && evacCardData.currentSlot != null)
                {
                    evacCardData.currentSlot.RemoveCard();
                }

                Destroy(pendingEvacuationCard);
                pendingEvacuationCard = null;
            }

            HideAllSlots();
            InstructionUI.Instance.ClearInstruction();
            return;
        }

        if (selectedCard != null)
        {
            CardData cardData = selectedCard.GetComponent<CardData>();
            if (cardData != null)
                cardData.SetSelected(false);
        }

        selectedCard = null;
        HideAllSlots();
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

    void CheckAutoEndTurn()
    {
        if (TurnManager.Instance == null)
            return;

        if (TurnManager.Instance.playerCardsPlayedThisTurn >= TurnManager.Instance.maxCardsPerTurn)
        {
            TurnManager.Instance.EndPlayerTurn();
        }
    }


    //Evacuation ----------------------------------------------
    void StartEvacuationMode(GameObject evacuationCard, int sourceCityIndex)
    {
        isEvacuationMode = true;
        evacuationSourceCityIndex = sourceCityIndex;
        selectedPopulationToMove = null;
        pendingEvacuationCard = evacuationCard;

        cameraChange.SwitchToBoardView();

        InstructionUI.Instance.ShowInstruction("Select a population card you want to evacuate.");
    }

    public void SelectPopulationForEvacuation(GameObject populationCard)
    {
        if (!isEvacuationMode)
            return;

        CardData cardData = populationCard.GetComponent<CardData>();
        if (cardData == null || cardData.cardType != CardType.Population)
            return;

        BoardSlot slot = cardData.currentSlot;
        if (slot == null)
            return;

        if (slot.cityIndex != evacuationSourceCityIndex)
        {
            return;
        }

        selectedPopulationToMove = populationCard;

        ShowEvacuationDestinationSlots();
        InstructionUI.Instance.ShowInstruction("Select a slot in the city you want the population to evacuate to.");
    }

    void ShowEvacuationDestinationSlots()
    {
        foreach (BoardSlot slot in boardSlots)
        {
            bool validDestination =
                slot.IsEmpty() &&
                slot.cityIndex != evacuationSourceCityIndex &&
                !slot.belongsToEnemy;

            slot.SetAvailable(validDestination);
        }
    }

    public void PlaceEvacuatedPopulation(BoardSlot destinationSlot)
    {
        if (!isEvacuationMode)
            return;

        if (selectedPopulationToMove == null)
        {
            return;
        }

        if (destinationSlot == null || !destinationSlot.IsEmpty())
            return;

        if (destinationSlot.cityIndex == evacuationSourceCityIndex)
        {
            InstructionUI.Instance.ShowInstruction("The evacuation must be executed to another city.");
            return;
        }

        int sourceCity = evacuationSourceCityIndex;
        int destinationCity = destinationSlot.cityIndex;

        int sourceElevation = CityManager.Instance.GetCityElevation(sourceCity);
        int destinationElevation = CityManager.Instance.GetCityElevation(destinationCity);

        bool tsunamiThreat = CityManager.Instance.CityHasTsunamiThreat(sourceCity);

        if (tsunamiThreat && destinationElevation < sourceElevation)
        {
            CityManager.Instance.RegisterEvacuationRisk(
                selectedPopulationToMove,
                sourceCity,
                destinationCity,
                DisasterType.Tsunami,
                0.4f
            );
        }


        CardData populationData = selectedPopulationToMove.GetComponent<CardData>();
        BoardSlot oldSlot = populationData.currentSlot;

        if (oldSlot != null)
            oldSlot.RemoveCard();

        selectedPopulationToMove.transform.SetParent(destinationSlot.transform);
        selectedPopulationToMove.transform.localPosition = Vector3.zero;
        selectedPopulationToMove.transform.localRotation = Quaternion.Euler(-90f, 90f, 0f);
        selectedPopulationToMove.transform.localScale = new Vector3(3f, 3f, 3f);

        destinationSlot.PlaceCard(selectedPopulationToMove);
        populationData.SetPlacedOnBoard(destinationSlot);

        if (pendingEvacuationCard != null)
        {
            CardData evacCardData = pendingEvacuationCard.GetComponent<CardData>();
            if (evacCardData != null && evacCardData.currentSlot != null)
            {
                evacCardData.currentSlot.RemoveCard();
            }

            Destroy(pendingEvacuationCard);
        }

        EndEvacuationMode();
    }

    void EndEvacuationMode()
    {
        isEvacuationMode = false;
        evacuationSourceCityIndex = -1;
        selectedPopulationToMove = null;
        pendingEvacuationCard = null;

        TurnManager.Instance.playerCardsPlayedThisTurn++;
        CheckAutoEndTurn();

        HideAllSlots();
        handManager.RefreshHand();

        InstructionUI.Instance.ClearInstruction();
    }

    public bool IsEvacuationMode()
    {
        return isEvacuationMode;
    }
}