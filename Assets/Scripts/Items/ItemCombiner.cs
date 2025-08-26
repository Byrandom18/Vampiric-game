using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ItemCombiner : MonoBehaviour
{
    //[SerializeField] private float inheritanceChance = 0.7f;

    public EquipmentItem CombineItems(EquipmentItem item1, EquipmentItem item2)
    {
        // Проверяем, есть ли рецепт для этих предметов
        var recipe = FindCombinationRecipe(item1, item2);
        if (recipe == null)
        {
            Debug.LogWarning("No valid combination recipe for these items");
            return null;
        }

        // Создаём новый предмет на основе рецепта
        var newItem = Instantiate(recipe.result);
        newItem.rarity = (EquipmentItem.Rarity)((int)item1.rarity + 1);

        // Наследуем характеристики
        InheritStats(item1, item2, newItem);

        return newItem;
    }

    private EquipmentItem.CombinationRecipe FindCombinationRecipe(EquipmentItem item1, EquipmentItem item2)
    {
        // Проверяем все рецепты у первого предмета
        foreach (var recipe in item1.combinationRecipes)
        {
            if ((recipe.item1 == item1 && recipe.item2 == item2) ||
                (recipe.item1 == item2 && recipe.item2 == item1))
            {
                return recipe;
            }
        }
        return null;
    }

    private void InheritStats(EquipmentItem parent1, EquipmentItem parent2, EquipmentItem result)
    {
        var allStats = new List<EquipmentItem.SecondaryStat>();
        allStats.AddRange(parent1.secondaryStats);
        allStats.AddRange(parent2.secondaryStats);

        // Перемешиваем и фильтруем дубликаты
        var uniqueStats = allStats
            .GroupBy(s => s.type)
            .Select(g => new EquipmentItem.SecondaryStat
            {
                type = g.Key,
                value = g.Average(s => s.value) * GetRarityMultiplier(result.rarity)
            })
            .OrderBy(x => Random.value)
            .ToList();

        // Ограничиваем по максимальному количеству
        int maxStats = GetMaxSecondaryStats(result.rarity);
        result.secondaryStats = uniqueStats.Take(maxStats).ToList();
    }

    private float GetRarityMultiplier(EquipmentItem.Rarity rarity)
    {
        return rarity switch
        {
            EquipmentItem.Rarity.Rare => 1.1f,
            EquipmentItem.Rarity.Epic => 1.25f,
            EquipmentItem.Rarity.Legendary => 1.5f,
            _ => 1f
        };
    }

    private int GetMaxSecondaryStats(EquipmentItem.Rarity rarity)
    {
        return rarity switch
        {
            EquipmentItem.Rarity.Rare => 2,
            EquipmentItem.Rarity.Epic => 3,
            EquipmentItem.Rarity.Legendary => 4,
            _ => 0
        };
    }
}
