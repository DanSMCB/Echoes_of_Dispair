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
        if (CityManager.Instance == null)
            return;

        if (cityIndex < 0 || cityIndex >= CityManager.Instance.cities.Length)
            return;

        CityState city = CityManager.Instance.cities[cityIndex];

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (titleText != null)
            titleText.text = "City " + (cityIndex + 1);

        if (pendingText != null)
            pendingText.text = GetPendingDisasterText(city);

        if (activeText != null)
            activeText.text = GetActiveDisasterText(city);

        SetAlpha(1f);
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOutRoutine(2f, 1f));
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

    string GetPendingDisasterText(CityState city)
    {
        if (city.pendingDisaster == null || !city.pendingDisaster.IsValid())
            return "No pending disaster currently";

        string disasterName = FormatDisasterName(city.pendingDisaster.type);
        int turns = city.pendingDisaster.turnsUntilActivation;
        string turnText = turns == 1 ? "turn" : "turns";

        return "Pending disaster: " + disasterName + " in " + turns + " " + turnText;
    }

    string GetActiveDisasterText(CityState city)
    {
        string result;

        if (city.activeDisaster == null || !city.activeDisaster.IsValid() || !city.activeDisaster.IsActive())
        {
            return "No disaster happening currently.";
        }
        else {
            string disasterName = FormatDisasterName(city.activeDisaster.type);
            int turns = city.activeDisaster.activeTurnsRemaining;
            string turnText = turns == 1 ? "turn" : "turns";

            result = "Active disaster: " + disasterName + " (" + turns + " " + turnText + " left)";
        }

        if (city.blackoutActive)
        {
            result += "\nThere's a blackout happening. ";
        }

        return result;
    }

    string FormatDisasterName(DisasterType type)
    {
        switch (type)
        {
            case DisasterType.StrongWind:
                return "Strong Wind";
            case DisasterType.Earthquake:
                return "Earthquake";
            case DisasterType.Rain:
                return "Heavy Rain";
            case DisasterType.Wildfire:
                return "Wildfire";
            case DisasterType.Drought:
                return "Drought";
            case DisasterType.Depression:
                return "Tropical Depression";
            case DisasterType.Hurricane:
                return "Hurricane";
            case DisasterType.Flood:
                return "Flood";
            case DisasterType.Tsunami:
                return "Tsunami";
            default:
                return "Unknown";
        }
    }
}
