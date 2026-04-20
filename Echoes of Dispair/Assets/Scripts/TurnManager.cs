using UnityEngine;
using TMPro;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public EnemyManager enemyManager;
    public DeckManager playerDeckManager;

    public TMP_Text roundText;

    [Header("Turn State")]
    public bool isPlayerTurn = true;
    public bool gameEnded = false;

    [Header("Survival Goal")]
    public int currentRound = 1;
    public int targetRoundsToSurvive = 10;

    public GameObject playerTurn;
    public int playerCardsPlayedThisTurn = 0;
    public int maxCardsPerTurn = 2;

    public EndGamePanel victoryPanel;
    public EndGamePanel defeatPanel;

    public UnityEngine.UI.Button endTurnButton;



    void Awake()
    {
        Instance = this;
        roundText.text = "Survive " + ((targetRoundsToSurvive - currentRound) + 1) + " more rounds to win!";
    }

    public void EndPlayerTurn()
    {
        if (gameEnded)
            return;

        if (!isPlayerTurn)
            return;

        if (GameManager.Instance.IsEvacuationMode())
            return;

        if (endTurnButton != null) endTurnButton.interactable = false;

        isPlayerTurn = false;
        StartEnemyTurn();
    }

    void StartEnemyTurn()
    {
        if (gameEnded)
            return;

        playerTurn.SetActive(false);

        StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(2f);

        if (enemyManager != null)
            enemyManager.PlayTurn();
    }

    public void EndEnemyTurn()
    {
        if (gameEnded)
            return;

        CityManager.Instance.AdvanceDisastersOneTurn();

        CheckLoseCondition();
        if (gameEnded)
            return;

        AdvanceRound();

        CheckWinCondition();
        if (gameEnded)
            return;

        isPlayerTurn = true;
        StartPlayerTurn();
    }

    void StartPlayerTurn()
    {
        playerTurn.SetActive(true);

        playerCardsPlayedThisTurn = 0;

        if (endTurnButton != null) endTurnButton.interactable = true;

        if (playerDeckManager != null) playerDeckManager.DrawCard();
    }

    void AdvanceRound()
    {
        currentRound++;
        roundText.text = "Survive " + ((targetRoundsToSurvive - currentRound) + 1) + " more rounds to win!";
    }

    void CheckWinCondition()
    {
        if (currentRound > targetRoundsToSurvive)
        {
            gameEnded = true;

            if (RogueliteManager.Instance != null)
            {
                Debug.Log("Current battle stage: " + RogueliteManager.Instance.currentBattleStage);
                if (RogueliteManager.Instance.currentBattleStage == BattleStage.FirstDeity)
                {
                    RogueliteManager.Instance.MarkFirstBattleWon();
                }
                else if (RogueliteManager.Instance.currentBattleStage == BattleStage.FinalDeity)
                {
                    RogueliteManager.Instance.OnFinalBattleWon();
                }
            }

            if (victoryPanel != null)
                victoryPanel.Show();
        }
    }

    void CheckLoseCondition()
    {
        if (CityManager.Instance.currentPlayerHealth <= 0)
        {
            gameEnded = true;

            if (RogueliteManager.Instance != null)
            {
                RogueliteManager.Instance.ResetRunProgress();
            }

            if (defeatPanel != null)
                defeatPanel.Show();
        }
    }
}