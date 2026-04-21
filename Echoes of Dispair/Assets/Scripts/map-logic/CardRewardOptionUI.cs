using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RewardCardOptionUI : MonoBehaviour
{
    public Image cardImage;
    public Button button;

    private string cardId;
    private CardRewardPanel rewardPanel;

    public Image imagePreview;

    private RectTransform rectTransform;
    private bool wasHovering;

    void Awake()
    {
        imagePreview.enabled = false;
        rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(CardRewardDatabase.CardRewardEntry entry, CardRewardPanel panel)
    {
        rewardPanel = panel;
        cardId = entry.cardId;

        Debug.Log("Setup reward option: " + entry.cardId +
                  " | sprite = " + (entry.cardSprite != null ? entry.cardSprite.name : "NULL") +
                  " | image = " + (cardImage != null ? cardImage.name : "NULL"));

        if (cardImage != null)
            cardImage.sprite = entry.cardSprite;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }

        gameObject.SetActive(true);
    }

    void OnClicked()
    {
        imagePreview.enabled = false;
        if (rewardPanel != null)
            rewardPanel.SelectReward(cardId);
    }

    void Update()
    {
        if (rectTransform == null || Mouse.current == null)
            return;

        bool isHovering = RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform,
            Mouse.current.position.ReadValue(),
            null
        );

        if (isHovering && !wasHovering)
        {

            if (imagePreview != null && cardImage != null)
            {
                imagePreview.sprite = cardImage.sprite;
                imagePreview.enabled = true;
            }
        }
        else if (!isHovering && wasHovering)
        {

            if (imagePreview != null)
            {
                imagePreview.sprite = null;
                imagePreview.enabled = false;
            }
        }

        wasHovering = isHovering;
    }
}