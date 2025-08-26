using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public Slider healthBar;

    [Header ("Боевые характеристики")]
    public float health = 100;
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

    private void Start()
    {
        Instance = this;
        atk = baseAtk * (1 + atkMod / 100);
        health = maxHealth;
        BarsUpdate();
        StartCoroutine(HealthRegen());
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

    

    private void LevelUp()
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
        if (exp > maxExp)
            LevelUp();
    }
}
