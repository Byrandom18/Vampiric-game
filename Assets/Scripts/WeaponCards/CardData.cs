using UnityEngine;

public enum WeaponType { Cone, Spread, ArmorBreak, Minigun, Homing, Bouncing }
public enum StatType { BaseHealth,
HealthMod,
HealthRegen,
BaseAtk,
AtkMod,
DamageMod,
Luck,
CritRate,
CritDamage,
Def,
PenetrationBoost,
ProjectileSpeed,
Durations,
CdRed,
AddProjectile,
AreaMod,
DefShred }

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;
    [TextArea] public string description;
    public Sprite icon;
    public int maxLevel = 1;

    [Header("Weapon Unlock/Upgrade")]
    public WeaponType weaponType;
    public bool isWeaponUnlock = false;

    [Header("Multipliers")]
    [Range(0.1f, 2f)] public float damageMultiplier = 1f;
    [Range(0.1f, 2f)] public float speedMultiplier = 1f;
    [Range(0.1f, 2f)] public float lifetimeMultiplier = 1f;
    [Range(0.1f, 2f)] public float sizeMultiplier = 1f;
    [Range(0.1f, 2f)] public float intervalMultiplier = 1f;

    [Header("Additive Bonuses")]
    public int penetrateAdd = 0;
    public int countAdd = 0;
    public float defShredAdd = 0f;

    [Header("Player Stat Upgrade")]
    public StatType statType;
    public float statValue = 0f;

    [Header("Requirements")]
    public int requiredLevel = 1; // ƒŒ¡¿¬‹“≈ ›“” —“–Œ ”
    public CardData[] requiredCards;

    public string GetDescription(int currentLevel)
    {
        string desc = description;

        if (isWeaponUnlock)
            return "Unlock " + weaponType.ToString() + " Weapon";

        if (damageMultiplier != 1f)
            desc = desc.Replace("{dmg}", ((damageMultiplier - 1f) * 100f).ToString("+0;-#") + "%");

        if (speedMultiplier != 1f)
            desc = desc.Replace("{spd}", ((speedMultiplier - 1f) * 100f).ToString("+0;-#") + "%");

        if (lifetimeMultiplier != 1f)
            desc = desc.Replace("{dur}", ((lifetimeMultiplier - 1f) * 100f).ToString("+0;-#") + "%");

        if (sizeMultiplier != 1f)
            desc = desc.Replace("{size}", ((sizeMultiplier - 1f) * 100f).ToString("+0;-#") + "%");

        if (intervalMultiplier != 1f)
            desc = desc.Replace("{cd}", ((intervalMultiplier - 1f) * 100f).ToString("+0;-#") + "%");

        if (penetrateAdd != 0)
            desc = desc.Replace("{pen}", penetrateAdd.ToString("+0;-#"));

        if (countAdd != 0)
            desc = desc.Replace("{count}", countAdd.ToString("+0;-#"));

        if (defShredAdd != 0f)
            desc = desc.Replace("{shred}", defShredAdd.ToString("+0;-#"));

        if (statValue != 0f)
            desc = desc.Replace("{stat}", statValue.ToString("+0;-#"));

        return desc;
    }
}