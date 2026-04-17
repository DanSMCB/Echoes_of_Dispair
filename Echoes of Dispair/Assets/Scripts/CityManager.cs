using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class CityManager : MonoBehaviour
{
    public static CityManager Instance;

    public CityState[] cities;
    public BoardSlot[] boardSlots;

    [Header("Player Health")]
    public int maxPlayerHealth = 100;
    public int currentPlayerHealth = 100;
    public TMP_Text healthUI;

    [Header("City Elevations")]
    public int[] cityElevations = new int[3];

    public GameObject populationCardPrefab;
    public BoardSlot[] startingPopulationSlots;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeCities();
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

        Debug.Log("Jogador sofreu " + amount + " de dano. Vida atual: " + currentPlayerHealth);

        healthUI.text = currentPlayerHealth.ToString();

        if (currentPlayerHealth == 0)
        {
            Debug.Log("Game Over");
        }
    }

    public void ScheduleDisaster(int cityIndex, DisasterType type, int damagePerTurn, int delayTurns, int durationTurns)
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

        Debug.Log("Desastre agendado na cidade " + cityIndex + ": " + type +
                  " | ativa em " + delayTurns + " turnos | dura " + durationTurns + " turnos.");
    }

    public void AdvanceDisastersOneTurn()
    {
        foreach (CityState city in cities)
        {
            ProcessPendingDisaster(city);
            ProcessActiveDisaster(city);
        }
    }

    void ProcessPendingDisaster(CityState city)
    {
        if (city.pendingDisaster == null)
            return;

        city.pendingDisaster.turnsUntilActivation--;

        if (city.pendingDisaster.turnsUntilActivation <= 0)
        {
            city.activeDisaster = city.pendingDisaster;
            city.pendingDisaster = null;

            Debug.Log("Cidade " + city.cityIndex + " - desastre pendente " + city.pendingDisaster.type + " ativa em " + city.pendingDisaster.turnsUntilActivation + " turnos.");
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
            int totalDamage = city.activeDisaster.damagePerTurn * populationCount;
            DamagePlayer(totalDamage);

            Debug.Log("Cidade " + city.cityIndex + " sofreu efeito de " +
                      city.activeDisaster.type + " e causou " + totalDamage + " dano.");
        }
        else
        {
            Debug.Log("Cidade " + city.cityIndex + " tem desastre ativo (" +
                      city.activeDisaster.type + "), mas sem população.");
        }

        city.activeDisaster.activeTurnsRemaining--;

        if (city.activeDisaster.activeTurnsRemaining <= 0)
        {
            Debug.Log("Desastre terminou na cidade " + city.cityIndex + ": " + city.activeDisaster.type);
            city.activeDisaster = null;
        }

        Debug.Log("Cidade " + city.cityIndex + " - desastre ativo " + city.activeDisaster.type + " com " + city.activeDisaster.activeTurnsRemaining + " turnos restantes.");
    }

    public void ResolveNatureCard(CardData card, int cityIndex)
    {
        switch (card.natureCardType)
        {
            case NatureCardType.BreathOfBoreas:
                ScheduleDisaster(cityIndex, DisasterType.StrongWind, 0, 2, 2);
                break;

            case NatureCardType.FuryOfEnceladus:
                ScheduleDisaster(cityIndex, DisasterType.Earthquake, 10, 2, 2);
                break;

            case NatureCardType.TorrentOfTheNaiads:
                ScheduleDisaster(cityIndex, DisasterType.Rain, 0, 2, 2);
                break;

            case NatureCardType.FlameOfPrometheus:
                ScheduleDisaster(cityIndex, DisasterType.Wildfire, 6, 2, 2);
                break;

            case NatureCardType.ParchedEarth:
                ScheduleDisaster(cityIndex, DisasterType.Drought, 3, 2, 2);
                break;

            case NatureCardType.RapidIntensification:
                ScheduleDisaster(cityIndex, DisasterType.Hurricane, 10, 2, 2);
                break;

            case NatureCardType.SeismicEchoes:
                Debug.Log("Seismic Echoes ainda não implementado.");
                break;

            default:
                Debug.Log("Carta da natureza sem efeito implementado: " + card.cardName);
                break;
        }
    }
}
