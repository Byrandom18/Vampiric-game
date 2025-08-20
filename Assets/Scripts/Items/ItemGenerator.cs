using System.Linq;
using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    public EquipmentItem GenerateItemWithRandomStats(EquipmentItem baseItem, EquipmentItem.Rarity rarity)
    {
        var newItem = Instantiate(baseItem);
        newItem.rarity = rarity;
        GenerateRandomSecondaryStats(newItem);
        return newItem;
    }

    private void GenerateRandomSecondaryStats(EquipmentItem item)
    {
        int statsCount = GetRandomStatsCount(item.rarity);

        var availableStats = System.Enum.GetValues(typeof(EquipmentItem.StatType))
            .Cast<EquipmentItem.StatType>()
            .ToList();

        for (int i = 0; i < statsCount; i++)
        {
            if (availableStats.Count == 0) break;

            int randomIndex = Random.Range(0, availableStats.Count);
            var statType = availableStats[randomIndex];
            availableStats.RemoveAt(randomIndex);

            float value = CalculateStatValue(statType, item.rarity);
            item.secondaryStats.Add(new EquipmentItem.SecondaryStat
            {
                type = statType,
                value = value
            });
        }
    }

    // Добавляем недостающий метод
    private int GetRandomStatsCount(EquipmentItem.Rarity rarity)
    {
        // Логика определения количества случайных характеристик
        // в зависимости от редкости предмета
        switch (rarity)
        {
            case EquipmentItem.Rarity.Common:
                return 0; // Обычные предметы без доп. характеристик
            case EquipmentItem.Rarity.Rare:
                return Random.Range(1, 3); // 1-2 характеристики
            case EquipmentItem.Rarity.Epic:
                return Random.Range(2, 4); // 2-3 характеристики
            case EquipmentItem.Rarity.Legendary:
                return Random.Range(3, 5); // 3-4 характеристики
            default:
                return 0;
        }
    }

    private float CalculateStatValue(EquipmentItem.StatType type, EquipmentItem.Rarity rarity)
    {
        // Базовая величина характеристики
        float baseValue = type switch
        {
            EquipmentItem.StatType.CritChance => 10,
            EquipmentItem.StatType.CritDamage => 20,
            EquipmentItem.StatType.AtkMod => 16,
            EquipmentItem.StatType.HealthMod => 16,
            EquipmentItem.StatType.Def => 2,
            EquipmentItem.StatType.AtkFlat => 6,
            EquipmentItem.StatType.HealthFlat => 30,
            EquipmentItem.StatType.HealthRegen => 2,
            EquipmentItem.StatType.ExpMod => 10,
            EquipmentItem.StatType.GoldMod => 10,
            EquipmentItem.StatType.ProjectileSpeed => 10,
            EquipmentItem.StatType.Durations => 10,
            EquipmentItem.StatType.CdRed => 5,
            EquipmentItem.StatType.Penetration => 2,
            EquipmentItem.StatType.Luck => 10,
            EquipmentItem.StatType.DamageMod => 5,
            EquipmentItem.StatType.MeleeMod => 10,
            EquipmentItem.StatType.RangeMod => 10,
            EquipmentItem.StatType.DemonMod => 10,
            EquipmentItem.StatType.ExplosiveMod => 10,
            EquipmentItem.StatType.AreaMod =>10,
            _ => 1 //с этой темой он игнорит фаталы, которые будут если не добавить стат тайп в файл эквипмент айтем
        };

        // Множитель редкости
        float rarityMultiplier = rarity switch
        {
            EquipmentItem.Rarity.Rare => 0.5f,
            EquipmentItem.Rarity.Epic => 0.75f,
            EquipmentItem.Rarity.Legendary => 1f,
            _ => 0.5f 
        };

        return baseValue * rarityMultiplier;
    }
}
