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

    private MapProgressState currentState = MapProgressState.Start;
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

        ApplyState(currentState);
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
                if (currentState == MapProgressState.Start)
                {
                    Debug.Log("Entering first deity battle...");
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

    public void ResetMapState()
    {
        currentState = MapProgressState.Start;
        ApplyState(currentState);
    }

    private void ApplyState(MapProgressState state)
    {
        if (firstDeityNode == null || choiceNodeA == null || choiceNodeB == null || finalDeityNode == null)
        {
            Debug.LogWarning("MapManager: missing node references.");
            return;
        }

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

                firstDeityNode.SetSelected(false);
                finalDeityNode.SetSelected(false);
                break;

            case MapProgressState.FinalDeityUnlocked:
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