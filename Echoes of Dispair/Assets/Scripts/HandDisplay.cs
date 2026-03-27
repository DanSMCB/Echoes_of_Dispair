using UnityEngine;

public class HandDisplay : MonoBehaviour
{
    public float spacing = 0.5f;      // distância horizontal entre cartas
    public float fanAngle = 20f;      // ângulo máximo para rotação do leque
    public float moveSpeed = 5f;      // velocidade da animação das cartas
    public float maxHandWidth = 5f;   // largura máxima do leque

    void Start()
    {
        UpdateHand();
    }

    public void UpdateHand()
    {
        int cardCount = transform.childCount;

        for (int i = 0; i < cardCount; i++)
        {
            Transform card = transform.GetChild(i);

            // Normaliza posição no leque (0 a 1)
            float t = (cardCount == 1) ? 0.5f : (float)i / (cardCount - 1);

            // calcula largura proporcional
            float totalWidth = Mathf.Min(spacing * (cardCount - 1), maxHandWidth);
            float xPos = Mathf.Lerp(-totalWidth / 2f, totalWidth / 2f, t);

            float angle = Mathf.Lerp(-fanAngle, fanAngle, t);

            float depthOffset = i * 0.01f;
            float yOffset = -Mathf.Abs(angle) * 0.02f;
            Vector3 targetPos = new Vector3(xPos, yOffset, depthOffset);
            Quaternion targetRot = Quaternion.Euler(0, 180, angle);

            card.localScale = Vector3.one;

            // Inicia coroutine para animar suavemente
            StartCoroutine(AnimateCard(card, targetPos, targetRot));
        }
    }

    private System.Collections.IEnumerator AnimateCard(Transform card, Vector3 targetPos, Quaternion targetRot)
    {
        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 startPos = card.localPosition;
        Quaternion startRot = card.localRotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            card.localPosition = Vector3.Lerp(startPos, targetPos, t);
            card.localRotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }

        // Garante posição final
        card.localPosition = targetPos;
        card.localRotation = targetRot;
    }
}
