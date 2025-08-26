using UnityEngine;
using System.Collections.Generic;

public class PlayerProgress : MonoBehaviour
{
    public static PlayerProgress Instance;

    [System.Serializable]
    public class CardProgress
    {
        public CardData card;
        public int level;
    }

    public List<CardProgress> selectedCards = new List<CardProgress>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddSelectedCard(CardData card)
    {
        CardProgress existing = selectedCards.Find(c => c.card == card);

        if (existing != null)
        {
            existing.level = Mathf.Min(existing.level + 1, card.maxLevel);
        }
        else
        {
            selectedCards.Add(new CardProgress { card = card, level = 1 });
        }
    }

    public int GetCardLevel(CardData card)
    {
        CardProgress progress = selectedCards.Find(c => c.card == card);
        return progress != null ? progress.level : 0;
    }

    public bool HasCard(CardData card)
    {
        return selectedCards.Exists(c => c.card == card);
    }
}