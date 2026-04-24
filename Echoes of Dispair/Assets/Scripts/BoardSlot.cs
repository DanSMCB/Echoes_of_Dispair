using UnityEngine;

public class BoardSlot : MonoBehaviour
{
    public int slotIndex;
    public int cityIndex;
    public bool belongsToEnemy;
    public bool isFrontRow;
    public bool isBackRow;

    public GameObject highlightVisual;
    public GameObject playEffectPrefab;

    [HideInInspector] public bool isAvailable = false;
    [HideInInspector] public GameObject currentCard = null;

    public bool IsEmpty()
    {
        return currentCard == null;
    }

    public void SetAvailable(bool value)
    {
        isAvailable = value;

        if (highlightVisual != null)
            highlightVisual.SetActive(value);
    }

    public void PlaceCard(GameObject card)
    {
        currentCard = card;
    }

    public void RemoveCard()
    {
        currentCard = null;
    }

    public void PlayCardEffect()
    {

        Debug.Log("Playing effect on slot " + name);
        if (playEffectPrefab == null)
            return;

        GameObject effect = Instantiate(playEffectPrefab, transform.position + Vector3.up*2, Quaternion.identity);
        effect.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        effect.transform.SetParent(transform);
        effect.transform.localPosition = Vector3.zero;
    }
}