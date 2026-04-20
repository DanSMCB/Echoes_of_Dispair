using System.Collections.Generic;
using UnityEngine;

public class CardRewardDatabase : MonoBehaviour
{
    public static CardRewardDatabase Instance;

    [System.Serializable]
    public class CardRewardEntry
    {
        public string cardId;
        public GameObject cardPrefab;
    }

    [Header("Reward Cards")]
    public List<CardRewardEntry> entries = new List<CardRewardEntry>();

    private Dictionary<string, GameObject> cardLookup = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            BuildLookup();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void BuildLookup()
    {
        cardLookup.Clear();

        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.cardId) || entry.cardPrefab == null)
                continue;

            if (!cardLookup.ContainsKey(entry.cardId))
            {
                cardLookup.Add(entry.cardId, entry.cardPrefab);
            }
            else
            {
                Debug.LogWarning("Duplicate card reward ID found: " + entry.cardId);
            }
        }
    }

    public GameObject GetCardPrefabById(string id)
    {
        foreach (var entry in entries)
        {
            if (entry.cardId == id)
                return entry.cardPrefab;
        }

        return null;
    }
}