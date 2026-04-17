using UnityEngine;
using UnityEngine.UI;

public class CardPreview : MonoBehaviour
{
    public static CardPreview Instance;

    public GameObject previewObject;
    public Image previewImage;

    void Awake()
    {
        Instance = this;
        previewImage.enabled = false;
    }

    public void ShowCard(Sprite cardSprite)
    {
        previewImage.sprite = cardSprite;
        previewImage.enabled = true;

    }

    public void HideCard()
    {
        previewImage.enabled = false;
    }
}