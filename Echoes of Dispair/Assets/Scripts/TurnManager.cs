using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public EnemyManager enemyManager;

    public bool isPlayerTurn = true;

    void Awake()
    {
        Instance = this;
    }

    public void EndPlayerTurn()
    {
        isPlayerTurn = false;
        StartEnemyTurn();
    }

    void StartEnemyTurn()
    {
        Debug.Log("Turno do inimigo");
        enemyManager.PlayTurn();
    }

    public void EndEnemyTurn()
    {
        isPlayerTurn = true;
        StartPlayerTurn();
    }

    void StartPlayerTurn()
    {
        Debug.Log("Turno do jogador");
    }
}