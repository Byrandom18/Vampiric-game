using System.Collections;
using UnityEngine;

public class EnemyClass : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float damage;

    [Header("Настройки сближения")]
    [SerializeField] private bool enableClosing = false;
    [SerializeField] private float closingDistance = 3f;
    [SerializeField] private float forceDuration = 1f;
    [SerializeField] private float forceCD = 5f;
    [SerializeField] private float forcePower = 2f;

    [Header("Оптимизация")]
    [SerializeField] private float updateInterval = 0.1f; // Частота обновления AI
    [SerializeField] private float teleportDistance = 25f;

    // Ссылки на компоненты
    private EnemyDamage enemyDamage;
    private Rigidbody2D rb;
    private Transform playerTransform;
    private Vector2 originalScale;

    // Переменные состояния
    private float currentSpeed;
    private bool canHitInside = true;
    private bool isClosing = false;
    private bool canClosing = true;
    private Coroutine aiCoroutine;

    // Кешированные векторы для избежания аллокации в Update
    private Vector2 directionToPlayer;
    private Vector2 newVelocity;

    private void Awake()
    {
        // Переносим инициализацию компонентов в Awake, это более корректно
        rb = GetComponent<Rigidbody2D>();
        enemyDamage = GetComponent<EnemyDamage>();
        originalScale = transform.localScale;
    }

    private void Start()
    {
        // Кешируем трансформ игрока. Используем более безопасный поиск.
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Enemy: Player not found! Make sure the player has the 'Player' tag.");
            this.enabled = false; // Отключаем скрипт, если игрок не найден
            return;
        }

        // Инициализация скорости и урона
        currentSpeed = baseSpeed;
        if (enemyDamage != null)
            damage = enemyDamage.damage;
    }

    private void OnEnable()
    {
        // Сброс состояний при включении/переиспользовании объекта (важно для Object Pooling)
        isClosing = false;
        //canClosing = true;
        currentSpeed = baseSpeed;
        rb.linearVelocity = Vector2.zero;

        // Запускаем корутину AI
        if (aiCoroutine != null) StopCoroutine(aiCoroutine);
        aiCoroutine = StartCoroutine(AIUpdateCoroutine());
    }

    private void OnDisable()
    {
        // Останавливаем корутину и сбрасываем velocity при выключении объекта
        if (aiCoroutine != null) StopCoroutine(aiCoroutine);
        rb.linearVelocity = Vector2.zero;
    }

    // Основная корутина для ИИ
    private IEnumerator AIUpdateCoroutine()
    {
        WaitForSeconds wait = new WaitForSeconds(updateInterval); // Кешируем ожидание для оптимизации

        while (true)
        {
            yield return wait; // Ждем заданный интервал
            ChaseBehavior();
        }
    }

    // Логика преследования
    private void ChaseBehavior()
    {
        if (playerTransform == null) return;

        // Вычисляем дистанцию и направление К ОДНОМУ РАЗУ
        directionToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude; // .magnitude дешевле .Distance

        // Телепортация если игрок слишком далеко
        if (distanceToPlayer > teleportDistance)
        {
            transform.position += (Vector3)directionToPlayer.normalized * (teleportDistance * 0.9f);
            return;
        }

        // Нормализуем направление только если оно нужно (после проверки дистанции)
        directionToPlayer.Normalize();

        // Логика сближения (рывка)
        if (enableClosing && canClosing && distanceToPlayer < closingDistance)
        {
            StartCoroutine(ClosingRushCoroutine(directionToPlayer));
            return; // Выходим, чтобы основное движение не мешало рывку
        }

        // Основное преследование (работает, только если не в состоянии рывка)
        if (!isClosing)
        {
            // Задаем скорость напрямую. Это предпочтительнее, чем AddForce для простого движения.
            newVelocity = directionToPlayer * currentSpeed;
            rb.linearVelocity = newVelocity;

            // Поворот спрайта
            if (directionToPlayer.x != 0)
            {
                transform.localScale = new Vector3(
                    Mathf.Sign(directionToPlayer.x) * originalScale.x,
                    originalScale.y,
                    1
                );
            }
        }

        // Проверка близости для урона
        if (distanceToPlayer < 0.4f && canHitInside)
        {
            StartCoroutine(InsideDamageCheckCoroutine());
        }
    }

    private IEnumerator InsideDamageCheckCoroutine()
    {
        canHitInside = false;
        yield return new WaitForSeconds(0.2f);

        if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) < 0.2f)
        {
            // Более безопасный вызов. Ищем компонент на игроке в момент атаки.
            PlayerStats playerStats = playerTransform.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }
        }
        yield return new WaitForSeconds(0.1f); // Небольшая задержка между проверками
        canHitInside = true;
    }

    // Корутина рывка
    private IEnumerator ClosingRushCoroutine(Vector2 direction)
    {
        canClosing = false;
        isClosing = true; // Флаг, что мы в рывке. AIUpdate теперь не будет трогать velocity.

        // СИЛЬНЫЙ РЫВОК. AddForce с Impulse идеально подходит.
        // Очищаем текущую скорость, чтобы импульс был четким.
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * (currentSpeed * forcePower), ForceMode2D.Impulse);

        // Ждем пока длится импульс рывка
        yield return new WaitForSeconds(forceDuration);

        // Гасим скорость после рывка. Иначе враг будет лететь вечно.
        rb.linearVelocity = Vector2.zero;
        isClosing = false; // Возвращаем контроль AIUpdate

        // Временное замедление
        float slowedSpeed = baseSpeed * 0.5f;
        currentSpeed = slowedSpeed;

        // Ждем восстановления способности к рывку
        yield return new WaitForSeconds(forceCD);

        // Восстанавливаем обычную скорость
        currentSpeed = baseSpeed;
        canClosing = true;
    }

    // Метод для внешнего изменения скорости (например, от эффектов)
    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeed = baseSpeed * multiplier;
    }

    // Метод для сброса скорости
    public void ResetSpeed()
    {
        currentSpeed = baseSpeed;
    }
}