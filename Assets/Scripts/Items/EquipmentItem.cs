using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EquipmentItem", menuName = "Scriptable Objects/EquipmentItem")]
public class EquipmentItem : ScriptableObject
{
    public enum Rarity { Common, Rare, Epic, Legendary }

    [Header("Base Info")]
    public string itemName;
    public Sprite icon;
    public Rarity rarity;

    [Header("Fixed Main Effect")]
    public string mainEffectName;
    public float mainEffectValue;

    [Header("Possible Combinations")]
    public List<CombinationRecipe> combinationRecipes;

    [Header("Generated Stats")]
    public List<SecondaryStat> secondaryStats = new List<SecondaryStat>();

    [System.Serializable]
    public class SecondaryStat
    {
        public StatType type;
        public float value;
    }

    [System.Serializable]
    public class CombinationRecipe
    {
        public EquipmentItem item1;
        public EquipmentItem item2;
        public EquipmentItem result;
    }

    public enum StatType
    {
        CritChance,
        CritDamage,
        AtkMod,
        HealthMod,
        Def,
        AtkFlat,
        HealthFlat,
        HealthRegen,
        ExpMod,
        GoldMod,
        ProjectileSpeed,
        Durations,
        CdRed,
        Penetration,
        Luck,
        DamageMod,
        MeleeMod,
        RangeMod,
        DemonMod,
        ExplosiveMod,
        AreaMod
    }
}
