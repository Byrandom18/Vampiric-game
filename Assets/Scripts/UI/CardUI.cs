using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image cardBackground;
    public Image cardIcon;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardDescriptionText;
    public Button selectButton;

    [Header("Animation Settings")]
    public float hoverScale = 1.1f;
    public float hoverDuration = 0.2f;

    private CardData currentCardData;
    private CardSelectionSystem selectionSystem;
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    private void Start()
    {
        originalScale = transform.localScale;

        // Назначаем обработчик клика на кнопку
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnCardSelected);
        }
    }

    /// <summary>
    /// Инициализирует карточку данными
    /// </summary>
    public void Initialize(CardData cardData, CardSelectionSystem system)
    {
        currentCardData = cardData;
        selectionSystem = system;

        // Заполняем UI данными из CardData
        if (cardIcon != null && cardData.icon != null)
        {
            cardIcon.sprite = cardData.icon;
        }

        if (cardNameText != null)
        {
            cardNameText.text = cardData.cardName;
        }

        if (cardDescriptionText != null)
        {
            // Получаем текущий уровень карты
            int currentLevel = 0;
            if (PlayerProgress.Instance != null)
            {
                currentLevel = PlayerProgress.Instance.GetCardLevel(cardData);
            }

            cardDescriptionText.text = cardData.GetDescription(currentLevel + 1);
        }

        // Меняем цвет фона в зависимости от типа карты
        UpdateCardAppearance();
    }

    /// <summary>
    /// Обновляет внешний вид карточки в зависимости от типа
    /// </summary>
    private void UpdateCardAppearance()
    {
        if (cardBackground == null) return;

        Color backgroundColor = Color.gray;

        if (currentCardData.isWeaponUnlock)
        {
            backgroundColor = new Color(0.2f, 0.6f, 1f); // Синий для разблокировки
        }
        else if (currentCardData.statValue != 0)
        {
            backgroundColor = new Color(0.8f, 0.4f, 0.2f); // Оранжевый для статов
        }
        else
        {
            backgroundColor = new Color(0.4f, 0.8f, 0.2f); // Зеленый для улучшений
        }

        cardBackground.color = backgroundColor;
    }

    /// <summary>
    /// Обработчик выбора карточки
    /// </summary>
    private void OnCardSelected()
    {
        if (selectionSystem != null && currentCardData != null)
        {
            selectionSystem.SelectCard(currentCardData);
        }
    }

    /// <summary>
    /// Анимация при наведении курсора
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(ScaleAnimation(originalScale * hoverScale));
    }

    /// <summary>
    /// Анимация при уходе курсора
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(ScaleAnimation(originalScale));
    }

    /// <summary>
    /// Корутина для плавного изменения масштаба
    /// </summary>
    private IEnumerator ScaleAnimation(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < hoverDuration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / hoverDuration);
            elapsed += Time.unscaledDeltaTime; // Используем unscaled потому что игра на паузе
            yield return null;
        }

        transform.localScale = targetScale;
        scaleCoroutine = null;
    }

    /// <summary>
    /// Включает или выключает кнопку выбора
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (selectButton != null)
        {
            selectButton.interactable = interactable;
        }
    }

    /// <summary>
    /// Показывает карточку как выбранную
    /// </summary>
    public void ShowAsSelected()
    {
        if (cardBackground != null)
        {
            cardBackground.color = Color.yellow;
        }
    }

    /// <summary>
    /// Сбрасывает визуальное выделение
    /// </summary>
    public void ResetAppearance()
    {
        UpdateCardAppearance();
    }

    /// <summary>
    /// Мгновенно сбрасывает масштаб
    /// </summary>
    public void ResetScale()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }
        transform.localScale = originalScale;
    }
}