using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlagueCityManager : MonoBehaviour
{
    public static PlagueCityManager Instance;

    [Header("Cities")]
    public PlagueCityState[] cities;
    public BoardSlot[] boardSlots;

    [Header("Player Health")]
    public int maxPlayerHealth = 30;
    public int currentPlayerHealth = 30;
    public TMP_Text healthUI;

    [Header("Plague Combo Prefabs")]
    public GameObject silentSpreadPrefab;
    public GameObject pandemicPrefab;
    public GameObject globalPandemicPrefab;

    public GameObject populationCardPrefab;
    public BoardSlot[] startingPopulationSlots;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeCities();

        int bonusHealth = 0;

        if (RogueliteManager.Instance != null)
            bonusHealth = RogueliteManager.Instance.metaProgress.bonusMaxHealth;

        maxPlayerHealth += bonusHealth;
        currentPlayerHealth = maxPlayerHealth;
        healthUI.text = currentPlayerHealth.ToString();

        SpawnStartingPopulations();
    }

    void InitializeCities()
    {
        if (cities == null || cities.Length != 3)
            cities = new PlagueCityState[3];

        for (int i = 0; i < cities.Length; i++)
        {
            cities[i] = new PlagueCityState();
            cities[i].Initialize(i);
        }
    }

    void SpawnStartingPopulations()
    {
        for (int i = 0; i < startingPopulationSlots.Length; i++)
        {
            BoardSlot slot = startingPopulationSlots[i];

            if (slot == null)
                continue;

            if (!slot.IsEmpty())
                continue;

            GameObject population = Instantiate(populationCardPrefab, slot.transform);
            population.transform.localPosition = Vector3.zero;
            population.transform.localRotation = Quaternion.Euler(-90f, 90f, 0f);
            population.transform.localScale = new Vector3(3f, 3f, 3f);

            slot.PlaceCard(population);

            CardData cardData = population.GetComponent<CardData>();
            if (cardData != null)
                cardData.SetPlacedOnBoard(slot);
        }
    }

    public int GetPopulationCountInCity(int cityIndex)
    {
        int count = 0;

        foreach (BoardSlot slot in boardSlots)
        {
            if (slot.cityIndex != cityIndex)
                continue;

            if (slot.currentCard == null)
                continue;

            CardData card = slot.currentCard.GetComponent<CardData>();

            if (card != null && card.cardType == CardType.Population)
                count++;
        }

        return count;
    }

    public bool CityHasBaseVirus(int cityIndex)
    {
        if (cityIndex < 0 || cityIndex >= cities.Length)
            return false;

        return cities[cityIndex].hasBaseVirus;
    }

    public void ResolvePlagueCard(CardData card, BoardSlot slot)
    {
        if (card == null || slot == null)
            return;

        int cityIndex = slot.cityIndex;
        PlagueCityState city = cities[cityIndex];

        switch (card.plagueCardType)
        {
            case PlagueCardType.PlagueOfNosoi:
                RegisterBaseVirus(card, cityIndex);
                ScheduleThreat(cityIndex, PlagueThreatType.PlagueOfNosoi, 1, 2, 10, card.gameObject);
                break;

            case PlagueCardType.AirborneSpread:
                ScheduleThreat(cityIndex, PlagueThreatType.AirborneSpread, 4, 2, 2, card.gameObject);
                break;

            case PlagueCardType.WaterbornePathogen:
                ScheduleThreat(cityIndex, PlagueThreatType.WaterbornePathogen, 4, 2, 2, card.gameObject);
                break;

            case PlagueCardType.SurfaceContamination:
                ScheduleThreat(cityIndex, PlagueThreatType.SurfaceContamination, 3, 2, 2, card.gameObject);
                break;

            case PlagueCardType.UrbanTransmission:
                ScheduleThreat(cityIndex, PlagueThreatType.UrbanTransmission, 5, 2, 2, card.gameObject);
                break;

            case PlagueCardType.DelayedSymptoms:
                ScheduleThreat(cityIndex, PlagueThreatType.DelayedSymptoms, 0, 2, 2, card.gameObject);
                break;

            case PlagueCardType.RapidMutation:
                city.rapidMutationTurns = 4;
                city.rapidMutationCard = card.gameObject;
                break;

            case PlagueCardType.SilentSpread:
                ScheduleThreat(cityIndex, PlagueThreatType.SilentSpread, 6, 3, 3, card.gameObject);
                break;

            case PlagueCardType.Pandemic:
                ScheduleThreat(cityIndex, PlagueThreatType.Pandemic, 8, 3, 3, card.gameObject);
                break;

            case PlagueCardType.GlobalPandemic:
                ScheduleThreat(cityIndex, PlagueThreatType.GlobalPandemic, 12, 3, 3, card.gameObject);
                break;
        }

        TryResolvePlagueCombos(cityIndex);
    }

    void TryResolvePlagueCombos(int cityIndex)
    {
        TryCreatePandemic(cityIndex);
    }

    void TryCreatePandemic(int cityIndex)
    {
        List<CardData> cards = GetPlagueCardsInCity(cityIndex);

        CardData airborne = FindPlagueCard(cards, PlagueCardType.AirborneSpread);
        CardData urban = FindPlagueCard(cards, PlagueCardType.UrbanTransmission);

        if (airborne == null || urban == null)
            return;

        FusePlagueCardsIntoCombination(airborne,urban,pandemicPrefab,cityIndex,PlagueThreatType.Pandemic,8,2,3);
    }

    public List<CardData> GetPlagueCardsInCity(int cityIndex)
    {
        List<CardData> cards = new List<CardData>();

        foreach (BoardSlot slot in boardSlots)
        {
            if (slot.cityIndex != cityIndex)
                continue;

            if (!slot.belongsToEnemy)
                continue;

            if (slot.currentCard == null)
                continue;

            CardData card = slot.currentCard.GetComponent<CardData>();

            if (card != null && card.cardType == CardType.Plague)
                cards.Add(card);
        }

        return cards;
    }

    CardData FindPlagueCard(List<CardData> cards, PlagueCardType type)
    {
        foreach (CardData card in cards)
        {
            if (card.plagueCardType == type)
                return card;
        }

        return null;
    }

    void FusePlagueCardsIntoCombination(
    CardData cardA,
    CardData cardB,
    GameObject combinationPrefab,
    int cityIndex,
    PlagueThreatType threatType,
    int damagePerTurn,
    int delayTurns,
    int durationTurns)
    {
        if (combinationPrefab == null)
        {
            return;
        }

        BoardSlot targetSlot = cardB.currentSlot != null ? cardB.currentSlot : cardA.currentSlot;

        if (cardA.currentSlot != null)
            cardA.currentSlot.RemoveCard();

        if (cardB.currentSlot != null)
            cardB.currentSlot.RemoveCard();

        Destroy(cardA.gameObject);
        Destroy(cardB.gameObject);

        GameObject combinedCard = Instantiate(combinationPrefab, targetSlot.transform);
        combinedCard.transform.localPosition = Vector3.zero;
        combinedCard.transform.localRotation = Quaternion.Euler(-90f, -90f, 0f);
        combinedCard.transform.localScale = new Vector3(3f, 3f, 3f);

        targetSlot.PlaceCard(combinedCard);

        CardData combinedData = combinedCard.GetComponent<CardData>();
        if (combinedData != null)
            combinedData.SetPlacedOnBoard(targetSlot);

        ScheduleThreat(cityIndex, threatType, damagePerTurn, delayTurns, durationTurns, combinedCard);

        Debug.Log("Plague combo created in city " + cityIndex + ": " + combinedData.cardName);
    }

    void RegisterBaseVirus(CardData card, int cityIndex)
    {
        PlagueCityState city = cities[cityIndex];

        city.hasBaseVirus = true;
        city.baseVirusCard = card.gameObject;

        Debug.Log("Base virus started in city " + cityIndex);
    }

    public void ScheduleThreat(
        int cityIndex,
        PlagueThreatType type,
        int damagePerTurn,
        int delayTurns,
        int durationTurns,
        GameObject sourceCard)
    {
        PlagueCityState city = cities[cityIndex];

        PlagueThreatInstance threat = new PlagueThreatInstance
        {
            type = type,
            damagePerTurn = damagePerTurn,
            turnsUntilActivation = delayTurns,
            activeTurnsRemaining = durationTurns
        };

        city.pendingThreats.Add(threat);
        city.pendingThreatCards.Add(sourceCard);
    }

    public void AdvancePlagueThreatsOneTurn()
    {

        foreach (PlagueCityState city in cities)
        {

            ProcessPendingThreats(city);
            ProcessActiveThreats(city);
        }

        ReduceHumanEffectDurations();
        ReducePlagueModifierDurations();
    }

    void ProcessPendingThreats(PlagueCityState city)
    {
        for (int i = city.pendingThreats.Count - 1; i >= 0; i--)
        {
            PlagueThreatInstance threat = city.pendingThreats[i];

            threat.turnsUntilActivation--;

            if (threat.turnsUntilActivation <= 0)
            {
                GameObject card = city.pendingThreatCards[i];

                city.activeThreats.Add(threat);
                city.activeThreatCards.Add(card);

                city.pendingThreats.RemoveAt(i);
                city.pendingThreatCards.RemoveAt(i);

                Debug.Log("Plague threat activated in city " + city.cityIndex + ": " + threat.type);
            }
        }
    }

    void ProcessActiveThreats(PlagueCityState city)
    {
        for (int i = city.activeThreats.Count - 1; i >= 0; i--)
        {
            PlagueThreatInstance threat = city.activeThreats[i];
            GameObject card = city.activeThreatCards[i];

            if (threat == null || !threat.IsActive())
                continue;

            int baseDamage = threat.damagePerTurn;
            int finalDamage = ModifyPlagueDamageForCity(city, threat.type, baseDamage);

            int prevented = baseDamage - finalDamage;
            if (prevented > 0)
            {
                LearningTracker.Instance.AddTotalDamage(baseDamage);
                LearningTracker.Instance.AddPreventedDamage(prevented);
            }

            DamagePlayer(finalDamage);

            Debug.Log("City " + city.cityIndex + " suffered " +
                      finalDamage + " damage from " + threat.type);

            threat.activeTurnsRemaining--;

            if (threat.activeTurnsRemaining <= 0)
            {
                Debug.Log("Plague threat ended in city " + city.cityIndex + ": " + threat.type);

                HandleEndedThreat(city, threat.type, card);

                city.activeThreats.RemoveAt(i);
                city.activeThreatCards.RemoveAt(i);
            }
        }
    }

    void HandleEndedThreat(PlagueCityState city, PlagueThreatType endedType, GameObject endedCard)
    {
        if (endedType == PlagueThreatType.DelayedSymptoms)
        {
            TransformToPlagueThreatCard(endedCard,silentSpreadPrefab,city.cityIndex,PlagueThreatType.SilentSpread,6,1,3);
            return;
        }

        if (endedType == PlagueThreatType.Pandemic)
        {
            int delay = city.rapidMutationTurns > 0 ? 1 : 2;

            TransformToPlagueThreatCard(endedCard,globalPandemicPrefab,city.cityIndex,PlagueThreatType.GlobalPandemic,12,delay,3);
            return;
        }

        DestroyThreatCard(endedCard);
    }

    void TransformToPlagueThreatCard(
    GameObject oldCard,
    GameObject newPrefab,
    int cityIndex,
    PlagueThreatType newThreatType,
    int damagePerTurn,
    int delayTurns,
    int durationTurns)
    {
        if (oldCard == null || newPrefab == null)
        {
            return;
        }

        CardData oldData = oldCard.GetComponent<CardData>();
        BoardSlot targetSlot = oldData != null ? oldData.currentSlot : null;

        if (targetSlot == null)
        {
            return;
        }

        targetSlot.RemoveCard();
        Destroy(oldCard);

        GameObject newCard = Instantiate(newPrefab, targetSlot.transform);
        newCard.transform.localPosition = Vector3.zero;
        newCard.transform.localRotation = Quaternion.Euler(-90f, -90f, 0f);
        newCard.transform.localScale = new Vector3(3f, 3f, 3f);

        targetSlot.PlaceCard(newCard);

        CardData newData = newCard.GetComponent<CardData>();
        if (newData != null)
            newData.SetPlacedOnBoard(targetSlot);

        ScheduleThreat(cityIndex, newThreatType, damagePerTurn, delayTurns, durationTurns, newCard);

        Debug.Log("Plague evolved into " + newThreatType + " in city " + cityIndex);
    }

    void DestroyThreatCard(GameObject card)
    {
        if (card == null)
            return;

        CardData data = card.GetComponent<CardData>();

        if (data != null && data.currentSlot != null)
            data.currentSlot.RemoveCard();

        Destroy(card);
    }

    int ModifyPlagueDamageForCity(PlagueCityState city, PlagueThreatType threatType, int baseDamage)
    {
        float damage = baseDamage;

        switch (threatType)
        {
            case PlagueThreatType.AirborneSpread:
                if (city.maskUsageTurns > 0)
                    damage *= 0f;
                break;

            case PlagueThreatType.WaterbornePathogen:
                if (city.boilWaterTurns > 0)
                    damage *= 0f;

                if (city.handWashingTurns > 0)
                    if (city.boilWaterTurns == 0) { 
                        damage *= 1.4f;
                    }else damage *= 0f;
                break;

            case PlagueThreatType.SurfaceContamination:
                if (city.handWashingTurns > 0)
                    damage *= 0.5f;

                if (city.useSanitizerTurns > 0)
                    damage *= 0.2f;
                break;

            case PlagueThreatType.DelayedSymptoms:
            case PlagueThreatType.SilentSpread:
                if (city.selfIsolationTurns > 0)
                    damage *= 0.4f;

                if (city.avoidCrowdsTurns > 0)
                    damage *= 0.2f;

                if (city.maskUsageTurns > 0)
                    damage *= 0.5f;
                break;

            case PlagueThreatType.UrbanTransmission:
                if (city.avoidCrowdsTurns > 0)
                    damage *= 0f;

                if (city.lockdownMeasuresTurns > 0)
                    damage *= 0.4f;
                break;

            case PlagueThreatType.Pandemic:
                if (city.maskUsageTurns > 0)
                    damage *= 0.4f;

                if (city.avoidCrowdsTurns > 0)
                    damage *= 0.4f;

                if (city.stockEssentialsTurns > 0)
                    damage *= 0.5f;

                if (city.vaccinationTurns > 0)
                    damage *= 0.2f;

                if (city.lockdownMeasuresTurns > 0)
                    damage *= 0.3f;
                break;

            case PlagueThreatType.GlobalPandemic:
                if (city.maskUsageTurns > 0)
                    damage *= 0.6f;

                if (city.avoidCrowdsTurns > 0)
                    damage *= 0.6f;

                if (city.vaccinationTurns > 0)
                    damage *= 0.3f;

                if (city.lockdownMeasuresTurns > 0)
                    damage *= 0.4f;
                break;
        }

        return Mathf.RoundToInt(damage);
    }

    public void ResolveHumanPlagueCard(CardData card, int cityIndex)
    {
        PlagueCityState city = cities[cityIndex];

        switch (card.humanCardType)
        {
            case HumanCardType.SelfIsolation:
                city.selfIsolationTurns = 2;
                city.selfIsolationCard = card.gameObject;
                break;

            case HumanCardType.MaskUsage:
                city.maskUsageTurns = 2;
                city.maskUsageCard = card.gameObject;
                break;

            case HumanCardType.HandWashing:
                city.handWashingTurns = 2;
                city.handWashingCard = card.gameObject;
                break;

            case HumanCardType.UseSanitizer:
                city.useSanitizerTurns = 2;
                city.useSanitizerCard = card.gameObject;
                break;

            case HumanCardType.AvoidCrowds:
                city.avoidCrowdsTurns = 2;
                city.avoidCrowdsCard = card.gameObject;
                break;

            case HumanCardType.StockEssentials:
                city.stockEssentialsTurns = 2;
                city.stockEssentialsCard = card.gameObject;
                break;

            case HumanCardType.BoilWater:
                city.boilWaterTurns = 2;
                city.boilWaterCard = card.gameObject;
                break;

            case HumanCardType.Vaccination:
                city.vaccinationTurns = 3;
                city.vaccinationCard = card.gameObject;
                break;

            case HumanCardType.LockdownMeasures:
                city.lockdownMeasuresTurns = 4;
                city.lockdownMeasuresCard = card.gameObject;
                break;
        }

        Debug.Log(card.humanCardType + " applied in city " + cityIndex);
    }

    void ReduceHumanEffectDurations()
    {
        foreach (PlagueCityState city in cities)
        {
            DestroyEffectCardIfExpired(ref city.selfIsolationTurns, ref city.selfIsolationCard);
            DestroyEffectCardIfExpired(ref city.maskUsageTurns, ref city.maskUsageCard);
            DestroyEffectCardIfExpired(ref city.handWashingTurns, ref city.handWashingCard);
            DestroyEffectCardIfExpired(ref city.useSanitizerTurns, ref city.useSanitizerCard);
            DestroyEffectCardIfExpired(ref city.avoidCrowdsTurns, ref city.avoidCrowdsCard);
            DestroyEffectCardIfExpired(ref city.stockEssentialsTurns, ref city.stockEssentialsCard);
            DestroyEffectCardIfExpired(ref city.boilWaterTurns, ref city.boilWaterCard);
            DestroyEffectCardIfExpired(ref city.vaccinationTurns, ref city.vaccinationCard);
            DestroyEffectCardIfExpired(ref city.lockdownMeasuresTurns, ref city.lockdownMeasuresCard);
        }
    }

    void ReducePlagueModifierDurations()
    {
        foreach (PlagueCityState city in cities)
        {
            DestroyEffectCardIfExpired(ref city.rapidMutationTurns, ref city.rapidMutationCard);
        }
    }

    void DestroyEffectCardIfExpired(ref int turns, ref GameObject cardRef)
    {
        if (turns > 0)
        {
            turns--;

            if (turns <= 0)
            {
                if (cardRef != null)
                {
                    CardData data = cardRef.GetComponent<CardData>();

                    if (data != null && data.currentSlot != null)
                        data.currentSlot.RemoveCard();

                    Destroy(cardRef);
                    cardRef = null;
                }
            }
        }
    }

    public void DamagePlayer(int amount)
    {
        if (amount <= 0)
            return;
        Debug.Log(currentPlayerHealth + "hp");

        currentPlayerHealth -= amount;

        if (currentPlayerHealth < 0)
            currentPlayerHealth = 0;

        healthUI.text = currentPlayerHealth.ToString();
    }

    public bool WasHumanPlagueCardEffective(CardData card, int cityIndex)
    {
        PlagueCityState city = cities[cityIndex];

        switch (card.humanCardType)
        {
            case HumanCardType.SelfIsolation:
                return HasActiveOrPending(city, PlagueThreatType.DelayedSymptoms) ||
                       HasActiveOrPending(city, PlagueThreatType.SilentSpread);

            case HumanCardType.MaskUsage:
                return HasActiveOrPending(city, PlagueThreatType.AirborneSpread) ||
                       HasActiveOrPending(city, PlagueThreatType.SilentSpread) ||
                       HasActiveOrPending(city, PlagueThreatType.Pandemic) ||
                       HasActiveOrPending(city, PlagueThreatType.GlobalPandemic);

            case HumanCardType.HandWashing:
                return HasActiveOrPending(city, PlagueThreatType.SurfaceContamination);

            case HumanCardType.UseSanitizer:
                return HasActiveOrPending(city, PlagueThreatType.SurfaceContamination);

            case HumanCardType.AvoidCrowds:
                return HasActiveOrPending(city, PlagueThreatType.UrbanTransmission) ||
                       HasActiveOrPending(city, PlagueThreatType.SilentSpread) ||
                       HasActiveOrPending(city, PlagueThreatType.Pandemic) ||
                       HasActiveOrPending(city, PlagueThreatType.GlobalPandemic);

            case HumanCardType.StockEssentials:
                return HasActiveOrPending(city, PlagueThreatType.Pandemic) ||
                       HasActiveOrPending(city, PlagueThreatType.GlobalPandemic);

            case HumanCardType.BoilWater:
                return HasActiveOrPending(city, PlagueThreatType.WaterbornePathogen);

            case HumanCardType.Vaccination:
                return HasActiveOrPending(city, PlagueThreatType.Pandemic) ||
                       HasActiveOrPending(city, PlagueThreatType.GlobalPandemic);

            case HumanCardType.LockdownMeasures:
                return HasActiveOrPending(city, PlagueThreatType.UrbanTransmission) ||
                       HasActiveOrPending(city, PlagueThreatType.Pandemic) ||
                       HasActiveOrPending(city, PlagueThreatType.GlobalPandemic);

            default:
                return false;
        }
    }

    bool HasActiveOrPending(PlagueCityState city, PlagueThreatType type)
    {
        foreach (PlagueThreatInstance threat in city.pendingThreats)
        {
            if (threat.type == type)
                return true;
        }

        foreach (PlagueThreatInstance threat in city.activeThreats)
        {
            if (threat.type == type)
                return true;
        }

        return false;
    }
}