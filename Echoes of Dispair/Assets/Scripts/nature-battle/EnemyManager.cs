using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public List<GameObject> enemyDeck;
    public BoardSlot[] enemySlots;

    public void PlayTurn()
    {
        if (enemyDeck.Count == 0)
        {
            TurnManager.Instance.EndEnemyTurn();
            return;
        }

        EnemyMove chosenMove = ChooseBestMove();

        if (chosenMove == null)
        {
            Debug.Log("Enemy found no valid move.");
            TurnManager.Instance.EndEnemyTurn();
            return;
        }

        ExecuteMove(chosenMove);

        TurnManager.Instance.EndEnemyTurn();
    }

    EnemyMove ChooseBestMove()
    {
        EnemyMove move;

        if (Random.value < 0.3f)
        {
            move = TryFindComboPlusModifierMove();
            if (move != null) return move;
        }

        if (Random.value < 0.3f)
        {
            move = TryFindComboMove();
            if (move != null) return move;
        }

        move = TryFindBaseElementMove();
        if (move != null) return move;

        return null;
    }

    void ExecuteMove(EnemyMove move)
    {
        GameObject cardPrefab = move.cardPrefab;
        BoardSlot targetSlot = move.targetSlot;

        enemyDeck.Remove(cardPrefab);

        GameObject card = Instantiate(cardPrefab, targetSlot.transform);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.Euler(-90f, -90f, 0f);
        card.transform.localScale = new Vector3(3f, 3f, 3f);

        targetSlot.PlaceCard(card);

        CardData cardData = card.GetComponent<CardData>();
        if (cardData != null)
        {
            cardData.SetPlacedOnBoard(targetSlot);

            if (cardData.cardType == CardType.Nature)
            {
                CityManager.Instance.ResolveNaturePlay(cardData, targetSlot);
            }
        }

        Debug.Log("Enemy played " + cardData.cardName + " in city " + targetSlot.cityIndex);
    }

    EnemyMove TryFindComboPlusModifierMove()
    {
        foreach (BoardSlot slot in enemySlots)
        {
            if (!slot.IsEmpty())
                continue;

            if (!slot.isBackRow)
                continue;

            int cityIndex = slot.cityIndex;

            List<CardData> cityCards = CityManager.Instance.GetNatureCardsInCity(cityIndex, true);

            bool hasScourge = HasNatureCard(cityCards, NatureCardType.ScourgeOfHelios);
            bool hasEye = HasNatureCard(cityCards, NatureCardType.EyeOfHuracan);
            bool hasFlame = HasNatureCard(cityCards, NatureCardType.FlameOfPrometheus);
            bool hasFury = HasNatureCard(cityCards, NatureCardType.FuryOfEnceladus);

            foreach (GameObject cardPrefab in enemyDeck)
            {
                CardData data = cardPrefab.GetComponent<CardData>();
                if (data == null) continue;

                if ((hasScourge || hasFlame) && data.natureCardType == NatureCardType.ParchedEarth)
                    return new EnemyMove { cardPrefab = cardPrefab, targetSlot = slot };

                if (hasEye && data.natureCardType == NatureCardType.RapidIntensification)
                    return new EnemyMove { cardPrefab = cardPrefab, targetSlot = slot };

                if (hasFury && data.natureCardType == NatureCardType.SeismicEchoes)
                    return new EnemyMove { cardPrefab = cardPrefab, targetSlot = slot };
            }
        }

        return null;
    }

    EnemyMove TryFindComboMove()
    {
        foreach (GameObject cardPrefab in enemyDeck)
        {
            CardData data = cardPrefab.GetComponent<CardData>();
            if (data == null) continue;

            foreach (BoardSlot slot in enemySlots)
            {
                if (!slot.IsEmpty())
                    continue;

                if (IsBaseElementCard(data.natureCardType) && !slot.isFrontRow)
                    continue;

                if (IsModifierCard(data.natureCardType) && !slot.isBackRow)
                    continue;

                int cityIndex = slot.cityIndex;
                List<CardData> cityCards = CityManager.Instance.GetNatureCardsInCity(cityIndex, true);

                if (WouldCompleteFusionCombo(cityCards, data.natureCardType))
                    return new EnemyMove { cardPrefab = cardPrefab, targetSlot = slot };

                if (WouldCompleteModifierCombo(cityCards, data.natureCardType))
                    return new EnemyMove { cardPrefab = cardPrefab, targetSlot = slot };
            }
        }

        return null;
    }

    EnemyMove TryFindBaseElementMove()
    {
        List<BoardSlot> candidateSlots = new List<BoardSlot>();

        foreach (BoardSlot slot in enemySlots)
        {
            if (!slot.IsEmpty())
                continue;

            if (!slot.isFrontRow)
                continue;

            int cityIndex = slot.cityIndex;

            if (CityManager.Instance.CityHasActiveDisaster(cityIndex))
                continue;

            int baseCount = CityManager.Instance.CountBaseElementCardsInCity(cityIndex);
            bool hasFusion = CityManager.Instance.CityHasFusionCard(cityIndex);
            bool hasModifier = CityManager.Instance.CityHasModifierCard(cityIndex);

            if (hasFusion && hasModifier)
                continue;

            if (baseCount > 0)
                continue;

            candidateSlots.Add(slot);
        }

        if (candidateSlots.Count == 0)
            return null;

        List<GameObject> baseCards = new List<GameObject>();

        foreach (GameObject cardPrefab in enemyDeck)
        {
            CardData data = cardPrefab.GetComponent<CardData>();
            if (data == null) continue;

            if (IsBaseElementCard(data.natureCardType))
                baseCards.Add(cardPrefab);
        }

        if (baseCards.Count == 0)
            return null;

        GameObject chosenCard = baseCards[Random.Range(0, baseCards.Count)];
        BoardSlot chosenSlot = candidateSlots[Random.Range(0, candidateSlots.Count)];

        return new EnemyMove { cardPrefab = chosenCard, targetSlot = chosenSlot };
    }

    bool WouldCompleteFusionCombo(List<CardData> cityCards, NatureCardType newCard)
    {
        bool hasTorrent = HasNatureCard(cityCards, NatureCardType.TorrentOfTheNaiads);
        bool hasFury = HasNatureCard(cityCards, NatureCardType.FuryOfEnceladus);
        bool hasBoreas = HasNatureCard(cityCards, NatureCardType.BreathOfBoreas);
        bool hasFlame = HasNatureCard(cityCards, NatureCardType.FlameOfPrometheus);

        if (newCard == NatureCardType.FuryOfEnceladus && hasTorrent) return true;
        if (newCard == NatureCardType.TorrentOfTheNaiads && (hasFury || hasTorrent)) return true;
        if (newCard == NatureCardType.BreathOfBoreas && (hasBoreas || hasFlame)) return true;
        if (newCard == NatureCardType.FlameOfPrometheus && hasBoreas) return true;

        return false;
    }

    bool WouldCompleteModifierCombo(List<CardData> cityCards, NatureCardType newCard)
    {
        bool hasFlame = HasNatureCard(cityCards, NatureCardType.FlameOfPrometheus);
        bool hasScourge = HasNatureCard(cityCards, NatureCardType.ScourgeOfHelios);
        bool hasEye = HasNatureCard(cityCards, NatureCardType.EyeOfHuracan);
        bool hasFury = HasNatureCard(cityCards, NatureCardType.FuryOfEnceladus);

        if (newCard == NatureCardType.ParchedEarth && (hasFlame || hasScourge)) return true;
        if (newCard == NatureCardType.RapidIntensification && hasEye) return true;
        if (newCard == NatureCardType.SeismicEchoes && hasFury) return true;

        return false;
    }

    bool HasNatureCard(List<CardData> cards, NatureCardType type)
    {
        foreach (CardData card in cards)
        {
            if (card.natureCardType == type)
                return true;
        }

        return false;
    }

    bool IsBaseElementCard(NatureCardType type)
    {
        return type == NatureCardType.BreathOfBoreas ||
               type == NatureCardType.FuryOfEnceladus ||
               type == NatureCardType.TorrentOfTheNaiads ||
               type == NatureCardType.FlameOfPrometheus;
    }

    bool IsModifierCard(NatureCardType type)
    {
        return type == NatureCardType.ParchedEarth ||
               type == NatureCardType.RapidIntensification ||
               type == NatureCardType.SeismicEchoes;
    }
}