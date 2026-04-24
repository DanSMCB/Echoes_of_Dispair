using UnityEngine;
using TMPro;
using System.Collections;

public class CityInfoPanel : MonoBehaviour
{
    public static CityInfoPanel Instance;

    [Header("UI References")]
    public GameObject panelRoot;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI pendingText;
    public TextMeshProUGUI activeText;

    Coroutine fadeRoutine;

    void Awake()
    {
        Instance = this;
        HidePanel();
    }

    public void ShowCityInfo(int cityIndex)
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (titleText != null)
            titleText.text = "City " + (cityIndex + 1);

        if (TurnManager.Instance != null && TurnManager.Instance.battleMode == BattleMode.Plague)
        {
            ShowPlagueCityInfo(cityIndex);
        }
        else
        {
            ShowNatureCityInfo(cityIndex);
        }

        SetAlpha(1f);
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOutRoutine(2f, 1f));
    }

    void ShowNatureCityInfo(int cityIndex)
    {
        if (CityManager.Instance == null)
            return;

        if (cityIndex < 0 || cityIndex >= CityManager.Instance.cities.Length)
            return;

        CityState city = CityManager.Instance.cities[cityIndex];

        if (pendingText != null)
            pendingText.text = GetNaturePendingText(city);

        if (activeText != null)
            activeText.text = GetNatureActiveText(city);
    }

    void ShowPlagueCityInfo(int cityIndex)
    {
        if (PlagueCityManager.Instance == null)
            return;

        if (cityIndex < 0 || cityIndex >= PlagueCityManager.Instance.cities.Length)
            return;

        PlagueCityState city = PlagueCityManager.Instance.cities[cityIndex];

        if (pendingText != null)
            pendingText.text = GetPlaguePendingText(city);

        if (activeText != null)
            activeText.text = GetPlagueActiveText(city);
    }

    IEnumerator FadeOutRoutine(float delay, float fadeDuration)
    {
        yield return new WaitForSeconds(delay);

        float time = 0f;

        while (time < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
            SetAlpha(alpha);

            time += Time.deltaTime;
            yield return null;
        }

        SetAlpha(0f);
    }

    void SetAlpha(float alpha)
    {
        Color c1 = titleText.color;
        Color c2 = pendingText.color;
        Color c3 = activeText.color;
        c1.a = alpha;
        c2.a = alpha;
        c3.a = alpha;
        titleText.color = c1;
        pendingText.color = c2;
        activeText.color = c3;
    }

    public void HidePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    string GetNaturePendingText(CityState city)
    {
        if (city.pendingDisaster == null || !city.pendingDisaster.IsValid())
            return "No pending disaster currently.";

        string disasterName = FormatDisasterName(city.pendingDisaster.type);
        int turns = city.pendingDisaster.turnsUntilActivation;
        string turnText = turns == 1 ? "turn" : "turns";

        return "Pending disaster: " + disasterName + " in " + turns + " " + turnText;
    }

    string GetNatureActiveText(CityState city)
    {
        string result;

        if (city.activeDisaster == null || !city.activeDisaster.IsValid() || !city.activeDisaster.IsActive())
        {
            result = "No disaster happening currently.";
        }
        else
        {
            string disasterName = FormatDisasterName(city.activeDisaster.type);
            int turns = city.activeDisaster.activeTurnsRemaining;
            string turnText = turns == 1 ? "turn" : "turns";

            result = "Active disaster: " + disasterName + " (" + turns + " " + turnText + " left)";
        }

        if (city.blackoutActive)
        {
            result += "\nThere's a blackout happening.";
        }

        return result;
    }

    string GetPlaguePendingText(PlagueCityState city)
    {
        if (!city.hasBaseVirus)
            return "No infection detected.";

        string result = "A virus has hit this city.\n";

        string factors = GetSpreadFactorsText(city);
        result += "\nCurrent spread factors:\n" + factors;

        if (city.pendingThreats.Count > 0)
        {
            result += "\nDeveloping:\n";

            for (int i = 0; i < city.pendingThreats.Count; i++)
            {
                var threat = city.pendingThreats[i];
                string turnText = threat.turnsUntilActivation == 1 ? "turn" : "turns";

                result += "- " + FormatPlagueThreatName(threat.type) +
                          " in " + threat.turnsUntilActivation + " " + turnText + "\n";
            }
        }

        return result;
    }

    string GetPlagueActiveText(PlagueCityState city)
    {
        if (!city.hasBaseVirus)
            return "";

        if (city.activeThreats.Count == 0)
            return "Outbreak status:\nLocal outbreak";

        string result = "Active effects:\n";

        for (int i = 0; i < city.activeThreats.Count; i++)
        {
            var threat = city.activeThreats[i];
            string turnText = threat.activeTurnsRemaining == 1 ? "turn" : "turns";

            result += "- " + FormatPlagueThreatName(threat.type) +
                      " (" + threat.activeTurnsRemaining + " " + turnText + " left)\n";
        }

        return result;
    }

    string GetSpreadFactorsText(PlagueCityState city)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (HasPlagueCardInCity(city.cityIndex, PlagueCardType.AirborneSpread))
            sb.AppendLine("- Airborne spread");

        if (HasPlagueCardInCity(city.cityIndex, PlagueCardType.WaterbornePathogen))
            sb.AppendLine("- Waterborne pathogen");

        if (HasPlagueCardInCity(city.cityIndex, PlagueCardType.SurfaceContamination))
            sb.AppendLine("- Surface contamination");

        if (HasPlagueCardInCity(city.cityIndex, PlagueCardType.UrbanTransmission))
            sb.AppendLine("- Urban transmission");

        if (HasPlagueCardInCity(city.cityIndex, PlagueCardType.DelayedSymptoms))
            sb.AppendLine("- Delayed symptoms");

        if (city.rapidMutationTurns > 0)
            sb.AppendLine("- Rapid mutation");

        if (sb.Length == 0)
            return "None";

        return sb.ToString();
    }

    string GetOutbreakStatusText(PlagueThreatType type)
    {
        switch (type)
        {
            case PlagueThreatType.Pandemic:
                return "A pandemic has been declared.";

            case PlagueThreatType.GlobalPandemic:
                return "A global pandemic has been declared.";

            default:
                return "Local outbreak";
        }
    }

    string FormatDisasterName(DisasterType type)
    {
        switch (type)
        {
            case DisasterType.StrongWind: return "Strong Wind";
            case DisasterType.Earthquake: return "Earthquake";
            case DisasterType.Rain: return "Heavy Rain";
            case DisasterType.Wildfire: return "Wildfire";
            case DisasterType.Drought: return "Drought";
            case DisasterType.Depression: return "Tropical Depression";
            case DisasterType.TropicalStorm: return "Tropical Storm";
            case DisasterType.Hurricane: return "Hurricane";
            case DisasterType.Flood: return "Flood";
            case DisasterType.Tsunami: return "Tsunami";
            default: return "Unknown";
        }
    }

    string FormatPlagueThreatName(PlagueThreatType type)
    {
        switch (type)
        {
            case PlagueThreatType.PlagueOfNosoi: return "";
            case PlagueThreatType.AirborneSpread: return "Airborne Spread";
            case PlagueThreatType.WaterbornePathogen: return "Waterborne Pathogen";
            case PlagueThreatType.SurfaceContamination: return "Surface Contamination";
            case PlagueThreatType.RapidMutation: return "Rapid Mutation";
            case PlagueThreatType.UrbanTransmission: return "Urban Transmission";
            case PlagueThreatType.DelayedSymptoms: return "Delayed Symptoms";
            case PlagueThreatType.SilentSpread: return "Silent Spread";
            case PlagueThreatType.Pandemic: return "Pandemic";
            case PlagueThreatType.GlobalPandemic: return "Global Pandemic";
            default: return "Unknown";
        }
    }

    bool HasPlagueCardInCity(int cityIndex, PlagueCardType type)
    {
        if (PlagueCityManager.Instance == null)
            return false;

        var cards = PlagueCityManager.Instance.GetPlagueCardsInCity(cityIndex);

        foreach (CardData card in cards)
        {
            if (card.plagueCardType == type)
                return true;
        }

        return false;
    }
}
