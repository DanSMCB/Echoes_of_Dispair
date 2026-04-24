using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlagueEnemyManager : MonoBehaviour
{
    public List<GameObject> enemyDeck = new List<GameObject>();
    public BoardSlot[] enemySlots;

    [Header("AI Chances")]
    [Range(0f, 1f)] public float chanceToStartNewInfection = 0.4f;
    [Range(0f, 1f)] public float chanceToAttemptCombo = 0.3f;

    [Header("Card Placement")]
    public Vector3 cardLocalScale = new Vector3(3f, 3f, 3f);
    public Vector3 cardLocalRotation = new Vector3(-90f, -90f, 0f);

    public void PlayTurn()
    {
        if (enemyDeck.Count == 0)
        {
            Debug.Log("Plague enemy deck is empty.");
            TurnManager.Instance.EndEnemyTurn();
            return;
        }

        EnemyMove move = ChooseMove();

        if (move == null)
        {
            Debug.Log("Plague enemy found no valid move.");
            TurnManager.Instance.EndEnemyTurn();
            return;
        }

        ExecuteMove(move);

        TurnManager.Instance.EndEnemyTurn();
    }

    EnemyMove ChooseMove()
    {
        bool hasAnyInfectedCity = HasAnyInfectedCity();
        bool hasUninfectedCity = HasAnyUninfectedCityWithFreeSlot();

        if (!hasAnyInfectedCity)
        {
            return TryStartNewInfection();
        }

        if (hasUninfectedCity && Random.value < chanceToStartNewInfection)
        {
            EnemyMove newInfectionMove = TryStartNewInfection();
            if (newInfectionMove != null)
                return newInfectionMove;
        }

        if (Random.value < chanceToAttemptCombo)
        {
            EnemyMove comboMove = TryFindComboMove();
            if (comboMove != null)
                return comboMove;
        }

        EnemyMove buffMove = TryBuffExistingInfection();
        if (buffMove != null)
            return buffMove;

        EnemyMove fallbackNewInfection = TryStartNewInfection();
        if (fallbackNewInfection != null)
            return fallbackNewInfection;

        return null;
    }

    EnemyMove TryStartNewInfection()
    {
        GameObject plaguePrefab = FindCardInDeck(PlagueCardType.PlagueOfNosoi);
        if (plaguePrefab == null)
            return null;

        List<BoardSlot> validSlots = new List<BoardSlot>();

        foreach (BoardSlot slot in enemySlots)
        {
            if (!slot.IsEmpty())
                continue;

            if (slot.currentCard != null)
                continue;

            if (!slot.isFrontRow)
                continue;

            if (PlagueCityManager.Instance.CityHasBaseVirus(slot.cityIndex))
                continue;

            validSlots.Add(slot);
        }

        if (validSlots.Count == 0)
            return null;

        BoardSlot chosenSlot = validSlots[Random.Range(0, validSlots.Count)];

        return new EnemyMove
        {
            cardPrefab = plaguePrefab,
            targetSlot = chosenSlot
        };
    }

    EnemyMove TryBuffExistingInfection()
    {
        List<GameObject> buffCards = GetBuffCardsInDeck();

        if (buffCards.Count == 0)
            return null;

        List<BoardSlot> validSlots = new List<BoardSlot>();

        foreach (BoardSlot slot in enemySlots)
        {
            if (!slot.IsEmpty())
                continue;

            if (slot.currentCard != null)
                continue;

            if (!PlagueCityManager.Instance.CityHasBaseVirus(slot.cityIndex))
                continue;

            validSlots.Add(slot);
        }

        if (validSlots.Count == 0)
            return null;

        GameObject chosenCard = buffCards[Random.Range(0, buffCards.Count)];
        BoardSlot chosenSlot = validSlots[Random.Range(0, validSlots.Count)];

        return new EnemyMove
        {
            cardPrefab = chosenCard,
            targetSlot = chosenSlot
        };
    }

    EnemyMove TryFindComboMove()
    {
        foreach (BoardSlot slot in enemySlots)
        {
            if (!slot.IsEmpty())
                continue;

            if (slot.currentCard != null)
                continue;

            if (!PlagueCityManager.Instance.CityHasBaseVirus(slot.cityIndex))
                continue;

            List<CardData> cityCards = GetPlagueCardsInCity(slot.cityIndex);

            foreach (GameObject cardPrefab in enemyDeck)
            {
                CardData data = cardPrefab.GetComponent<CardData>();
                if (data == null || data.cardType != CardType.Plague)
                    continue;

                if (data.plagueCardType == PlagueCardType.PlagueOfNosoi)
                    continue;

                if (WouldCompletePlagueCombo(cityCards, data.plagueCardType))
                {
                    return new EnemyMove
                    {
                        cardPrefab = cardPrefab,
                        targetSlot = slot
                    };
                }
            }
        }

        return null;
    }

    bool WouldCompletePlagueCombo(List<CardData> cityCards, PlagueCardType newCard)
    {
        bool hasAirborne = HasPlagueCard(cityCards, PlagueCardType.AirborneSpread);
        bool hasUrban = HasPlagueCard(cityCards, PlagueCardType.UrbanTransmission);
        bool hasDelayed = HasPlagueCard(cityCards, PlagueCardType.DelayedSymptoms);
        bool hasPandemic = HasPlagueCard(cityCards, PlagueCardType.Pandemic);

        if (newCard == PlagueCardType.AirborneSpread && hasUrban)
            return true;

        if (newCard == PlagueCardType.UrbanTransmission && hasAirborne)
            return true;

        if (newCard == PlagueCardType.RapidMutation && hasPandemic)
            return true;

        if (newCard == PlagueCardType.DelayedSymptoms && !hasDelayed)
            return true;

        return false;
    }

    void ExecuteMove(EnemyMove move)
    {
        GameObject cardPrefab = move.cardPrefab;
        BoardSlot targetSlot = move.targetSlot;

        enemyDeck.Remove(cardPrefab);

        GameObject card = Instantiate(cardPrefab, targetSlot.transform);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.Euler(cardLocalRotation);
        card.transform.localScale = cardLocalScale;

        targetSlot.PlaceCard(card);

        CardData cardData = card.GetComponent<CardData>();
        if (cardData != null)
        {
            cardData.SetPlacedOnBoard(targetSlot);

            if (cardData.cardType == CardType.Plague)
            {
                PlagueCityManager.Instance.ResolvePlagueCard(cardData, targetSlot);
            }
        }

        Debug.Log("Plague god played " + cardData.cardName + " in city " + targetSlot.cityIndex);
    }

    GameObject FindCardInDeck(PlagueCardType type)
    {
        foreach (GameObject cardPrefab in enemyDeck)
        {
            CardData data = cardPrefab.GetComponent<CardData>();
            if (data != null && data.cardType == CardType.Plague && data.plagueCardType == type)
                return cardPrefab;
        }

        return null;
    }

    List<GameObject> GetBuffCardsInDeck()
    {
        List<GameObject> result = new List<GameObject>();

        foreach (GameObject cardPrefab in enemyDeck)
        {
            CardData data = cardPrefab.GetComponent<CardData>();
            if (data == null || data.cardType != CardType.Plague)
                continue;

            if (data.plagueCardType == PlagueCardType.PlagueOfNosoi)
                continue;

            if (data.plagueCardType == PlagueCardType.SilentSpread ||
                data.plagueCardType == PlagueCardType.Pandemic ||
                data.plagueCardType == PlagueCardType.GlobalPandemic)
                continue;

            result.Add(cardPrefab);
        }

        return result;
    }

    bool HasAnyInfectedCity()
    {
        for (int i = 0; i < PlagueCityManager.Instance.cities.Length; i++)
        {
            if (PlagueCityManager.Instance.CityHasBaseVirus(i))
                return true;
        }

        return false;
    }

    bool HasAnyUninfectedCityWithFreeSlot()
    {
        foreach (BoardSlot slot in enemySlots)
        {
            if (!slot.IsEmpty())
                continue;

            if (slot.currentCard != null)
                continue;

            if (!PlagueCityManager.Instance.CityHasBaseVirus(slot.cityIndex))
                return true;
        }

        return false;
    }

    List<CardData> GetPlagueCardsInCity(int cityIndex)
    {
        List<CardData> cards = new List<CardData>();

        foreach (BoardSlot slot in enemySlots)
        {
            if (slot.cityIndex != cityIndex)
                continue;

            if (slot.currentCard == null)
                continue;

            CardData data = slot.currentCard.GetComponent<CardData>();

            if (data != null && data.cardType == CardType.Plague)
                cards.Add(data);
        }

        return cards;
    }

    bool HasPlagueCard(List<CardData> cards, PlagueCardType type)
    {
        foreach (CardData card in cards)
        {
            if (card.plagueCardType == type)
                return true;
        }

        return false;
    }
}