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
        public Sprite cardSprite;
        public string cardName;
    }

    [Header("Reward Cards")]
    public List<CardRewardEntry> entries = new List<CardRewardEntry>();

    private Dictionary<string, CardRewardEntry> cardLookup = new Dictionary<string, CardRewardEntry>();

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
                cardLookup.Add(entry.cardId, entry);
            }
        }
    }

    public GameObject GetCardPrefabById(string cardId)
    {
        if (string.IsNullOrEmpty(cardId))
            return null;

        if (cardLookup.TryGetValue(cardId, out CardRewardEntry entry))
            return entry.cardPrefab;

        return null;
    }

    public CardRewardEntry GetEntryById(string cardId)
    {
        if (string.IsNullOrEmpty(cardId))
            return null;

        if (cardLookup.TryGetValue(cardId, out CardRewardEntry entry))
            return entry;

        return null;
    }

    public List<CardRewardEntry> GetRandomRewardOptions(int count)
    {
        List<CardRewardEntry> pool = new List<CardRewardEntry>(entries);
        List<CardRewardEntry> result = new List<CardRewardEntry>();

        count = Mathf.Min(count, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            result.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }

        return result;
    }
}