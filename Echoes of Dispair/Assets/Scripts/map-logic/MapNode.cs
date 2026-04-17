using UnityEngine;

public enum MapNodeType
{
    FirstDeity,
    Choice,
    FinalDeity
}

public class MapNode : MonoBehaviour
{
    public MapNodeType nodeType;
    public bool isInteractable = false;

    [Header("Renderer")]
    public SpriteRenderer spriteRenderer;
    public Collider nodeCollider;

    [Header("Sprite")]
    public Sprite normalSprite;

    [Header("Materials")]
    public Material highlightMaterial;

    [Header("Colors")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    
    private Material originalMaterial;

    private bool isSelected = false;
    private bool isHovered = false;

    private void Awake()
    {
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.sharedMaterial;
            spriteRenderer.sprite = normalSprite;
        }

        if (nodeCollider != null)
            nodeCollider.enabled = true;

        isInteractable = true;
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

    private void UpdateVisual()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.sprite = normalSprite;
        spriteRenderer.color = isInteractable ? activeColor : inactiveColor;

        if (isSelected || (isHovered && isInteractable))
            spriteRenderer.material = highlightMaterial;
        else
            spriteRenderer.material = originalMaterial;
    }

    private void OnMouseEnter()
    {
        Debug.Log("mouse enter");
        isHovered = true;
        UpdateVisual();
    }

    private void OnMouseExit()
    {
        Debug.Log("mouse exit");
        isHovered = false;
        UpdateVisual();
    }

    private void OnMouseDown()
    {
        Debug.Log("mouse down");
        if (!isInteractable)
            return;

        MapManager.Instance.OnNodeClicked(this);
    }
}