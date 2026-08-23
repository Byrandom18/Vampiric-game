using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using Vampiric.Game;
using Vampiric.Meta;
using Vampiric.Stats;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    public static event System.Action<Transform> OnPlayerSpawned;
    public static event System.Action OnPlayerDeath;
    public StatSheet Sheet { get; private set; } = new StatSheet();

    [Header("UI Elements")]
    public Slider healthBar;
    public Text levelText;
    public Text goldText;
    public Text gemsText;
    public ExpBar expBar;

    [Header("˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜˜˜˜˜")]
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

    [Header("˜˜˜˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜˜˜˜˜")]
    public float maxExp = 5;
    public float exp;
    public float expIncrease = 10;
    public int lvl = 1;
    public float gold;
    public int gems;
    public int killCount;
    public int wavesCompleted;

    [Header("˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜ ˜ ˜˜˜˜˜˜˜˜˜˜")]
    public float difficultyChange = 1;
    public float difficultyMod = 0f;
    public float difficultyDelay = 30f;
    public float survivalTime = 0f;

    [Header("˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜ ˜˜˜˜˜")]
    public float expGainMultiplier = 1f;
    public float goldGainMultiplier = 1f;
    public float itemDropChanceMultiplier = 1f;

    // ˜˜˜˜˜˜˜ ˜˜˜ UI ˜ ˜˜˜˜˜˜ ˜˜˜˜˜˜
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
        PushBasesToSheet();
        RefreshDerivedStats();
        health = maxHealth;
    }

    private void PushBasesToSheet()
    {
        Sheet.SetBase(StatId.BaseHealth, baseHealth);
        Sheet.SetBase(StatId.HealthMod, healthMod);
        Sheet.SetBase(StatId.HealthRegen, healthRegen);
        Sheet.SetBase(StatId.BaseAttack, baseAtk);
        Sheet.SetBase(StatId.AttackMod, atkMod);
        Sheet.SetBase(StatId.DamageMod, damageMod);
        Sheet.SetBase(StatId.Luck, luck);
        Sheet.SetBase(StatId.CritRate, critRate);
        Sheet.SetBase(StatId.CritDamage, critDamage);
        Sheet.SetBase(StatId.Defense, def);
        Sheet.SetBase(StatId.Penetration, penetrationBoost);
        Sheet.SetBase(StatId.ProjectileSpeed, projectileSpeed);
        Sheet.SetBase(StatId.Durations, durations);
        Sheet.SetBase(StatId.CooldownReduction, cdRed);
        Sheet.SetBase(StatId.ExtraProjectiles, addProjectile);
        Sheet.SetBase(StatId.AreaMod, areaMod);
        Sheet.SetBase(StatId.DefenseShred, defShred);
        Sheet.SetBase(StatId.ExpMod, (expGainMultiplier - 1f) * 100f);
        Sheet.SetBase(StatId.GoldMod, (goldGainMultiplier - 1f) * 100f);
    }

    public void RefreshDerivedStats()
    {
        Sheet.Recalculate();
        atk = Sheet.ToRuntime(default, invulnerability, false).Attack;
        healthMod = Sheet.Get(StatId.HealthMod);
        damageMod = Sheet.Get(StatId.DamageMod);
        critRate = Sheet.Get(StatId.CritRate);
        critDamage = Sheet.Get(StatId.CritDamage);
        def = Sheet.Get(StatId.Defense);
        healthRegen = Sheet.Get(StatId.HealthRegen);
        cdRed = Sheet.Get(StatId.CooldownReduction);
        addProjectile = Mathf.RoundToInt(Sheet.Get(StatId.ExtraProjectiles));
        areaMod = Sheet.Get(StatId.AreaMod);
        defShred = Sheet.Get(StatId.DefenseShred);
        projectileSpeed = Sheet.Get(StatId.ProjectileSpeed);
        durations = Sheet.Get(StatId.Durations);
        penetrationBoost = Mathf.RoundToInt(Sheet.Get(StatId.Penetration));
        expGainMultiplier = 1f + Sheet.Get(StatId.ExpMod) / 100f;
        goldGainMultiplier = 1f + Sheet.Get(StatId.GoldMod) / 100f;
        maxHealth = (baseHealth + Sheet.Get(StatId.HealthFlat)) * (1f + healthMod / 100f);
        health = Mathf.Min(health, maxHealth);
        UpdateAllUI();
        OnStatsUpdated?.Invoke();
    }

    private void LateUpdate()
    {
        SimulationDriver.Instance?.SyncPlayerFromStats(this, Sheet);
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
            Debug.Log($"˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜: {difficultyMod}");
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
        StatId id = statType switch
        {
            StatType.BaseHealth => StatId.BaseHealth,
            StatType.HealthMod => StatId.HealthMod,
            StatType.HealthRegen => StatId.HealthRegen,
            StatType.BaseAtk => StatId.BaseAttack,
            StatType.AtkMod => StatId.AttackMod,
            StatType.DamageMod => StatId.DamageMod,
            StatType.Luck => StatId.Luck,
            StatType.CritRate => StatId.CritRate,
            StatType.CritDamage => StatId.CritDamage,
            StatType.Def => StatId.Defense,
            StatType.PenetrationBoost => StatId.Penetration,
            StatType.ProjectileSpeed => StatId.ProjectileSpeed,
            StatType.Durations => StatId.Durations,
            StatType.CdRed => StatId.CooldownReduction,
            StatType.AddProjectile => StatId.ExtraProjectiles,
            StatType.AreaMod => StatId.AreaMod,
            StatType.DefShred => StatId.DefenseShred,
            _ => StatId.None
        };

        if (id != StatId.None)
        {
            if (id == StatId.BaseHealth)
            {
                baseHealth += value;
                Sheet.SetBase(StatId.BaseHealth, baseHealth);
            }
            else if (id == StatId.BaseAttack)
            {
                baseAtk += value;
                Sheet.SetBase(StatId.BaseAttack, baseAtk);
            }
            else
            {
                Sheet.AddModifier(StatSheet.CardSource, new StatModifier(id, value));
            }
        }

        RefreshDerivedStats();
    }

    private void UpdateStats()
    {
        RefreshDerivedStats();
    }

    public void TakeDamage(float damage)
    {
        if (isDead || invulnerability) return;

        damage -= def;
        damage = Mathf.Max(1f, damage * (1 - def / (def + 100f))); // ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜ ˜˜˜˜˜˜

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

        // ˜˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜ ˜˜˜˜˜˜˜˜
        if (healthRegenCoroutine != null) StopCoroutine(healthRegenCoroutine);
        if (difficultyCoroutine != null) StopCoroutine(difficultyCoroutine);
        if (survivalTimerCoroutine != null) StopCoroutine(survivalTimerCoroutine);

        // ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜ ˜˜˜˜˜˜
        OnDeath?.Invoke();
        OnPlayerDeath?.Invoke();
        ProfileState.Current?.CommitFinishedRun(gold, gems);
        if (Application.CanStreamedLevelBeLoaded("Hub"))
        {
            SceneManager.LoadScene("Hub");
        }
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

        // ˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜ ˜˜˜˜
        if (lvl % 10 == 0)
            expIncrease += 2;
        maxExp += expIncrease;

        // ˜˜˜˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜ ˜˜˜ ˜˜˜˜˜˜
        AddHealth(maxHealth * 0.1f);

        UpdateAllUI();
        OnLevelUp?.Invoke();

        // ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜
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

    // ˜˜˜˜˜ ˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜˜ (˜˜˜ ˜˜˜˜˜ ˜˜˜˜)
    public void ResetStats()
    {
        Sheet = new StatSheet();
        isDead = false;

        // ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜˜˜˜˜
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

        // ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜
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
        // ˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜ ˜˜˜˜ ˜˜˜˜˜˜˜ ˜˜˜ ˜˜˜˜˜˜˜˜˜˜˜
        OnHealthChanged = null;
        OnLevelUp = null;
        OnStatsUpdated = null;
        OnDeath = null;
    }
}

// ˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜˜˜˜˜
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
