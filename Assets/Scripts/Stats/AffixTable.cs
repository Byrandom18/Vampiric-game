using System.Collections.Generic;
using Vampiric.Items;
using Vampiric.Stats;

namespace Vampiric.Stats
{
    public static class AffixTable
    {
        public static readonly StatId[] Rollable =
        {
            StatId.CritRate,
            StatId.CritDamage,
            StatId.AttackMod,
            StatId.HealthMod,
            StatId.Defense,
            StatId.AttackFlat,
            StatId.HealthFlat,
            StatId.HealthRegen,
            StatId.ExpMod,
            StatId.GoldMod,
            StatId.ProjectileSpeed,
            StatId.Durations,
            StatId.CooldownReduction,
            StatId.Penetration,
            StatId.Luck,
            StatId.DamageMod,
            StatId.MeleeMod,
            StatId.RangeMod,
            StatId.ExplosiveMod,
            StatId.AreaMod
        };

        public static float BaseValue(StatId id)
        {
            return id switch
            {
                StatId.CritRate => 10f,
                StatId.CritDamage => 20f,
                StatId.AttackMod => 16f,
                StatId.HealthMod => 16f,
                StatId.Defense => 2f,
                StatId.AttackFlat => 6f,
                StatId.HealthFlat => 30f,
                StatId.HealthRegen => 2f,
                StatId.ExpMod => 10f,
                StatId.GoldMod => 10f,
                StatId.ProjectileSpeed => 10f,
                StatId.Durations => 10f,
                StatId.CooldownReduction => 5f,
                StatId.Penetration => 1f,
                StatId.Luck => 10f,
                StatId.DamageMod => 5f,
                StatId.MeleeMod => 10f,
                StatId.RangeMod => 10f,
                StatId.ExplosiveMod => 10f,
                StatId.AreaMod => 10f,
                _ => 1f
            };
        }

        public static float RarityMultiplier(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Rare => 0.5f,
                ItemRarity.Epic => 0.75f,
                ItemRarity.Legendary => 1f,
                _ => 0.5f
            };
        }

        public static void GetAffixCountRange(ItemRarity rarity, out int min, out int max)
        {
            switch (rarity)
            {
                case ItemRarity.Rare:
                    min = 1;
                    max = 2;
                    break;
                case ItemRarity.Epic:
                    min = 2;
                    max = 3;
                    break;
                case ItemRarity.Legendary:
                    min = 3;
                    max = 4;
                    break;
                default:
                    min = 0;
                    max = 0;
                    break;
            }
        }

        public static int MaxLifespan(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Rare => 3,
                ItemRarity.Epic => 5,
                ItemRarity.Legendary => 8,
                _ => 3
            };
        }

        public static int RepairCost(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Rare => 50,
                ItemRarity.Epic => 150,
                ItemRarity.Legendary => 400,
                _ => 50
            };
        }

        public static IReadOnlyList<StatId> GetRollable()
        {
            return Rollable;
        }
    }
}
