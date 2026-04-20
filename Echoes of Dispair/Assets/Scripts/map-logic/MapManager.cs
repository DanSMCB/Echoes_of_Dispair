using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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

    [Header("Raycast")]
    public Camera mainCamera;
    public LayerMask nodeLayerMask = ~0;

    private MapProgressState CurrentState
    {
        get
        {
            if (RogueliteManager.Instance == null)
                return MapProgressState.Start;

            return RogueliteManager.Instance.runProgress.currentMapState;
        }
    }
    private MapNode currentHoveredNode;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (RogueliteManager.Instance != null)
        {
            Debug.Log("MapManager Start - CurrentState = " + CurrentState +
                      " | RogueliteManager ID: " + RogueliteManager.Instance.GetInstanceID());
        }
        else
        {
            Debug.LogError("MapManager Start - RogueliteManager.Instance is NULL");
        }
        ApplyState(CurrentState);
    }

    private void OnEnable()
    {
        Debug.Log("MapManager OnEnable called");
    }

    private void Update()
    {
        HandleHoverAndClick();
    }

    private void HandleHoverAndClick()
    {
        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        MapNode hitNode = null;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, nodeLayerMask))
        {
            hitNode = hit.collider.GetComponent<MapNode>();
        }

        if (hitNode != currentHoveredNode)
        {
            if (currentHoveredNode != null)
                currentHoveredNode.SetHovered(false);

            currentHoveredNode = hitNode;

            if (currentHoveredNode != null)
                currentHoveredNode.SetHovered(true);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && hitNode != null)
        {
            Debug.Log("Clicked node: " + hitNode.name);
            OnNodeClicked(hitNode);
        }
    }

    public void OnNodeClicked(MapNode clickedNode)
    {
        if (clickedNode == null || !clickedNode.isInteractable)
            return;

        switch (clickedNode.nodeType)
        {
            case MapNodeType.FirstDeity:
                if (CurrentState == MapProgressState.Start)
                {
                    Debug.Log("Entering first deity battle...");
                    if (RogueliteManager.Instance != null)
                    {
                        RogueliteManager.Instance.currentBattleStage = BattleStage.FirstDeity;
                    }
                    SceneManager.LoadScene(battleSceneName);
                }
                break;

            case MapNodeType.Choice:
                if (CurrentState == MapProgressState.ChoicesUnlocked)
                {
                    Debug.Log("Choice selected: " + clickedNode.name);

                    choiceNodeA.SetSelected(clickedNode == choiceNodeA);
                    choiceNodeB.SetSelected(clickedNode == choiceNodeB);

                    if (RogueliteManager.Instance != null)
                    {
                        switch (clickedNode.rewardType)
                        {
                            case RewardType.Health:
                                RogueliteManager.Instance.ChooseHealthReward(clickedNode.healthRewardAmount);
                                break;

                            case RewardType.Card:
                                RogueliteManager.Instance.ChooseCardReward(clickedNode.cardRewardId);
                                break;
                        }
                    }

                    ApplyState(CurrentState);
                }
                break;

            case MapNodeType.FinalDeity:
                if (CurrentState == MapProgressState.FinalDeityUnlocked)
                {
                    Debug.Log("Entering final deity battle...");
                    if (RogueliteManager.Instance != null)
                    {
                        RogueliteManager.Instance.currentBattleStage = BattleStage.FinalDeity;
                    }
                    SceneManager.LoadScene(battleSceneName);
                }
                break;
        }
    }

    public void UnlockChoices()
    {
        if (RogueliteManager.Instance != null)
        {
            RogueliteManager.Instance.runProgress.firstBattleWon = true;
            RogueliteManager.Instance.runProgress.currentMapState = MapProgressState.ChoicesUnlocked;
        }
        ApplyState(CurrentState);
    }

    public void ResetMapState()
    {
        if (RogueliteManager.Instance != null)
        {
            RogueliteManager.Instance.ResetRunProgress();
        }

        ApplyState(CurrentState);
    }

    private void ApplyState(MapProgressState state)
    {
        Debug.Log("ApplyState called with: " + state);
        if (firstDeityNode == null || choiceNodeA == null || choiceNodeB == null || finalDeityNode == null)
        {
            Debug.LogWarning("MapManager: missing node references.");
            return;
        }

        switch (state)
        {
            case MapProgressState.Start:
                Debug.Log("Applying START state");
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
                Debug.Log("Applying CHOICES UNLOCKED state");
                firstDeityNode.SetInteractable(false);
                choiceNodeA.SetInteractable(true);
                choiceNodeB.SetInteractable(true);
                finalDeityNode.SetInteractable(false);

                firstDeityNode.SetSelected(false);
                finalDeityNode.SetSelected(false);
                break;

            case MapProgressState.FinalDeityUnlocked:
                Debug.Log("Applying FINAL DEITY UNLOCKED state");
                firstDeityNode.SetInteractable(false);
                choiceNodeA.SetInteractable(false);
                choiceNodeB.SetInteractable(false);
                finalDeityNode.SetInteractable(true);
                break;
        }
    }

    [ContextMenu("DEBUG Unlock Choices")]
    private void DebugUnlockChoices()
    {
        UnlockChoices();
    }

    [ContextMenu("DEBUG Reset Map")]
    private void DebugResetMap()
    {
        ResetMapState();
    }
}