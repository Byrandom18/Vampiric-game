using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance;

    [Header("Base Stats")]
    public float baseDamage = 10f;
    public float baseSpeed = 5f;
    public float baseHealth = 100f;
    public float baseCritChance = 5f;

    [Header("Current Stats")]
    public float currentDamage;
    public float currentSpeed;
    public float currentHealth;
    public float currentCritChance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        UpdateStats();
    }

    public void UpgradeStat(StatType statType, float value)
    {
        switch (statType)
        {
            case StatType.Damage: baseDamage += value; break;
            case StatType.Speed: baseSpeed += value; break;
            case StatType.Health: baseHealth += value; break;
            case StatType.Crit: baseCritChance += value; break;
        }

        UpdateStats();
    }

    private void UpdateStats()
    {
        currentDamage = baseDamage;
        currentSpeed = baseSpeed;
        currentHealth = baseHealth;
        currentCritChance = baseCritChance;
    }
}