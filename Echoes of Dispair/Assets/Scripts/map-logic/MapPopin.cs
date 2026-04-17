using System.Collections;
using UnityEngine;

public class MapPopIn : MonoBehaviour
{
    public float duration = 0.2f;
    public float overshoot = 1.1f;

    private Vector3 targetScale;

    private void Awake()
    {
        targetScale = transform.localScale;
    }

    public void Play()
    {
        StopAllCoroutines();
        StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        transform.localScale = Vector3.zero;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float scaleT = Mathf.SmoothStep(0f, 1f, t);

            float current = Mathf.Lerp(0f, overshoot, scaleT);
            transform.localScale = targetScale * current;
            yield return null;
        }

        transform.localScale = targetScale;
    }
}