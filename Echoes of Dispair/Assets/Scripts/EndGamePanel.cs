using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGamePanel : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 2f;

    public TMP_Text efficientCardsRateText;

    public string mapSceneName = "MapScene";

    void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        efficientCardsRateText.text = $"Cards efficiency rate: {LearningTracker.Instance.GetAccuracy():F1}%";
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void ReturnToMap()
    {
        Debug.Log("ReturnToMap called. Loading scene: " + mapSceneName);
        SceneManager.LoadScene(mapSceneName);
    }
}