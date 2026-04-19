using UnityEngine;

public class LearningTracker : MonoBehaviour
{
    public static LearningTracker Instance;

    public int totalCardsPlayed = 0;
    public int wellPlayedCards = 0;

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
}