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

    private void Start()
    {
        InitializeSystem();
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
    /// Показывает панель выбора карточек при получении уровня
    /// </summary>
    public void ShowCardSelection()
    {
        if (isSelecting) return;

        Time.timeScale = 0f;
        isSelecting = true;
        selectionTimer = selectionTime;

        // Активируем панель
        if (cardSelectionPanel != null)
        {
            cardSelectionPanel.SetActive(true);
        }

        // Обновляем текст
        UpdateLevelUpText();

        // Показываем таймер
        if (timerFillImage != null && useTimer)
        {
            timerFillImage.gameObject.SetActive(true);
            timerFillImage.fillAmount = 1f;
        }

        // Генерируем и отображаем карточки
        GenerateCardOptions();
        DisplayCards();

        // Запускаем корутину таймера
        if (useTimer)
        {
            selectionCoroutine = StartCoroutine(SelectionTimerCoroutine());
        }
    }

    /// <summary>
    /// Генерирует доступные варианты карточек
    /// </summary>
    private void GenerateCardOptions()
    {
        currentCardOptions.Clear();

        // Получаем доступные карты
        List<CardData> availableCards = GetAvailableCards();

        // Если карт меньше чем нужно показать - показываем все
        int cardsCount = Mathf.Min(cardsToShow, availableCards.Count);

        for (int i = 0; i < cardsCount; i++)
        {
            // Выбираем случайную карту из доступных
            int randomIndex = Random.Range(0, availableCards.Count);
            CardData selectedCard = availableCards[randomIndex];

            currentCardOptions.Add(selectedCard);
            availableCards.RemoveAt(randomIndex);
        }
    }

    /// <summary>
    /// Возвращает список доступных для выбора карт
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
    /// Проверяет, доступна ли карта для выбора
    /// </summary>
    private bool IsCardAvailable(CardData card)
    {
        // Проверяем требования уровня
        if (PlayerLevelSystem.Instance != null &&
            PlayerLevelSystem.Instance.currentLevel < card.requiredLevel)
        {
            return false;
        }

        // Для карт разблокировки оружия проверяем, не разблокировано ли уже оружие
        if (card.isWeaponUnlock)
        {
            if (WeaponManager.Instance != null &&
                WeaponManager.Instance.IsWeaponUnlocked(card.weaponType))
            {
                return false;
            }
        }

        // Проверяем требования по другим картам
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

        // Проверяем максимальный уровень
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
    /// Отображает карточки в UI
    /// </summary>
    private void DisplayCards()
    {
        // Очищаем предыдущие карточки
        ClearCurrentCards();

        // Создаем новые карточки
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
    /// Очищает текущие карточки
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
    /// Обработчик выбора карточки игроком
    /// </summary>
    public void SelectCard(CardData selectedCard)
    {
        if (!isSelecting) return;

        ApplyCardEffect(selectedCard);
        CloseSelection();
    }

    /// <summary>
    /// Применяет эффект выбранной карточки
    /// </summary>
    private void ApplyCardEffect(CardData card)
    {
        // Сохраняем в прогрессе
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.AddSelectedCard(card);
        }

        // Применяем эффект карты
        if (card.isWeaponUnlock)
        {
            // Разблокируем оружие
            if (WeaponManager.Instance != null)
            {
                WeaponManager.Instance.ApplyCardEffect(card);
            }
        }
        else if (card.statValue != 0)
        {
            // Улучшаем статы игрока
            if (PlayerStatsManager.Instance != null)
            {
                PlayerStatsManager.Instance.UpgradeStat(card.statType, card.statValue);
            }
        }
        else
        {
            // Улучшаем оружие
            if (WeaponManager.Instance != null)
            {
                WeaponManager.Instance.ApplyCardEffect(card);
            }
        }

        // Воспроизводим звук или эффект
        PlaySelectionEffect();
    }

    /// <summary>
    /// Закрывает панель выбора
    /// </summary>
    private void CloseSelection()
    {
        if (selectionCoroutine != null)
        {
            StopCoroutine(selectionCoroutine);
        }

        Time.timeScale = 1f;
        isSelecting = false;

        // Деактивируем панель
        if (cardSelectionPanel != null)
        {
            cardSelectionPanel.SetActive(false);
        }

        // Скрываем таймер
        if (timerFillImage != null)
        {
            timerFillImage.gameObject.SetActive(false);
        }

        // Очищаем карточки
        ClearCurrentCards();

        // Уведомляем другие системы о завершении выбора
        OnSelectionComplete();
    }

    /// <summary>
    /// Корутина таймера выбора
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
    /// Автоматический выбор случайной карты при таймауте
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
    /// Обновляет текст таймера
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
                levelUpText.text = "LEVEL UP! Choose an upgrade:";
            }
        }
    }

    /// <summary>
    /// Обновляет визуальное отображение таймера
    /// </summary>
    private void UpdateSelectionTimer()
    {
        if (timerFillImage != null)
        {
            timerFillImage.fillAmount = selectionTimer / selectionTime;
        }
    }

    /// <summary>
    /// Воспроизводит эффект выбора карты
    /// </summary>
    private void PlaySelectionEffect()
    {
        // Здесь можно добавить звук, частицы и т.д.
        // Debug.Log("Card selected: " + selectedCard.cardName);
    }

    /// <summary>
    /// Вызывается после завершения выбора
    /// </summary>
    private void OnSelectionComplete()
    {
        // Уведомляем другие системы
        // Debug.Log("Card selection completed");
    }

    /// <summary>
    /// Принудительно закрывает выбор карточек
    /// </summary>
    public void ForceCloseSelection()
    {
        CloseSelection();
    }

    /// <summary>
    /// Проверяет, активно ли сейчас окно выбора
    /// </summary>
    public bool IsSelecting()
    {
        return isSelecting;
    }

    /// <summary>
    /// Устанавливает время для выбора
    /// </summary>
    public void SetSelectionTime(float time)
    {
        selectionTime = Mathf.Max(1f, time);
    }

    /// <summary>
    /// Устанавливает количество показываемых карточек
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