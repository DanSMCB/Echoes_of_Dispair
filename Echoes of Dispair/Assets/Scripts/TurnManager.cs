using UnityEngine;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public EnemyManager enemyManager;
    public TMP_Text roundText;

    [Header("Turn State")]
    public bool isPlayerTurn = true;
    public bool gameEnded = false;

    [Header("Survival Goal")]
    public int currentRound = 1;
    public int targetRoundsToSurvive = 10;

    public GameObject playerTurn;

    public EndGamePanel victoryPanel;
    public EndGamePanel defeatPanel;

    void Awake()
    {
        Instance = this;
        roundText.text = "Survive " + ((targetRoundsToSurvive - currentRound) + 1) + " more rounds to win!";
    }

    public void EndPlayerTurn()
    {
        if (gameEnded)
            return;

        isPlayerTurn = false;
        StartEnemyTurn();
    }

    void StartEnemyTurn()
    {
        if (gameEnded)
            return;

        playerTurn.SetActive(false);

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

            if (victoryPanel != null)
                victoryPanel.Show();
        }
    }

    void CheckLoseCondition()
    {
        if (CityManager.Instance.currentPlayerHealth <= 0)
        {
            gameEnded = true;

            if (defeatPanel != null)
                defeatPanel.Show();
        }
    }
}