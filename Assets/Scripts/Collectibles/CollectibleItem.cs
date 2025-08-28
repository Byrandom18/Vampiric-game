using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleItem : MonoBehaviour, IPoolable
{
    public enum ItemType { Exp, Gold, Magnet, Heart, Bomb, Equipment, Gem }
    public ItemType itemType;

    [Header("Value Settings")]
    public float value = 1;
    public int minValue = 1;
    public int maxValue = 1;
    public static float totalStoredExp = 0f;

    [Header("Limit Settings")]
    public static int maxItemsOnScene = 500;
    public static int currentItemsCount = 0;
    public float mergeRadius = 1.5f;
    public float mergeCheckInterval = 0.3f;

    [Header("Movement Settings")]
    public float initialWaitTime = 0.5f;
    public float attractionRadius = 2f;
    public float baseSpeed = 8f;
    public float maxSpeed = 15f;
    public float acceleration = 2f;

    [Header("Bomb Settings")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float bombDamageMod = 5f;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private GameObject mergeEffect;

    // Компоненты
    private Transform player;
    private Rigidbody2D rb;
    private Collider2D itemCollider;
    private SpriteRenderer spriteRenderer;

    // Состояния
    private bool isAttracted = false;
    private bool canBeCollected = false;
    private bool isMerging = false;
    private float currentSpeed;

    // Корутины
    private Coroutine magnetCoroutine;
    private Coroutine mergeCheckCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        itemCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (itemCollider == null)
        {
            Debug.LogError("CollectibleItem requires a Collider2D!");
        }
    }

    private void OnEnable()
    {
        currentItemsCount++;
        InitializeItem();
    }

    private void OnDisable()
    {
        currentItemsCount--;
        StopAllCoroutines();
        ResetState();
    }

    public void Initialize(ItemType type, float itemValue = 0)
    {
        itemType = type;
        value = itemValue > 0 ? itemValue : Random.Range(minValue, maxValue + 1);
        InitializeItem();
    }

    private void InitializeItem()
    {
        FindPlayer();
        ResetState();
        SetVisuals();
        StartCoroutines();
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void ResetState()
    {
        isAttracted = false;
        canBeCollected = false;
        isMerging = false;
        currentSpeed = baseSpeed;

        // Сбрасываем физику
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
        }

        // Включаем коллайдер
        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }
    }

    private void SetVisuals()
    {
        if (spriteRenderer != null)
        {
            // Масштабируем в зависимости от значения
            float scaleFactor = 0.8f + Mathf.Log(value + 1) * 0.3f;
            transform.localScale = Vector3.one * Mathf.Clamp(scaleFactor, 0.8f, 3f);
        }
    }

    private void StartCoroutines()
    {
        // Останавливаем старые корутины
        if (mergeCheckCoroutine != null) StopCoroutine(mergeCheckCoroutine);
        if (magnetCoroutine != null) StopCoroutine(magnetCoroutine);

        // Запускаем новые
        mergeCheckCoroutine = StartCoroutine(MergeCheckCoroutine());
        StartCoroutine(InitialWaitCoroutine());
    }

    private IEnumerator InitialWaitCoroutine()
    {
        yield return new WaitForSeconds(initialWaitTime);
        canBeCollected = true;

        // Включаем физику после задержки
        if (rb != null)
        {
            rb.simulated = true;
        }
    }

    private IEnumerator MergeCheckCoroutine()
    {
        WaitForSeconds wait = new WaitForSeconds(mergeCheckInterval);

        while (true)
        {
            yield return wait;

            if (!isAttracted && canBeCollected && !isMerging && itemType == ItemType.Exp)
            {
                CheckForMerge();
            }
        }
    }

    private void CheckForMerge()
    {
        if (isMerging) return;

        // Ищем все предметы опыта в радиусе
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, mergeRadius);

        foreach (Collider2D collider in nearbyColliders)
        {
            if (collider == null || collider.gameObject == gameObject) continue;

            CollectibleItem otherItem = collider.GetComponent<CollectibleItem>();
            if (otherItem != null &&
                otherItem.itemType == ItemType.Exp &&
                otherItem.canBeCollected &&
                !otherItem.isMerging &&
                !isMerging)
            {
                StartCoroutine(MergeItemsCoroutine(otherItem));
                break;
            }
        }
    }

    private IEnumerator MergeItemsCoroutine(CollectibleItem otherItem)
    {
        if (otherItem == null || otherItem.isMerging) yield break;

        // Помечаем оба предмета как объединяющиеся
        isMerging = true;
        otherItem.isMerging = true;

        // Определяем главный предмет (с большим значением)
        CollectibleItem mainItem = value >= otherItem.value ? this : otherItem;
        CollectibleItem secondaryItem = value >= otherItem.value ? otherItem : this;

        // Отключаем физику и коллайдеры для плавного движения
        if (mainItem.rb != null) mainItem.rb.simulated = false;
        if (secondaryItem.rb != null) secondaryItem.rb.simulated = false;
        if (secondaryItem.itemCollider != null) secondaryItem.itemCollider.enabled = false;

        Vector3 startPosition = secondaryItem.transform.position;
        float mergeTime = 0.5f;
        float timer = 0f;

        // Эффект объединения
        if (mergeEffect != null)
        {
            Instantiate(mergeEffect, mainItem.transform.position, Quaternion.identity);
        }

        // Анимация перемещения
        while (timer < mergeTime && mainItem != null && secondaryItem != null)
        {
            timer += Time.deltaTime;
            float progress = timer / mergeTime;

            if (secondaryItem != null)
            {
                secondaryItem.transform.position = Vector3.Lerp(
                    startPosition,
                    mainItem.transform.position,
                    progress
                );
            }

            yield return null;
        }

        // Завершение объединения
        if (mainItem != null && secondaryItem != null)
        {
            // Суммируем значения
            mainItem.value += secondaryItem.value;
            mainItem.SetVisuals();

            // Восстанавливаем главный предмет
            if (mainItem.rb != null) mainItem.rb.simulated = true;
            mainItem.isMerging = false;

            // Возвращаем второй предмет в пул
            secondaryItem.isMerging = false;
            secondaryItem.ReturnToPool();
        }
        else
        {
            // Защита от ошибок
            if (mainItem != null) mainItem.isMerging = false;
            if (secondaryItem != null) secondaryItem.isMerging = false;
        }
    }

    private void Update()
    {
        if (!canBeCollected || isMerging) return;

        // Поиск игрока если он потерян
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Автопритяжение при близком расстоянии
        if (distanceToPlayer <= attractionRadius && !isAttracted)
        {
            isAttracted = true;
        }

        // Движение к игроку
        if (isAttracted)
        {
            MoveTowardsPlayer(distanceToPlayer);
        }
    }

    private void MoveTowardsPlayer(float distanceToPlayer)
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;

        // Плавное ускорение при приближении
        float speedMultiplier = Mathf.Lerp(1f, 2f, 1 - (distanceToPlayer / attractionRadius));
        currentSpeed = Mathf.Min(maxSpeed, baseSpeed * speedMultiplier);

        if (rb != null)
        {
            rb.linearVelocity = direction * currentSpeed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canBeCollected && !isMerging)
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        if (PlayerStats.Instance == null || !canBeCollected || isMerging) return;

        switch (itemType)
        {
            case ItemType.Exp:
                PlayerStats.Instance.AddExp(value);
                break;
            case ItemType.Gold:
                PlayerStats.Instance.AddGold(value);
                break;
            case ItemType.Gem:
                PlayerStats.Instance.AddGems((int)value);
                break;
            case ItemType.Magnet:
                MagnetActivation();
                break;
            case ItemType.Heart:
                PlayerStats.Instance.AddHealth(PlayerStats.Instance.maxHealth * 0.3f);
                break;
            case ItemType.Bomb:
                Explode();
                break;
        }

        ReturnToPool();
    }

    public void ForceAttract()
    {
        if (!canBeCollected || isMerging) return;
        isAttracted = true;
    }

    public void MagnetActivation()
    {
        if (magnetCoroutine != null)
            StopCoroutine(magnetCoroutine);

        magnetCoroutine = StartCoroutine(MagnetEffectCoroutine());
    }

    private IEnumerator MagnetEffectCoroutine()
    {
        float magnetDuration = 10f;
        float originalRadius = attractionRadius;

        // Увеличиваем радиус притяжения
        attractionRadius *= 3f;

        // Применяем накопленный опыт
        ApplyStoredExp();

        // Притягиваем все предметы
        CollectibleItem[] allItems = FindObjectsByType<CollectibleItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (CollectibleItem item in allItems)
        {
            if (item != this && item.canBeCollected && !item.isMerging)
            {
                item.ForceAttract();
                item.currentSpeed = Mathf.Min(item.maxSpeed, item.currentSpeed * 1.5f);
            }
        }

        yield return new WaitForSeconds(magnetDuration);

        // Восстанавливаем радиус
        attractionRadius = originalRadius;
    }

    public static void ApplyStoredExp()
    {
        if (totalStoredExp > 0 && PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddExp(totalStoredExp);
            totalStoredExp = 0f;
        }
    }

    private void Explode()
    {
        // Визуальный эффект
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Поиск врагов в радиусе
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                EnemyDamage enemyDamage = enemy.GetComponent<EnemyDamage>();
                if (enemyDamage != null)
                {
                    float damage = PlayerStats.Instance.GetTotalDamage() * bombDamageMod;
                    enemyDamage.TakeDamage(damage, PlayerStats.Instance.defShred);
                }
            }
        }
    }

    public void OnSpawn()
    {
        // Реализация интерфейса IPoolable
    }

    public void ReturnToPool()
    {
        // Сбрасываем состояние перед возвратом
        ResetState();

        if (ItemPoolManager.Instance != null)
        {
            ItemPoolManager.Instance.ReturnItemToPool(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetValue(float newValue)
    {
        value = newValue;
        SetVisuals();
    }

    public void SetSpeed(float newSpeed)
    {
        baseSpeed = newSpeed;
        currentSpeed = newSpeed;
    }

    // Метод для принудительного сброса состояния
    public void ForceReset()
    {
        isMerging = false;
        canBeCollected = true;

        if (itemCollider != null) itemCollider.enabled = true;
        if (rb != null) rb.simulated = true;
    }

    // Визуализация в редакторе
    private void OnDrawGizmosSelected()
    {
        // Радиус притяжения
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);

        // Радиус объединения
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, mergeRadius);

        // Радиус взрыва (для бомб)
        if (itemType == ItemType.Bomb)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}