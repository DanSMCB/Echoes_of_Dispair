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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("RogueliteManager CREATED and persisted. Instance ID: " + GetInstanceID());
        }
        else
        {
            Debug.Log("Duplicate RogueliteManager DESTROYED. Instance ID: " + GetInstanceID());
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);
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

        Debug.Log("MarkFirstBattleWon CALLED. State is now: " + runProgress.currentMapState +
              " | Manager ID: " + GetInstanceID());
    }

    public void ChooseHealthReward(int amount)
    {
        metaProgress.bonusMaxHealth += amount;

        runProgress.rewardChosen = true;
        runProgress.finalBattleUnlocked = true;
        runProgress.currentMapState = MapProgressState.FinalDeityUnlocked;

        Debug.Log("Permanent health reward gained: +" + amount);
    }

    public void ChooseCardReward(string cardId)
    {
        if (!metaProgress.permanentCardRewards.Contains(cardId))
            metaProgress.permanentCardRewards.Add(cardId);

        runProgress.rewardChosen = true;
        runProgress.finalBattleUnlocked = true;
        runProgress.currentMapState = MapProgressState.FinalDeityUnlocked;

        Debug.Log("Permanent card reward gained: " + cardId);
    }

    public void OnFinalBattleWon()
    {
        Debug.Log("Final battle won.");
        // mais tarde podes decidir aqui se:
        // - fecha a demo
        // - mostra ecrã final
        // - começa novo ciclo
    }
}