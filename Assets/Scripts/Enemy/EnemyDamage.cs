using System;
using System.Collections;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("Базовые характеристики")]
    public float baseHealth = 10f;
    public float baseDamage = 1f;
    public float baseDefence = 0f;
    public int baseExpValue = 1;

    [Header("Множители сложности")]
    [SerializeField] private bool scaleWithDifficulty = true;
    [SerializeField] private float healthMultiplierPerWave = 0.15f;
    [SerializeField] private float damageMultiplierPerWave = 0.1f;

    [Header("Визуальные эффекты")]
    [SerializeField] private GameObject expPrefab;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color critDamageColor = Color.yellow;
    [SerializeField] private float textSize = 1f;
    [SerializeField] private float shrinkDuration = 0.5f;

    // Текущие характеристики
    [System.NonSerialized] public float health;
    [System.NonSerialized] public float damage;
    [System.NonSerialized] public float defence;
    [System.NonSerialized] public int expValue;

    // События
    public event Action<EnemyDamage> OnDeath;
    public event Action<EnemyDamage, float> OnDamaged;

    // Ссылки на компоненты
    private Animator animator;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;
    private DamageTextManager textManager;
    private EnemyClass enemyMovement;

    // Внутренние переменные
    private bool isDead = false;
    private Vector3 originalScale;
    private float currentDifficultyMod = 0;
    private int currentWaveNumber = 1;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        textManager = GetComponent<DamageTextManager>();
        enemyMovement = GetComponent<EnemyClass>();
        originalScale = transform.localScale;
    }

    // Публичный метод для инициализации врага
    public void InitializeEnemy(float difficultyMod = 0, int waveNumber = 1)
    {
        currentDifficultyMod = difficultyMod;
        currentWaveNumber = waveNumber;
        ResetEnemy();
    }

    private void ResetEnemy()
    {
        // Сброс состояния
        isDead = false;
        transform.localScale = originalScale;

        // Включаем компоненты
        if (enemyCollider != null) enemyCollider.enabled = true;
        if (rb != null) rb.simulated = true;
        if (enemyMovement != null) enemyMovement.enabled = true;
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        // Рассчитываем характеристики
        float waveMultiplier = 1f + (currentWaveNumber - 1) * healthMultiplierPerWave;
        float damageMultiplier = 1f + (currentWaveNumber - 1) * damageMultiplierPerWave;

        health = baseHealth * (scaleWithDifficulty ? waveMultiplier : 1f);
        damage = baseDamage * (scaleWithDifficulty ? damageMultiplier : 1f);
        defence = baseDefence;
        expValue = Mathf.Max(1, Mathf.RoundToInt(baseExpValue * (1 + currentDifficultyMod * 0.05f)));

        // Дополнительная настройка
        if (PlayerStats.Instance != null && scaleWithDifficulty)
        {
            health *= (1 + PlayerStats.Instance.difficultyMod * 0.1f);
            damage *= (1 + PlayerStats.Instance.difficultyMod * 0.1f);
        }
    }

    private void OnEnable()
    {
        // Автоматическая инициализация при активации объекта
        if (health <= 0) // Если объект был переиспользован
        {
            ResetEnemy();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDead && other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(float incomingDamage, float defShred = 0f)
    {
        if (isDead) return;

        defence = Mathf.Max(0, defence - defShred);
        float finalDamage = Mathf.Max(1f, incomingDamage - defence);

        // Критический удар
        bool isCrit = false;
        float currentTextSize = textSize;

        if (PlayerStats.Instance != null && UnityEngine.Random.Range(0f, 100f) <= PlayerStats.Instance.critRate)
        {
            finalDamage *= 1 + (PlayerStats.Instance.critDamage / 100f);
            isCrit = true;
            currentTextSize *= 1.5f;
        }

        health -= finalDamage;

        // Визуальная обратная связь
        if (textManager != null)
        {
            int damageText = Mathf.RoundToInt(finalDamage);
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            Color textColor = isCrit ? critDamageColor : damageColor;
            textManager.CreateDamageText(damageText, spawnPos, textColor, currentTextSize);
        }

        OnDamaged?.Invoke(this, finalDamage);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Отключаем компоненты
        if (enemyCollider != null) enemyCollider.enabled = false;
        if (rb != null) rb.simulated = false;
        if (enemyMovement != null) enemyMovement.enabled = false;

        // Анимация смерти
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        StartCoroutine(DeathSequenceCoroutine());
    }

    private IEnumerator DeathSequenceCoroutine()
    {
        if (animator != null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Эффект сжатия
        float timer = 0f;
        Vector3 startScale = transform.localScale;

        while (timer < shrinkDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / shrinkDuration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            yield return null;
        }

        SpawnExp();
        OnDeath?.Invoke(this);

        // Уничтожаем или деактивируем объект
        HandleDeath();
    }

    private void SpawnExp()
    {
        //if (expPrefab == null) return;

        //// Простой Instantiate - работает всегда
        //GameObject expInstance = Instantiate(expPrefab, transform.position, Quaternion.identity);
        //CollectibleItem expScript = expInstance.GetComponent<CollectibleItem>();
        //if (expScript != null)
        //{
        //    expScript.value = expValue;
        //}
        if (ItemPoolManager.Instance != null)
        {
            ItemPoolManager.Instance.TrySpawnItem(
                CollectibleItem.ItemType.Exp,
                transform.position,
                expValue
            );
        }
    }

    private void HandleDeath()
    {
        // Если есть менеджер пула, используем его, иначе уничтожаем объект
        if (EnemyPoolManager.Instance != null)
        {
            EnemyPoolManager.Instance.ReturnEnemyToPool(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Метод для возврата в пул (вызывается из менеджера пула)
    public void ReturnToPool()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
}