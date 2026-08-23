using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CardSelectionSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject cardSelectionPanel;
    public Transform cardsContainer;
    public GameObject cardPrefab;
    public TextMeshProUGUI levelUpText;
    public Image timerFillImage;

    [Header("Selection Settings")]
    public int cardsToShow = 3;
    public float selectionTime = 10f;
    public bool useTimer = true;
    public bool autoSelectOnTimeout = true;

    [Header("Card Pool")]
    public List<CardData> allCards = new List<CardData>();

    private List<CardData> currentCardOptions = new List<CardData>();
    private List<GameObject> currentCardInstances = new List<GameObject>();
    private bool isSelecting = false;
    private float selectionTimer;
    private Coroutine selectionCoroutine;

    public static CardSelectionSystem Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isSelecting && useTimer)
        {
            UpdateSelectionTimer();
        }
    }

    private void InitializeSystem()
    {
        if (cardSelectionPanel != null)
        {
            cardSelectionPanel.SetActive(false);
        }

        if (timerFillImage != null)
        {
            timerFillImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜ ˜˜˜ ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜
    /// </summary>
    public void ShowCardSelection()
    {
        if (isSelecting) return;

        Time.timeScale = 0f;
        isSelecting = true;
        selectionTimer = selectionTime;

        // ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜
        if (cardSelectionPanel != null)
        {
            cardSelectionPanel.SetActive(true);
        }

        // ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜
        UpdateLevelUpText();

        // ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜
        if (timerFillImage != null && useTimer)
        {
            timerFillImage.gameObject.SetActive(true);
            timerFillImage.fillAmount = 1f;
        }

        // ˜˜˜˜˜˜˜˜˜˜ ˜ ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜
        GenerateCardOptions();
        DisplayCards();

        // ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜
        if (useTimer)
        {
            selectionCoroutine = StartCoroutine(SelectionTimerCoroutine());
        }
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜
    /// </summary>
    private void GenerateCardOptions()
    {
        currentCardOptions.Clear();

        // ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜
        List<CardData> availableCards = GetAvailableCards();

        // ˜˜˜˜ ˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜ ˜˜˜˜˜ ˜˜˜˜˜˜˜˜ - ˜˜˜˜˜˜˜˜˜˜ ˜˜˜
        int cardsCount = Mathf.Min(cardsToShow, availableCards.Count);

        for (int i = 0; i < cardsCount; i++)
        {
            // ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜ ˜˜˜˜˜˜˜˜˜
            int randomIndex = Random.Range(0, availableCards.Count);
            CardData selectedCard = availableCards[randomIndex];

            currentCardOptions.Add(selectedCard);
            availableCards.RemoveAt(randomIndex);
        }
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜ ˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜
    /// </summary>
    private List<CardData> GetAvailableCards()
    {
        List<CardData> availableCards = new List<CardData>();

        foreach (CardData card in allCards)
        {
            if (IsCardAvailable(card))
            {
                availableCards.Add(card);
            }
        }

        return availableCards;
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜, ˜˜˜˜˜˜˜˜ ˜˜ ˜˜˜˜˜ ˜˜˜ ˜˜˜˜˜˜
    /// </summary>
    private void Start()
    {
        InitializeSystem();
        InjectGeneratedCards();
    }

    private void InjectGeneratedCards()
    {
        var grenade = ScriptableObject.CreateInstance<CardData>();
        grenade.cardName = "˜˜˜˜˜˜˜";
        grenade.description = "Unlock Grenade Weapon";
        grenade.isWeaponUnlock = true;
        grenade.weaponType = WeaponType.ArmorBreak;
        grenade.resultWeaponId = (int)Vampiric.Weapons.WeaponId.Grenade;
        grenade.maxLevel = 1;
        allCards.Add(grenade);

        var catalog = Vampiric.Game.GameContentBootstrap.Instance != null
            ? Vampiric.Game.GameContentBootstrap.Instance.Evolutions
            : Vampiric.Weapons.WeaponEvolutionCatalog.CreateDefault();
        foreach (var recipe in catalog.Recipes)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = recipe.DisplayName;
            card.description = recipe.Description;
            card.isEvolution = true;
            card.isWeaponUnlock = true;
            card.firstWeaponId = (int)recipe.First;
            card.secondWeaponId = (int)recipe.Second;
            card.resultWeaponId = (int)recipe.Result;
            card.maxLevel = 1;
            allCards.Add(card);
        }
    }

    private bool IsCardAvailable(CardData card)
    {
        if (card == null)
        {
            return false;
        }

        if (PlayerStats.Instance != null && PlayerStats.Instance.lvl < card.requiredLevel)
        {
            return false;
        }

        if (card.isEvolution)
        {
            var loadout = Vampiric.Weapons.WeaponFireDirector.Instance?.Loadout;
            if (loadout == null)
            {
                return false;
            }

            return loadout.IsMaxLevel((Vampiric.Weapons.WeaponId)card.firstWeaponId) &&
                   loadout.IsMaxLevel((Vampiric.Weapons.WeaponId)card.secondWeaponId) &&
                   !loadout.IsUnlocked((Vampiric.Weapons.WeaponId)card.resultWeaponId);
        }

        if (card.resultWeaponId == (int)Vampiric.Weapons.WeaponId.Grenade)
        {
            var loadout = Vampiric.Weapons.WeaponFireDirector.Instance?.Loadout;
            return loadout != null && !loadout.IsUnlocked(Vampiric.Weapons.WeaponId.Grenade);
        }

        // ˜˜˜ ˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜, ˜˜ ˜˜˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜ ˜˜˜ ˜˜˜˜˜˜
        if (card.isWeaponUnlock)
        {
            if (WeaponManager.Instance != null &&
                WeaponManager.Instance.IsWeaponUnlocked(card.weaponType))
            {
                return false;
            }
        }

        // ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜ ˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜˜
        if (card.requiredCards != null && card.requiredCards.Length > 0)
        {
            foreach (CardData requiredCard in card.requiredCards)
            {
                if (PlayerProgress.Instance == null ||
                    !PlayerProgress.Instance.HasCard(requiredCard))
                {
                    return false;
                }
            }
        }

        // ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜
        if (PlayerProgress.Instance != null)
        {
            int currentLevel = PlayerProgress.Instance.GetCardLevel(card);
            if (currentLevel >= card.maxLevel)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜ ˜ UI
    /// </summary>
    private void DisplayCards()
    {
        // ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜
        ClearCurrentCards();

        // ˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜˜˜˜˜˜˜
        foreach (CardData cardData in currentCardOptions)
        {
            GameObject cardInstance = Instantiate(cardPrefab, cardsContainer);
            currentCardInstances.Add(cardInstance);

            CardUI cardUI = cardInstance.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.Initialize(cardData, this);
            }
        }
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜
    /// </summary>
    private void ClearCurrentCards()
    {
        foreach (GameObject cardInstance in currentCardInstances)
        {
            if (cardInstance != null)
            {
                Destroy(cardInstance);
            }
        }
        currentCardInstances.Clear();
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜
    /// </summary>
    public void SelectCard(CardData selectedCard)
    {
        if (!isSelecting) return;

        ApplyCardEffect(selectedCard);
        CloseSelection();
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜
    /// </summary>
    private void ApplyCardEffect(CardData card)
    {
        // ˜˜˜˜˜˜˜˜˜ ˜ ˜˜˜˜˜˜˜˜˜
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.AddSelectedCard(card);
        }

        // ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜
        if (card.isWeaponUnlock)
        {
            // ˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜
            if (WeaponManager.Instance != null)
            {
                WeaponManager.Instance.ApplyCardEffect(card);
            }
        }
        else if (card.statValue != 0)
        {
            // ˜˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜˜˜˜˜
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.UpgradeStat(card.statType, card.statValue);
            }
        }
        else
        {
            // ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜
            if (WeaponManager.Instance != null)
            {
                WeaponManager.Instance.ApplyCardEffect(card);
            }
        }

        // ˜˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜ ˜˜˜ ˜˜˜˜˜˜
        PlaySelectionEffect();
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜˜
    /// </summary>
    private void CloseSelection()
    {
        if (selectionCoroutine != null)
        {
            StopCoroutine(selectionCoroutine);
        }

        Time.timeScale = 1f;
        isSelecting = false;

        // ˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜
        if (cardSelectionPanel != null)
        {
            cardSelectionPanel.SetActive(false);
        }

        // ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜
        if (timerFillImage != null)
        {
            timerFillImage.gameObject.SetActive(false);
        }

        // ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜
        ClearCurrentCards();

        // ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜˜˜ ˜ ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜
        OnSelectionComplete();
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜ ˜˜˜˜˜˜
    /// </summary>
    private IEnumerator SelectionTimerCoroutine()
    {
        while (selectionTimer > 0f)
        {
            selectionTimer -= Time.unscaledDeltaTime;
            UpdateLevelUpText();

            if (timerFillImage != null)
            {
                timerFillImage.fillAmount = selectionTimer / selectionTime;
            }

            yield return null;
        }

        if (autoSelectOnTimeout)
        {
            AutoSelectCard();
        }
        else
        {
            CloseSelection();
        }
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜˜ ˜˜˜˜˜˜˜˜
    /// </summary>
    private void AutoSelectCard()
    {
        if (currentCardOptions.Count > 0)
        {
            int randomIndex = Random.Range(0, currentCardOptions.Count);
            SelectCard(currentCardOptions[randomIndex]);
        }
        else
        {
            CloseSelection();
        }
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜˜˜˜˜˜
    /// </summary>
    private void UpdateLevelUpText()
    {
        if (levelUpText != null)
        {
            if (useTimer)
            {
                levelUpText.text = $"LEVEL UP! Choose an upgrade: {Mathf.CeilToInt(selectionTimer)}s";
            }
            else
            {
                levelUpText.text = $"LEVEL {PlayerStats.Instance.lvl}!";
            }
        }
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜
    /// </summary>
    private void UpdateSelectionTimer()
    {
        if (timerFillImage != null)
        {
            timerFillImage.fillAmount = selectionTimer / selectionTime;
        }
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜
    /// </summary>
    private void PlaySelectionEffect()
    {
        // ˜˜˜˜˜ ˜˜˜˜˜ ˜˜˜˜˜˜˜˜ ˜˜˜˜, ˜˜˜˜˜˜˜ ˜ ˜.˜.
        // Debug.Log("Card selected: " + selectedCard.cardName);
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜
    /// </summary>
    private void OnSelectionComplete()
    {
        if (PlayerStats.Instance.exp > PlayerStats.Instance.maxExp)
            PlayerStats.Instance.LevelUp();
        // ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜˜˜
        // Debug.Log("Card selection completed");
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜˜˜˜˜˜˜
    /// </summary>
    public void ForceCloseSelection()
    {
        CloseSelection();
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜, ˜˜˜˜˜˜˜ ˜˜ ˜˜˜˜˜˜ ˜˜˜˜ ˜˜˜˜˜˜
    /// </summary>
    public bool IsSelecting()
    {
        return isSelecting;
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜˜ ˜˜˜˜˜˜
    /// </summary>
    public void SetSelectionTime(float time)
    {
        selectionTime = Mathf.Max(1f, time);
    }

    /// <summary>
    /// ˜˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜
    /// </summary>
    public void SetCardsToShow(int count)
    {
        cardsToShow = Mathf.Clamp(count, 1, 5);
    }

    private void PositionCardSelectionPanel()
    {
        RectTransform panelRect = cardSelectionPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.1f);
        panelRect.anchorMax = new Vector2(0.9f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
    }
}