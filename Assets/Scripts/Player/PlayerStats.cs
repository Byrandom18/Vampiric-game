using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    public static event System.Action<Transform> OnPlayerSpawned;
    public static event System.Action OnPlayerDeath;

    [Header("UI Elements")]
    public Slider healthBar;
    public Text levelText;
    public Text goldText;
    public Text gemsText;
    public ExpBar expBar;

    [Header("Боевые характеристики")]
    public float health = 100;
    public float baseHealth = 100;
    public float healthMod;
    public float maxHealth;
    public float healthRegen;
    public float baseAtk = 10;
    public float atk = 1;
    public float atkMod;
    public float damageMod;
    public float luck;
    public float critRate = 5;
    public float critDamage = 50;
    public float def;
    public float speedMod;
    public int penetrationBoost;
    public float projectileSpeed;
    public float durations;
    public float cdRed;
    public int addProjectile;
    public float areaMod;
    public float defShred;
    public bool invulnerability = false;

    [Header("Вспомогательные характеристики")]
    public float maxExp = 5;
    public float exp;
    public float expIncrease = 10;
    public int lvl = 1;
    public float gold;
    public int gems;
    public int killCount;
    public int wavesCompleted;

    [Header("Настройки сложности и прогрессии")]
    public float difficultyChange = 1;
    public float difficultyMod = 0f;
    public float difficultyDelay = 30f;
    public float survivalTime = 0f;

    [Header("Модификаторы опыта и дропа")]
    public float expGainMultiplier = 1f;
    public float goldGainMultiplier = 1f;
    public float itemDropChanceMultiplier = 1f;

    // События для UI и других систем
    public event Action OnHealthChanged;
    public event Action OnLevelUp;
    public event Action OnStatsUpdated;
    public event Action OnDeath;

    private Coroutine healthRegenCoroutine;
    private Coroutine difficultyCoroutine;
    private Coroutine survivalTimerCoroutine;
    private bool isDead = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeStats();
        StartCoroutines();
        UpdateAllUI();

        OnPlayerSpawned?.Invoke(transform);
    }

    private void InitializeStats()
    {
        atk = baseAtk * (1 + atkMod / 100);
        maxHealth = baseHealth * (1 + healthMod / 100);
        health = maxHealth;
    }

    private void StartCoroutines()
    {
        if (healthRegenCoroutine != null) StopCoroutine(healthRegenCoroutine);
        if (difficultyCoroutine != null) StopCoroutine(difficultyCoroutine);
        if (survivalTimerCoroutine != null) StopCoroutine(survivalTimerCoroutine);

        healthRegenCoroutine = StartCoroutine(HealthRegenCoroutine());
        difficultyCoroutine = StartCoroutine(DifficultyChangeCoroutine());
        survivalTimerCoroutine = StartCoroutine(SurvivalTimerCoroutine());
    }

    private IEnumerator HealthRegenCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (!isDead && health < maxHealth)
            {
                health = Mathf.Min(health + healthRegen, maxHealth);
                UpdateHealthUI();
                OnHealthChanged?.Invoke();
            }
        }
    }

    private IEnumerator DifficultyChangeCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(difficultyDelay);
            difficultyMod += difficultyChange;
            Debug.Log($"Сложность увеличена: {difficultyMod}");
        }
    }

    private IEnumerator SurvivalTimerCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            survivalTime += 1f;
        }
    }

    public void UpgradeStat(StatType statType, float value)
    {
        switch (statType)
        {
            case StatType.BaseHealth: baseHealth += value; break;
            case StatType.HealthMod: healthMod += value; break;
            case StatType.HealthRegen: healthRegen += value; break;
            case StatType.BaseAtk: baseAtk += value; break;
            case StatType.AtkMod: atkMod += value; break;
            case StatType.DamageMod: damageMod += value; break;
            case StatType.Luck: luck += value; break;
            case StatType.CritRate: critRate = critRate + value; break;
            case StatType.CritDamage: critDamage += value; break;
            case StatType.Def: def += value; break;
            case StatType.PenetrationBoost: penetrationBoost += Mathf.RoundToInt(value); break;
            case StatType.ProjectileSpeed: projectileSpeed += value; break;
            case StatType.Durations: durations += value; break;
            case StatType.CdRed: cdRed = Mathf.Min(99f, cdRed + value); break;
            case StatType.AddProjectile: addProjectile += Mathf.RoundToInt(value); break;
            case StatType.AreaMod: areaMod += value; break;
            case StatType.DefShred: defShred += value; break;
            //case StatType.ExpGain: expGainMultiplier += value / 100f; break;
            //case StatType.GoldGain: goldGainMultiplier += value / 100f; break;
            //case StatType.DropChance: itemDropChanceMultiplier += value / 100f; break;
        }

        UpdateStats();
    }

    private void UpdateStats()
    {
        atk = baseAtk * (1 + atkMod / 100) * (1 + damageMod / 100);
        maxHealth = baseHealth * (1 + healthMod / 100);

        // Ограничиваем здоровье, если максимум уменьшился
        health = Mathf.Min(health, maxHealth);

        UpdateAllUI();
        OnStatsUpdated?.Invoke();
    }

    public void TakeDamage(float damage)
    {
        if (isDead || invulnerability) return;

        damage -= def;
        damage = Mathf.Max(1f, damage * (1 - def / (def + 100f))); // Формула уменьшения урона от защиты

        health -= damage;
        health = Mathf.Max(0, health);

        UpdateHealthUI();
        OnHealthChanged?.Invoke();

        if (health <= 0 && !isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // Останавливаем все корутины
        if (healthRegenCoroutine != null) StopCoroutine(healthRegenCoroutine);
        if (difficultyCoroutine != null) StopCoroutine(difficultyCoroutine);
        if (survivalTimerCoroutine != null) StopCoroutine(survivalTimerCoroutine);

        // Вызываем событие смерти
        OnDeath?.Invoke();
        OnPlayerDeath?.Invoke(); // <- НОВОЕ событие

        //// Показываем экран смерти
        //if (GameManager.Instance != null)
        //{
        //    GameManager.Instance.ShowDeathScreen();
        //}
    }

    public void Revive(float healthPercent = 0.5f)
    {
        isDead = false;
        health = maxHealth * healthPercent;
        UpdateHealthUI();
        StartCoroutines();
    }

    public void AddExp(float value)
    {
        if (isDead) return;

        float actualExp = value * expGainMultiplier;
        exp += actualExp;

        UpdateExpUI();

        if (exp >= maxExp)
        {
            LevelUp();
        }
    }

    public void AddGold(float value)
    {
        float actualGold = value * goldGainMultiplier;
        gold += actualGold;
        UpdateGoldUI();
    }

    public void AddGems(int value)
    {
        gems += value;
        UpdateGemsUI();
    }

    public void AddKill()
    {
        killCount++;
    }

    public void AddWaveCompleted()
    {
        wavesCompleted++;
    }

    public void AddHealth(float healAmount)
    {
        if (isDead) return;

        health = Mathf.Min(health + healAmount, maxHealth);
        UpdateHealthUI();
        OnHealthChanged?.Invoke();
    }

    public void AddMaxHealth(float amount)
    {
        baseHealth += amount;
        maxHealth = baseHealth * (1 + healthMod / 100);
        health = Mathf.Min(health + amount, maxHealth);
        UpdateHealthUI();
        OnStatsUpdated?.Invoke();
    }

    public void LevelUp()
    {
        lvl++;
        exp -= maxExp;

        // Увеличиваем требуемый опыт
        if (lvl % 10 == 0)
            expIncrease += 2;
        maxExp += expIncrease;

        // Восстанавливаем немного здоровья при уровне
        AddHealth(maxHealth * 0.1f);

        UpdateAllUI();
        OnLevelUp?.Invoke();

        // Вызываем систему выбора карточек
        if (CardSelectionSystem.Instance != null)
        {
            CardSelectionSystem.Instance.ShowCardSelection();
        }
    }

    private void UpdateAllUI()
    {
        UpdateHealthUI();
        UpdateExpUI();
        UpdateGoldUI();
        UpdateGemsUI();
        UpdateLevelUI();
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }
    }

    private void UpdateExpUI()
    {
        if (expBar != null)
        {
            expBar.UpdateExpBar(exp, maxExp);
        }
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = gold.ToString("F0");
        }
    }

    private void UpdateGemsUI()
    {
        if (gemsText != null)
        {
            gemsText.text = gems.ToString();
        }
    }

    private void UpdateLevelUI()
    {
        if (levelText != null)
        {
            levelText.text = $"Lvl {lvl}";
        }
    }

    public float GetCritMultiplier()
    {
        return 1 + critDamage / 100f;
    }

    public bool CheckCrit()
    {
        return UnityEngine.Random.Range(0f, 100f) <= critRate;
    }

    public float GetTotalDamage(bool isCrit = false)
    {
        float totalDamage = atk;
        if (isCrit)
        {
            totalDamage *= GetCritMultiplier();
        }
        return totalDamage;
    }

    // Метод для сброса статов (при новой игре)
    public void ResetStats()
    {
        isDead = false;

        // Сбрасываем базовые характеристики
        baseHealth = 100;
        healthMod = 0;
        baseAtk = 10;
        atkMod = 0;
        damageMod = 0;
        luck = 0;
        critRate = 5;
        critDamage = 50;
        def = 0;
        healthRegen = 0;

        // Сбрасываем прогрессию
        lvl = 1;
        exp = 0;
        maxExp = 5;
        gold = 0;
        gems = 0;
        killCount = 0;
        wavesCompleted = 0;
        survivalTime = 0;
        difficultyMod = 0;

        InitializeStats();
        StartCoroutines();
        UpdateAllUI();
    }

    private void OnDestroy()
    {
        // Отписываемся от всех событий при уничтожении
        OnHealthChanged = null;
        OnLevelUp = null;
        OnStatsUpdated = null;
        OnDeath = null;
    }
}

// Расширенное перечисление типов статов
//public enum StatType
//{
//    BaseHealth,
//    HealthMod,
//    HealthRegen,
//    BaseAtk,
//    AtkMod,
//    DamageMod,
//    Luck,
//    CritRate,
//    CritDamage,
//    Def,
//    PenetrationBoost,
//    ProjectileSpeed,
//    Durations,
//    CdRed,
//    AddProjectile,
//    AreaMod,
//    DefShred,
//    ExpGain,
//    GoldGain,
//    DropChance
//}