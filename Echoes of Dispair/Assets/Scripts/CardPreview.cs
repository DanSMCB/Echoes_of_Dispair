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
        previewObject.SetActive(false);
    }

    public void ShowCard(Sprite cardSprite)
    {
        previewImage.sprite = cardSprite;
        previewObject.SetActive(true);
    }

    public void HideCard()
    {
        previewObject.SetActive(false);
    }
}