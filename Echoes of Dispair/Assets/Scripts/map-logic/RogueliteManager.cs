using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BattleStage
{
    None,
    FirstDeity,
    FinalDeity
}

public class RogueliteManager : MonoBehaviour
{
    public static RogueliteManager Instance;

    [Header("Run Progress")]
    public RunProgressData runProgress = new RunProgressData();

    [Header("Permanent Progress")]
    public MetaProgressData metaProgress = new MetaProgressData();

    [Header("Current Battle")]
    public BattleStage currentBattleStage = BattleStage.None;

    public RunStats runStats = new RunStats();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetRunProgress()
    {
        runProgress = new RunProgressData();
        Debug.Log("Run progress reset.");
    }

    public void MarkFirstBattleWon()
    {
        runProgress.firstBattleWon = true;
        runProgress.currentMapState = MapProgressState.ChoicesUnlocked;
    }

    public void ChooseHealthReward(int amount)
    {
        metaProgress.bonusMaxHealth += amount;

        runProgress.rewardChosen = true;
        runProgress.finalBattleUnlocked = true;
        runProgress.currentMapState = MapProgressState.FinalDeityUnlocked;
    }

    public void ChooseCardReward(string cardId)
    {
        if (!metaProgress.permanentCardRewards.Contains(cardId))
            metaProgress.permanentCardRewards.Add(cardId);

        runProgress.rewardChosen = true;
        runProgress.finalBattleUnlocked = true;
        runProgress.currentMapState = MapProgressState.FinalDeityUnlocked;
    }

    public void GenerateRewardCardOptions()
    {
        if (CardRewardDatabase.Instance == null)
        {
            return;
        }

        runProgress.currentRewardCardOptions.Clear();

        List<CardRewardDatabase.CardRewardEntry> randomOptions =
            CardRewardDatabase.Instance.GetRandomRewardOptions(3);

        foreach (var option in randomOptions)
        {
            runProgress.currentRewardCardOptions.Add(option.cardId);
        }

        Debug.Log("Generated reward card options: " + string.Join(", ", runProgress.currentRewardCardOptions));
    }

    public void OnFinalBattleWon()
    {
        Debug.Log("Final battle won.");
    }

    public void AccumulateBattleStats()
    {
        runStats.totalCardsPlayed += LearningTracker.Instance.totalCardsPlayed;
        runStats.totalGoodCards += LearningTracker.Instance.wellPlayedCards;

        runStats.totalDamageTaken += LearningTracker.Instance.totalDamageTaken;
        runStats.totalDamagePrevented += LearningTracker.Instance.totalDamagePrevented;
    }

    public float GetGlobalCardEfficiency()
    {
        if (runStats.totalCardsPlayed == 0)
            return 0f;

        return (float)runStats.totalGoodCards / runStats.totalCardsPlayed * 100f;
    }

    public float GetGlobalMitigationRate()
    {
        int total = runStats.totalDamageTaken + runStats.totalDamagePrevented;

        if (total == 0)
            return 0f;

        return (float)runStats.totalDamagePrevented / total * 100f;
    }
}

[System.Serializable]
public class RunStats
{
    public int totalCardsPlayed = 0;
    public int totalGoodCards = 0;

    public int totalDamageTaken = 0;
    public int totalDamagePrevented = 0;
}