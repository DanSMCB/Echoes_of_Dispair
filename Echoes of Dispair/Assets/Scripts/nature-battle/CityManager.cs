using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class CityManager : MonoBehaviour
{
    public static CityManager Instance;

    public CityState[] cities;
    public BoardSlot[] boardSlots;

    [Header("Player Health")]
    public int maxPlayerHealth = 30;
    public int currentPlayerHealth = 30;
    public TMP_Text healthUI;

    [Header("City Elevations")]
    public int[] cityElevations = new int[3];

    public GameObject populationCardPrefab;
    public BoardSlot[] startingPopulationSlots;

    [Header("Combination Prefabs")]
    public GameObject awakeningOfNamazuPrefab;
    public GameObject eyeOfHuracanPrefab;
    public GameObject cataclysmOfNjordPrefab;
    public GameObject scourgeOfHeliosPrefab;

    private List<PopulationEvacuationRisk> evacuationRisks = new List<PopulationEvacuationRisk>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
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
            cities = new CityState[3];

        for (int i = 0; i < cities.Length; i++)
        {
            cities[i] = new CityState();
            int elevation = 0;

            if (cityElevations != null && i < cityElevations.Length)
                elevation = cityElevations[i];

            cities[i].Initialize(i, elevation);
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

    public void DamagePlayer(int amount)
    {
        currentPlayerHealth -= amount;

        if (currentPlayerHealth < 0)
            currentPlayerHealth = 0;

        healthUI.text = currentPlayerHealth.ToString();
    }

    public void ScheduleDisaster(int cityIndex, DisasterType type, int damagePerTurn, int delayTurns, int durationTurns, GameObject sourceCard)
    {
        CityState city = cities[cityIndex];

        DisasterInstance disaster = new DisasterInstance
        {
            type = type,
            damagePerTurn = damagePerTurn,
            turnsUntilActivation = delayTurns,
            activeTurnsRemaining = durationTurns
        };

        city.pendingDisaster = disaster;
        city.pendingDisasterCard = sourceCard;

        if (InstructionUI.Instance != null)
        {
            InstructionUI.Instance.ShowInstruction(GetDisasterWarningText(type, cityIndex, delayTurns));
        }
    }

    public void AdvanceDisastersOneTurn()
    {
        foreach (CityState city in cities)
        {
            ProcessPendingDisaster(city);
            ProcessActiveDisaster(city);
            ProcessBlackoutChance(city);
        }

        ReduceHumanEffectDurations();
        ProcessEvacuationRisks();
        ReduceNatureModifierDurations();
    }

    void ProcessPendingDisaster(CityState city)
    {
        if (city.pendingDisaster == null)
            return;

        city.pendingDisaster.turnsUntilActivation--;

        if (city.pendingDisaster.turnsUntilActivation <= 0)
        {
            city.activeDisaster = city.pendingDisaster;
            city.activeDisasterCard = city.pendingDisasterCard;

            city.pendingDisaster = null;
            city.pendingDisasterCard = null;
        }
    }

    void ProcessActiveDisaster(CityState city)
    {
        if (city.activeDisaster == null)
            return;

        if (!city.activeDisaster.IsActive())
            return;

        int populationCount = GetPopulationCountInCity(city.cityIndex);

        if (populationCount > 0)
        {
            int baseDamage = city.activeDisaster.damagePerTurn;
            int finalDamagePerPopulation = ModifyDamageForCity(city, city.activeDisaster.type, baseDamage);
            int totalDamage = finalDamagePerPopulation * populationCount;

            if (totalDamage > 0)
            {
                DamagePlayer(totalDamage);
            }
        }

        city.activeDisaster.activeTurnsRemaining--;

        if (city.activeDisaster.activeTurnsRemaining <= 0)
        {
            if (city.activeDisaster.type == DisasterType.Flood)
            {
                city.blackoutActive = false;
            }

            if (city.activeDisasterCard != null)
            {
                BoardSlot slot = city.activeDisasterCard.GetComponent<CardData>()?.currentSlot;
                if (slot != null)
                    slot.RemoveCard();

                Destroy(city.activeDisasterCard);
                city.activeDisasterCard = null;
            }

            city.activeDisaster = null;
        }

    }

    void ReduceHumanEffectDurations()
    {
        foreach (CityState city in cities)
        {
            DestroyEffectCardIfExpired(ref city.emergencyGeneratorTurns, ref city.emergencyGeneratorCard);
            DestroyEffectCardIfExpired(ref city.stayShelteredTurns, ref city.stayShelteredCard);
            DestroyEffectCardIfExpired(ref city.powerCutOffTurns, ref city.powerCutOffCard);
            DestroyEffectCardIfExpired(ref city.emergencyKitTurns, ref city.emergencyKitCard);
            DestroyEffectCardIfExpired(ref city.stayHydratedTurns, ref city.stayHydratedCard);
            DestroyEffectCardIfExpired(ref city.sandbagsTurns, ref city.sandbagsCard);
            DestroyEffectCardIfExpired(ref city.barricadeTurns, ref city.barricadeCard);
        }
    }

    void ReduceNatureModifierDurations()
{
    foreach (CityState city in cities)
    {
        DestroyEffectCardIfExpired(ref city.parchedEarthTurns, ref city.parchedEarthCard);
        DestroyEffectCardIfExpired(ref city.rapidIntensificationTurns, ref city.rapidIntensificationCard);
        DestroyEffectCardIfExpired(ref city.seismicEchoesTurns, ref city.seismicEchoesCard);
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

    public void ResolveNaturePlay(CardData playedCard, BoardSlot playedSlot)
    {
        int cityIndex = playedSlot.cityIndex;

        if (TryResolveFusionCombination(cityIndex))
            return;

        if (TryResolveModifierCombination(cityIndex))
            return;

        ResolveNatureCard(playedCard, cityIndex, playedCard.gameObject);
    }

    public void ResolveNatureCard(CardData card, int cityIndex, GameObject sourceCard)
    {
        switch (card.natureCardType)
        {
            case NatureCardType.BreathOfBoreas:
                ScheduleDisaster(cityIndex, DisasterType.StrongWind, 0, 2, 5, sourceCard);
                break;

            case NatureCardType.FuryOfEnceladus:
                ScheduleDisaster(cityIndex, DisasterType.Earthquake, 10, 2, 1, sourceCard);
                break;

            case NatureCardType.TorrentOfTheNaiads:
                ScheduleDisaster(cityIndex, DisasterType.Rain, 0, 2, 4, sourceCard);
                break;

            case NatureCardType.FlameOfPrometheus:
                ScheduleDisaster(cityIndex, DisasterType.Wildfire, 6, 2, 2, sourceCard);
                break;

            case NatureCardType.ParchedEarth:
                RegisterNatureModifierCard(card, cityIndex);
                ScheduleDisaster(cityIndex, DisasterType.Drought, 2, 2, 2, sourceCard);
                break;

            case NatureCardType.RapidIntensification:
                RegisterNatureModifierCard(card, cityIndex);
                break;

            case NatureCardType.SeismicEchoes:
                RegisterNatureModifierCard(card, cityIndex);
                break;

            case NatureCardType.AwakeningOfNamazu:
                ScheduleDisaster(cityIndex, DisasterType.Tsunami, 11, 2, 2, sourceCard);
                break;

            case NatureCardType.ScourgeOfHelios:
                ScheduleDisaster(cityIndex, DisasterType.Wildfire, 7, 2, 3, sourceCard);
                break;

            case NatureCardType.CataclysmOfNjord:
                ScheduleDisaster(cityIndex, DisasterType.Flood, 4, 2, 3, sourceCard);
                break;

            case NatureCardType.EyeOfHuracan:
                ScheduleDisaster(cityIndex, DisasterType.Depression, 4, 2, 3, sourceCard);
                break;
        }
    }

    public void ResolveHumanCard(CardData card, int cityIndex)
    {
        CityState city = cities[cityIndex];

        switch (card.humanCardType)
        {
            case HumanCardType.EmergencyGenerator:
                city.emergencyGeneratorTurns = 5;
                city.emergencyGeneratorCard = card.gameObject;
                break;

            case HumanCardType.StaySheltered:
                city.stayShelteredTurns = 5;
                city.stayShelteredCard = card.gameObject;
                break;

            case HumanCardType.PowerCutOff:
                city.powerCutOffTurns = 5;
                city.powerCutOffCard = card.gameObject;
                break;

            case HumanCardType.EmergencyKit:
                city.emergencyKitTurns = 10;
                city.emergencyKitCard = card.gameObject;
                break;

            case HumanCardType.StayHydrated:
                city.stayHydratedTurns = 5;
                city.stayHydratedCard = card.gameObject;
                break;

            case HumanCardType.Sandbags:
                city.sandbagsTurns = 5;
                city.sandbagsCard = card.gameObject;
                break;

            case HumanCardType.Barricade:
                city.barricadeTurns = 5;
                city.barricadeCard = card.gameObject;
                break;

            case HumanCardType.Evacuation:
                break;
        }
    }

    public int GetCityElevation(int cityIndex)
    {
        if (cityIndex < 0 || cityIndex >= cities.Length)
            return 0;

        return cities[cityIndex].elevation;
    }

    int ModifyDamageForCity(CityState city, DisasterType disasterType, int baseDamage)
    {
        float damage = baseDamage;

        switch (disasterType)
        {
            case DisasterType.StrongWind:
            case DisasterType.Depression:
            case DisasterType.TropicalStorm:
                if (city.blackoutActive)
                    damage *= 2f;
                if (city.stayShelteredTurns > 0)
                    damage *= 0.3f;

                if (city.barricadeTurns > 0 && disasterType == DisasterType.Hurricane)
                    damage = 0f;
                break;

            case DisasterType.Hurricane:
                if (city.blackoutActive)
                    damage *= 2f;
                if (city.stayShelteredTurns > 0)
                        damage *= 0.6f;
    
                    if (city.barricadeTurns > 0)
                        damage *= 0.2f;
                    break;

            case DisasterType.Wildfire:
                if (city.blackoutActive)
                    damage *= 2f;
                if (city.stayShelteredTurns > 0)
                    damage *= 1.6f;

                if (city.powerCutOffTurns > 0)
                    damage *= 0.5f;

                if (city.stayHydratedTurns > 0)
                    damage *= 0.6f;

                if (city.sandbagsTurns > 0)
                    damage *= 1.4f;

                if (city.barricadeTurns > 0)
                    damage *= 1.4f;
                break;

            case DisasterType.Earthquake:
            case DisasterType.Tsunami:
                if (city.blackoutActive)
                    damage *= 1.4f;
                if (city.stayShelteredTurns > 0)
                    damage *= 1.6f;

                if (city.powerCutOffTurns > 0)
                    damage *= 0.5f;
                break;

            case DisasterType.Flood:
                if (city.blackoutActive)
                    damage *= 2f;
                if (city.stayHydratedTurns > 0)
                    damage = 0;

                if (city.sandbagsTurns > 0)
                    damage = 0;
                break;

            case DisasterType.Drought:
                if (city.blackoutActive)
                    damage *= 2f;
                if (city.stayHydratedTurns > 0)
                    damage = 0;
                break;
        }

        if (city.emergencyKitTurns > 0)
        {
            damage *= 0.2f;
            city.emergencyKitTurns = 0;

            if (city.emergencyKitCard != null)
            {
                CardData data = city.emergencyKitCard.GetComponent<CardData>();
                if (data != null && data.currentSlot != null)
                    data.currentSlot.RemoveCard();

                Destroy(city.emergencyKitCard);
                city.emergencyKitCard = null;
            }
        }

        return Mathf.RoundToInt(damage);
    }

    public bool CityHasActiveDisaster(int cityIndex)
    {
        CityState city = cities[cityIndex];
        return city.activeDisaster != null;
    }

    public void RegisterEvacuationRisk(GameObject populationCard, int sourceCityIndex, int destinationCityIndex, DisasterType disasterType, float damageMultiplier)
    {
        PopulationEvacuationRisk risk = new PopulationEvacuationRisk
        {
            populationCard = populationCard,
            sourceCityIndex = sourceCityIndex,
            destinationCityIndex = destinationCityIndex,
            relatedDisasterType = disasterType,
            damageMultiplier = damageMultiplier,
            consumed = false
        };

        evacuationRisks.Add(risk);
    }

    public bool CityHasTsunamiThreat(int cityIndex)
    {
        CityState city = cities[cityIndex];

        if (city.activeDisaster != null && city.activeDisaster.type == DisasterType.Tsunami)
            return true;

        if (city.pendingDisaster != null && city.pendingDisaster.type == DisasterType.Tsunami)
            return true;

        return false;
    }

    void ProcessEvacuationRisks()
    {
        for (int i = 0; i < evacuationRisks.Count; i++)
        {
            PopulationEvacuationRisk risk = evacuationRisks[i];

            if (risk.consumed)
                continue;

            if (risk.populationCard == null)
            {
                risk.consumed = true;
                continue;
            }

            CityState sourceCity = cities[risk.sourceCityIndex];

            if (sourceCity.activeDisaster == null)
                continue;

            if (sourceCity.activeDisaster.type != risk.relatedDisasterType)
                continue;

            int baseDamage = sourceCity.activeDisaster.damagePerTurn;
            int redirectedDamage = Mathf.RoundToInt(baseDamage * risk.damageMultiplier);

            if (redirectedDamage > 0)
            {
                DamagePlayer(redirectedDamage);
            }

            risk.consumed = true;
        }

        evacuationRisks.RemoveAll(r => r == null || r.consumed);
    }

    public List<CardData> GetNatureCardsInCity(int cityIndex, bool enemySideOnly = true)
    {
        List<CardData> cards = new List<CardData>();

        foreach (BoardSlot slot in boardSlots)
        {
            if (slot.cityIndex != cityIndex)
                continue;

            if (enemySideOnly && !slot.belongsToEnemy)
                continue;

            if (slot.currentCard == null)
                continue;

            CardData card = slot.currentCard.GetComponent<CardData>();
            if (card != null && card.cardType == CardType.Nature)
                cards.Add(card);
        }

        return cards;
    }

    public void RegisterNatureModifierCard(CardData card, int cityIndex)
    {
        CityState city = cities[cityIndex];

        switch (card.natureCardType)
        {
            case NatureCardType.ParchedEarth:
                city.parchedEarthTurns = 5;
                city.parchedEarthCard = card.gameObject;
                break;

            case NatureCardType.RapidIntensification:
                city.rapidIntensificationTurns = 5;
                city.rapidIntensificationCard = card.gameObject;
                break;

            case NatureCardType.SeismicEchoes:
                city.seismicEchoesTurns = 5;
                city.seismicEchoesCard = card.gameObject;
                break;
        }
    }

    bool TryResolveFusionCombination(int cityIndex)
    {
        List<CardData> natureCards = GetNatureCardsInCity(cityIndex, true);

        CardData torrent1 = FindFirstNatureCard(natureCards, NatureCardType.TorrentOfTheNaiads);
        CardData torrent2 = FindSecondNatureCard(natureCards, NatureCardType.TorrentOfTheNaiads);

        CardData fury = FindFirstNatureCard(natureCards, NatureCardType.FuryOfEnceladus);
        CardData boreas1 = FindFirstNatureCard(natureCards, NatureCardType.BreathOfBoreas);
        CardData boreas2 = FindSecondNatureCard(natureCards, NatureCardType.BreathOfBoreas);
        CardData flame = FindFirstNatureCard(natureCards, NatureCardType.FlameOfPrometheus);

        if (torrent1 != null && fury != null)
        {
            FuseCardsIntoCombination(torrent1, fury, awakeningOfNamazuPrefab, cityIndex, DisasterType.Tsunami, 12, 2, 3);
            return true;
        }

        if (boreas1 != null && boreas2 != null)
        {
            FuseCardsIntoCombination(boreas1, boreas2, eyeOfHuracanPrefab, cityIndex, DisasterType.Depression, 4, 2, 3);
            return true;
        }

        if (torrent1 != null && torrent2 != null)
        {
            FuseCardsIntoCombination(torrent1, torrent2, cataclysmOfNjordPrefab, cityIndex, DisasterType.Flood, 4, 2, 3);
            return true;
        }

        if (flame != null && boreas1 != null)
        {
            FuseCardsIntoCombination(flame, boreas1, scourgeOfHeliosPrefab, cityIndex, DisasterType.Wildfire, 7, 2, 3);
            return true;
        }

        return false;
    }

    CardData FindFirstNatureCard(List<CardData> cards, NatureCardType type)
    {
        foreach (CardData card in cards)
        {
            if (card.natureCardType == type)
                return card;
        }

        return null;
    }

    CardData FindSecondNatureCard(List<CardData> cards, NatureCardType type)
    {
        bool foundFirst = false;

        foreach (CardData card in cards)
        {
            if (card.natureCardType == type)
            {
                if (!foundFirst)
                    foundFirst = true;
                else
                    return card;
            }
        }

        return null;
    }

    public bool CityHasFusionCard(int cityIndex)
    {
        List<CardData> cards = GetNatureCardsInCity(cityIndex, true);

        foreach (CardData card in cards)
        {
            if (card.natureCardType == NatureCardType.AwakeningOfNamazu ||
                card.natureCardType == NatureCardType.ScourgeOfHelios ||
                card.natureCardType == NatureCardType.CataclysmOfNjord ||
                card.natureCardType == NatureCardType.EyeOfHuracan)
            {
                return true;
            }
        }

        return false;
    }
    public bool CityHasModifierCard(int cityIndex)
    {
        List<CardData> cards = GetNatureCardsInCity(cityIndex, true);

        foreach (CardData card in cards)
        {
            if (card.natureCardType == NatureCardType.ParchedEarth ||
                card.natureCardType == NatureCardType.RapidIntensification ||
                card.natureCardType == NatureCardType.SeismicEchoes)
            {
                return true;
            }
        }

        return false;
    }

    public int CountBaseElementCardsInCity(int cityIndex)
    {
        List<CardData> cards = GetNatureCardsInCity(cityIndex, true);
        int count = 0;

        foreach (CardData card in cards)
        {
            if (card.natureCardType == NatureCardType.BreathOfBoreas ||
                card.natureCardType == NatureCardType.FuryOfEnceladus ||
                card.natureCardType == NatureCardType.TorrentOfTheNaiads ||
                card.natureCardType == NatureCardType.FlameOfPrometheus)
            {
                count++;
            }
        }

        return count;
    }

    void FuseCardsIntoCombination(
    CardData cardA,
    CardData cardB,
    GameObject combinationPrefab,
    int cityIndex,
    DisasterType disasterType,
    int damagePerTurn,
    int delayTurns,
    int durationTurns)
    {
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

        ScheduleDisaster(cityIndex, disasterType, damagePerTurn, delayTurns, durationTurns, combinedCard);

        if (InstructionUI.Instance != null)
        {
            InstructionUI.Instance.ShowInstruction(GetDisasterWarningText(disasterType, cityIndex, delayTurns));
        }
    }

    bool TryResolveModifierCombination(int cityIndex)
    {
        List<CardData> natureCards = GetNatureCardsInCity(cityIndex, true);

        CardData flame = FindFirstNatureCard(natureCards, NatureCardType.FlameOfPrometheus);
        CardData scourge = FindFirstNatureCard(natureCards, NatureCardType.ScourgeOfHelios);
        CardData drought = FindFirstNatureCard(natureCards, NatureCardType.ParchedEarth);

        CardData depression = FindCardByNatureTypeInCity(cityIndex, NatureCardType.EyeOfHuracan);
        CardData rapidIntensification = FindFirstNatureCard(natureCards, NatureCardType.RapidIntensification);

        CardData fury = FindFirstNatureCard(natureCards, NatureCardType.FuryOfEnceladus);
        CardData seismic = FindFirstNatureCard(natureCards, NatureCardType.SeismicEchoes);

        if (flame != null && drought != null)
        {
            ScheduleDisaster(cityIndex, DisasterType.Wildfire, 8, 2, 3, flame.gameObject);
            return true;
        }

        if (flame != null && drought != null)
        {
            ScheduleDisaster(cityIndex, DisasterType.Wildfire, 12, 2, 3, flame.gameObject);
            return true;
        }

        if (depression != null && rapidIntensification != null)
        {
            ScheduleDisaster(cityIndex, DisasterType.Hurricane, 10, 2, 3, depression.gameObject);
            return true;
        }

        if (fury != null && seismic != null)
        {
            ScheduleDisaster(cityIndex, DisasterType.Earthquake, 12, 2, 3, fury.gameObject);
            return true;
        }

        return false;
    }

    CardData FindCardByNatureTypeInCity(int cityIndex, NatureCardType type)
    {
        List<CardData> natureCards = GetNatureCardsInCity(cityIndex, true);

        foreach (CardData card in natureCards)
        {
            if (card.natureCardType == type)
                return card;
        }

        return null;
    }

    void ProcessBlackoutChance(CityState city)
    {
        if (city.activeDisaster == null)
            return;

        if (city.activeDisaster.type != DisasterType.Flood)
            return;

        if (city.blackoutActive)
            return;

        if (city.emergencyGeneratorTurns > 0)
        {
            city.blackoutActive = false;
            return;
        }

        float chance = 0.5f;

        if (Random.value < chance)
        {
            city.blackoutActive = true;

            if (InstructionUI.Instance != null)
            {
                InstructionUI.Instance.ShowInstruction("A blackout has struck City " + (city.cityIndex + 1) + ".");
            }
        }
    }

    public bool IsBlackoutActive(int cityIndex)
    {
        if (cityIndex < 0 || cityIndex >= cities.Length)
            return false;

        return cities[cityIndex].blackoutActive;
    }

    public string GetDisasterWarningText(DisasterType disasterType, int cityIndex, int turnsUntilStrike)
    {
        int cityNumber = cityIndex + 1;
        string turnText = turnsUntilStrike == 1 ? "turn" : "turns";

        switch (disasterType)
        {
            case DisasterType.StrongWind:
                return "";

            case DisasterType.Earthquake:
                return "An earthquake will strike city " + cityNumber + " in " + turnsUntilStrike + " " + turnText + ".";

            case DisasterType.Rain:
                return "Heavy rainfall will affect city " + cityNumber + " in " + turnsUntilStrike + " " + turnText + ".";

            case DisasterType.Wildfire:
                return "An intense wildfire will strike city " + cityNumber + " in " + turnsUntilStrike + " " + turnText + ".";

            case DisasterType.Drought:
                return "A severe drought will affect city " + cityNumber + " in " + turnsUntilStrike + " " + turnText + ".";

            case DisasterType.Depression:
                return "A tropical depression will affect city " + cityNumber + " in " + turnsUntilStrike + " " + turnText + ".";

            case DisasterType.Hurricane:
                return "A hurricane will strike city " + cityNumber + " in " + turnsUntilStrike + " " + turnText + ".";

            case DisasterType.Flood:
                return "A major flood will hit city " + cityNumber + " in " + turnsUntilStrike + " " + turnText + ".";

            case DisasterType.Tsunami:
                return "A tsunami will strike city " + cityNumber + " in " + turnsUntilStrike + " " + turnText + ".";

            default:
                return "A disaster will affect city " + cityNumber + " in " + turnsUntilStrike + " " + turnText + ".";
        }
    }

    public bool WasHumanCardEffective(CardData card, int cityIndex)
    {
        CityState city = cities[cityIndex];

        DisasterType pendingType = city.pendingDisaster != null ? city.pendingDisaster.type : DisasterType.None;
        DisasterType activeType = city.activeDisaster != null ? city.activeDisaster.type : DisasterType.None;

        switch (card.humanCardType)
        {
            case HumanCardType.StaySheltered:
                return activeType == DisasterType.Depression || pendingType == DisasterType.Depression ||
                       activeType == DisasterType.Hurricane || pendingType == DisasterType.Hurricane ||
                       activeType == DisasterType.StrongWind;

            case HumanCardType.PowerCutOff:
                return activeType == DisasterType.Wildfire || pendingType == DisasterType.Wildfire ||
                       activeType == DisasterType.Hurricane || pendingType == DisasterType.Hurricane ||
                       activeType == DisasterType.Earthquake || pendingType == DisasterType.Earthquake ||
                       activeType == DisasterType.Tsunami || pendingType == DisasterType.Tsunami ||
                       activeType == DisasterType.Flood || pendingType == DisasterType.Flood;

            case HumanCardType.EmergencyKit:
                return activeType != DisasterType.None || pendingType != DisasterType.None;

            case HumanCardType.StayHydrated:
                return activeType == DisasterType.Drought || pendingType == DisasterType.Drought ||
                       activeType == DisasterType.Wildfire || pendingType == DisasterType.Wildfire;

            case HumanCardType.Sandbags:
                return activeType == DisasterType.Flood || pendingType == DisasterType.Flood;

            case HumanCardType.Barricade:
                return activeType == DisasterType.Hurricane || pendingType == DisasterType.Hurricane;

            case HumanCardType.EmergencyGenerator:
                return city.blackoutActive || pendingType == DisasterType.Flood || activeType == DisasterType.Flood;

            case HumanCardType.Evacuation:
                return pendingType != DisasterType.None || activeType != DisasterType.None;

            default:
                return false;
        }
    }
}
