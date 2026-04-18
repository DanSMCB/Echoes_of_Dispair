using UnityEngine;

public class CityState
{
    public int cityIndex;
    public int elevation;

    public DisasterInstance pendingDisaster;
    public DisasterInstance activeDisaster;

    public GameObject pendingDisasterCard;
    public GameObject activeDisasterCard;

    public int emergencyGeneratorTurns;
    public GameObject emergencyGeneratorCard;

    public int stayShelteredTurns;
    public GameObject stayShelteredCard;

    public int powerCutOffTurns;
    public GameObject powerCutOffCard;

    public int emergencyKitTurns;
    public GameObject emergencyKitCard;

    public int stayHydratedTurns;
    public GameObject stayHydratedCard;

    public int sandbagsTurns;
    public GameObject sandbagsCard;

    public int barricadeTurns;
    public GameObject barricadeCard;

    public int parchedEarthTurns;
    public GameObject parchedEarthCard;

    public int rapidIntensificationTurns;
    public GameObject rapidIntensificationCard;

    public int seismicEchoesTurns;
    public GameObject seismicEchoesCard;

    public void Initialize(int index, int cityElevation)
    {
        cityIndex = index;
        elevation = cityElevation;

        pendingDisaster = null;
        activeDisaster = null;

        pendingDisasterCard = null;
        activeDisasterCard = null;

        emergencyGeneratorTurns = 0;
        emergencyGeneratorCard = null;

        stayShelteredTurns = 0;
        stayShelteredCard = null;

        powerCutOffTurns = 0;
        powerCutOffCard = null;

        emergencyKitTurns = 0;
        emergencyKitCard = null;

        stayHydratedTurns = 0;
        stayHydratedCard = null;

        sandbagsTurns = 0;
        sandbagsCard = null;

        barricadeTurns = 0;
        barricadeCard = null;

        parchedEarthTurns = 0;
        parchedEarthCard = null;

        rapidIntensificationTurns = 0;
        rapidIntensificationCard = null;

        seismicEchoesTurns = 0;
        seismicEchoesCard = null;
    }
}
