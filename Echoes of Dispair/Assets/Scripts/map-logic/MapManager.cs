using UnityEngine;
using UnityEngine.SceneManagement;

public enum MapProgressState
{
    Start,
    ChoicesUnlocked,
    FinalDeityUnlocked
}

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("Nodes")]
    public MapNode firstDeityNode;
    public MapNode choiceNodeA;
    public MapNode choiceNodeB;
    public MapNode finalDeityNode;

    [Header("Scene Names")]
    public string battleSceneName = "BattleScene";

    private MapProgressState currentState = MapProgressState.Start;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ApplyState(currentState);
    }

    public void OnNodeClicked(MapNode clickedNode)
    {
        switch (clickedNode.nodeType)
        {
            case MapNodeType.FirstDeity:
                if (currentState == MapProgressState.Start)
                {
                    Debug.Log("Entering first deity battle...");
                    // aqui depois ligas à tua battle
                    SceneManager.LoadScene(battleSceneName);
                }
                break;

            case MapNodeType.Choice:
                if (currentState == MapProgressState.ChoicesUnlocked)
                {
                    Debug.Log("Choice selected: " + clickedNode.name);

                    choiceNodeA.SetSelected(clickedNode == choiceNodeA);
                    choiceNodeB.SetSelected(clickedNode == choiceNodeB);

                    currentState = MapProgressState.FinalDeityUnlocked;
                    ApplyState(currentState);
                }
                break;

            case MapNodeType.FinalDeity:
                if (currentState == MapProgressState.FinalDeityUnlocked)
                {
                    Debug.Log("Entering final deity battle...");
                    SceneManager.LoadScene(battleSceneName);
                }
                break;
        }
    }

    public void UnlockChoices()
    {
        currentState = MapProgressState.ChoicesUnlocked;
        ApplyState(currentState);
    }

    private void ApplyState(MapProgressState state)
    {
        switch (state)
        {
            case MapProgressState.Start:
                firstDeityNode.SetInteractable(true);
                choiceNodeA.SetInteractable(false);
                choiceNodeB.SetInteractable(false);
                finalDeityNode.SetInteractable(false);

                firstDeityNode.SetSelected(false);
                choiceNodeA.SetSelected(false);
                choiceNodeB.SetSelected(false);
                finalDeityNode.SetSelected(false);
                break;

            case MapProgressState.ChoicesUnlocked:
                firstDeityNode.SetInteractable(false);
                choiceNodeA.SetInteractable(true);
                choiceNodeB.SetInteractable(true);
                finalDeityNode.SetInteractable(false);
                break;

            case MapProgressState.FinalDeityUnlocked:
                firstDeityNode.SetInteractable(false);
                choiceNodeA.SetInteractable(false);
                choiceNodeB.SetInteractable(false);
                finalDeityNode.SetInteractable(true);
                break;
        }
    }
}