using UnityEngine;

public class LearningTracker : MonoBehaviour
{
    public static LearningTracker Instance;

    int totalCardsPlayed = 0;
    int wellPlayedCards = 0;

    int totalDamageTaken = 0;
    int totalDamagePrevented = 0;

    void Awake()
    {
        Instance = this;
    }

    public void RegisterCardPlay(bool wasEffective)
    {
        totalCardsPlayed++;

        if (wasEffective)
            wellPlayedCards++;
    }

    public float GetAccuracy()
    {
        if (totalCardsPlayed == 0)
            return 0f;

        return (float)wellPlayedCards / totalCardsPlayed * 100f;
    }

    public void AddPreventedDamage(int amount)
    {
        totalDamagePrevented += amount;
    }

    public void AddTotalDamage(int amount)
    {
        totalDamageTaken += amount;
    }

    public float GetMitigationRate()
    {
        int total = totalDamageTaken + totalDamagePrevented;

        if (total == 0)
            return 0f;

        return (float)totalDamagePrevented / total * 100f;
    }
}