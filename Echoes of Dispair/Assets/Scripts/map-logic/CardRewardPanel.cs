using UnityEngine;

public class CardRewardPanel : MonoBehaviour
{
    [Header("Options")]
    public RewardCardOptionUI optionA;
    public RewardCardOptionUI optionB;
    public RewardCardOptionUI optionC;

    public void RefreshFromRunProgress()
    {

        if (RogueliteManager.Instance == null || CardRewardDatabase.Instance == null)
        {
            return;
        }

        var options = RogueliteManager.Instance.runProgress.currentRewardCardOptions;

        if (options.Count < 3)
        {
            return;
        }

        var entryA = CardRewardDatabase.Instance.GetEntryById(options[0]);
        var entryB = CardRewardDatabase.Instance.GetEntryById(options[1]);
        var entryC = CardRewardDatabase.Instance.GetEntryById(options[2]);

        Debug.Log("Entry A: " + (entryA != null ? entryA.cardId : "NULL"));
        Debug.Log("Entry B: " + (entryB != null ? entryB.cardId : "NULL"));
        Debug.Log("Entry C: " + (entryC != null ? entryC.cardId : "NULL"));

        if (entryA != null && optionA != null) optionA.Setup(entryA, this);
        if (entryB != null && optionB != null) optionB.Setup(entryB, this);
        if (entryC != null && optionC != null) optionC.Setup(entryC, this);
    }

    public void SelectReward(string cardId)
    {
        Debug.Log("Reward selected: " + cardId);

        if (RogueliteManager.Instance != null)
        {
            RogueliteManager.Instance.ChooseCardReward(cardId);
            RogueliteManager.Instance.runProgress.currentRewardCardOptions.Clear();
        }

        gameObject.SetActive(false);

        if (MapManager.Instance != null)
        {
            MapManager.Instance.RefreshMapAfterRewardChoice();
        }
    }
}