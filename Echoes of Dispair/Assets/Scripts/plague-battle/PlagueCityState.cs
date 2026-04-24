using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlagueCityState
{
    public int cityIndex;

    public bool hasBaseVirus;
    public GameObject baseVirusCard;

    public List<PlagueThreatInstance> pendingThreats = new List<PlagueThreatInstance>();
    public List<GameObject> pendingThreatCards = new List<GameObject>();

    public List<PlagueThreatInstance> activeThreats = new List<PlagueThreatInstance>();
    public List<GameObject> activeThreatCards = new List<GameObject>();

    public int rapidMutationTurns;
    public GameObject rapidMutationCard;

    public int selfIsolationTurns;
    public GameObject selfIsolationCard;

    public int maskUsageTurns;
    public GameObject maskUsageCard;

    public int handWashingTurns;
    public GameObject handWashingCard;

    public int useSanitizerTurns;
    public GameObject useSanitizerCard;

    public int avoidCrowdsTurns;
    public GameObject avoidCrowdsCard;

    public int stockEssentialsTurns;
    public GameObject stockEssentialsCard;

    public int boilWaterTurns;
    public GameObject boilWaterCard;

    public int vaccinationTurns;
    public GameObject vaccinationCard;

    public int lockdownMeasuresTurns;
    public GameObject lockdownMeasuresCard;

    public void Initialize(int index)
    {
        cityIndex = index;

        hasBaseVirus = false;
        baseVirusCard = null;

        pendingThreats.Clear();
        pendingThreatCards.Clear();

        activeThreats.Clear();
        activeThreatCards.Clear();

        rapidMutationTurns = 0;
        rapidMutationCard = null;

        selfIsolationTurns = 0;
        selfIsolationCard = null;

        maskUsageTurns = 0;
        maskUsageCard = null;

        handWashingTurns = 0;
        handWashingCard = null;

        useSanitizerTurns = 0;
        useSanitizerCard = null;

        avoidCrowdsTurns = 0;
        avoidCrowdsCard = null;

        stockEssentialsTurns = 0;
        stockEssentialsCard = null;

        boilWaterTurns = 0;
        boilWaterCard = null;

        vaccinationTurns = 0;
        vaccinationCard = null;

        lockdownMeasuresTurns = 0;
        lockdownMeasuresCard = null;
    }
}