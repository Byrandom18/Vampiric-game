using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public Slider healthBar;

    [Header ("Боевые характеристики")]
    public float health = 100;
    public float maxHealth;
    public float healthRegen;
    public float atk = 1;
    public float damageMod;
    public float damageRed;
    public float luck;
    public float critRate;
    public float critDamage;
    public float def;
    public float speedMod;
    public float attackSpeed;
    public float penetrationBoost;
    public float projectileSpeed;
    public float durations;
    public bool invulnerability = false;


    [Header("Вспомогательные характеристики")]
    public float maxExp;
    public float exp;
    public int lvl;
    public float gold;
    public int gems;

    

    private void Start()
    {
        Instance = this;
        health = maxHealth;
        BarsUpdate();
    }


    public void TakeDamage(float damage)
    {
        if (!invulnerability)
        {
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
}
