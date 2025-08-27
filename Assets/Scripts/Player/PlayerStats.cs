using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public Slider healthBar;

    [Header ("Боевые характеристики")]
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

    [Header("Настройки сложности и прогрессии")]
    public float difficultyChange = 1; //как сильно растет
    public float difficultyMod = 1f; //множитель х-к
    public float difficultyDelay = 30f; //время до повышения

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        atk = baseAtk * (1 + atkMod / 100);
        UpdateStats();
        health = maxHealth;
        BarsUpdate();
        StartCoroutine(HealthRegen());
        StartCoroutine(DifficultyChange());
    }

    private IEnumerator DifficultyChange()
    {
        yield return new WaitForSeconds(difficultyDelay);
        difficultyMod += difficultyChange;
        StartCoroutine(DifficultyChange());
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
            case StatType.CritRate: critRate += value; break;
            case StatType.CritDamage: critDamage += value; break;
            case StatType.Def: def += value; break;
            case StatType.PenetrationBoost: penetrationBoost += Mathf.RoundToInt(value); break;
            case StatType.ProjectileSpeed: projectileSpeed += value; break;
            case StatType.Durations: durations += value; break;
            case StatType.CdRed: cdRed += value; break;
            case StatType.AddProjectile: addProjectile += Mathf.RoundToInt(value); break;
            case StatType.AreaMod: areaMod += value; break;
            case StatType.DefShred: defShred += value; break;
        }

        UpdateStats();
    }

    private void UpdateStats()
    {
        atk = baseAtk * (1 + atkMod / 100);
        maxHealth = baseHealth * (1 + healthMod / 100);
        BarsUpdate();
    }

    private IEnumerator HealthRegen()
    {
        if (health < maxHealth)
        {
            health += healthRegen;
            if (health > maxHealth)
                health = maxHealth;
            BarsUpdate();
        }
        yield return new WaitForSeconds(1);
        StartCoroutine(HealthRegen());
    }

    public void TakeDamage(float damage)
    {
        if (!invulnerability)
        {
            damage -= def;
            if (damage < 1)
                damage = 1;

            health -= damage;
            Debug.Log(health);
            BarsUpdate();
        }
        
    }

    public void BarsUpdate()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = health;
    }

    public void AddExp(float value)
    {
        exp += value;
        Debug.Log("exp = " + exp);
        if (exp >= maxExp)
            LevelUp();
    }
    public void AddGold(float value)
    {
        gold += value;
        Debug.Log("gold = " + gold);
        
    }
    public void AddHealth()
    {
        health += maxHealth / 10;
        if (health > maxHealth)
            health = maxHealth;
        BarsUpdate();
    }

    

    public void LevelUp()
    {
        lvl++;
        exp -= maxExp;
        if (lvl % 10 == 0)
            expIncrease += 2;
        maxExp += expIncrease;

        // Вызываем систему выбора карточек
        if (CardSelectionSystem.Instance != null)
        {
            CardSelectionSystem.Instance.ShowCardSelection();
        }
        
    }
}
