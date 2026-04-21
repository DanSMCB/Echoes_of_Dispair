using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public enum MapNodeType
{
    FirstDeity,
    Choice,
    FinalDeity
}

public enum RewardType
{
    None,
    Health,
    Card
}

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider))]
public class MapNode : MonoBehaviour
{
    public MapNodeType nodeType;
    public bool isInteractable = false;

    [Header("Colors")]
    public Color activeColor = Color.black;
    public Color inactiveColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    public Color highlightColor = new Color(1f, 0.2f, 0.254902f, 1f);

    [Header("Reward")]
    public RewardType rewardType = RewardType.None;
    public int healthRewardAmount = 5;
    public string cardRewardId = "";

    public GameObject healthReward;
    public GameObject cardReward;

    private SpriteRenderer spriteRenderer;
    private Collider nodeCollider;

    private bool isSelected = false;
    private bool isHovered = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        nodeCollider = GetComponent<Collider>();

        UpdateVisual();
    }

    public void SetInteractable(bool value)
    {
        isInteractable = value;

        if (nodeCollider != null)
            nodeCollider.enabled = value;

        UpdateVisual();
    }

    public void SetSelected(bool value)
    {
        isSelected = value;
        UpdateVisual();
    }

    public void SetDisabled(bool value)
    {
        isInteractable = value;
        UpdateVisual();
    }

    public void SetHovered(bool value)
    {
        isHovered = value;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null)
            return;

        if (isSelected)
        {
            spriteRenderer.color = highlightColor;
        }
        else if (isHovered && isInteractable)
        {
            if (nodeType == MapNodeType.Choice)
            {
                if (rewardType == RewardType.Health)
                {
                    healthReward.SetActive(true);
                }
                else if (rewardType == RewardType.Card)
                {
                    cardReward.SetActive(true);
                }
            }
            
            spriteRenderer.color = highlightColor;
        }
        else
        {
            if (nodeType == MapNodeType.Choice)
            {
                if (rewardType == RewardType.Health)
                {
                    healthReward.SetActive(false);
                }
                else if (rewardType == RewardType.Card)
                {
                    cardReward.SetActive(false);
                }
            }

            spriteRenderer.color = isInteractable ? activeColor : inactiveColor;
        }
    }
}