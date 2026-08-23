using Unity.Mathematics;

namespace Vampiric.Combat
{
    public static class DamageMath
    {
        public static float CategoryMultiplier(in PlayerRuntimeStatsProxy stats, WeaponCategory category)
        {
            return category switch
            {
                WeaponCategory.Ranged => 1f + stats.RangeMod * 0.01f,
                WeaponCategory.Explosive => 1f + stats.ExplosiveMod * 0.01f,
                WeaponCategory.Area => 1f + stats.AreaDamageMod * 0.01f,
                WeaponCategory.Melee => 1f + stats.MeleeMod * 0.01f,
                _ => 1f
            };
        }

        public static float Resolve(
            float amount,
            WeaponCategory category,
            float defenseShred,
            bool canCrit,
            float targetDefense,
            in PlayerRuntimeStatsProxy attacker,
            out bool isCrit)
        {
            isCrit = false;
            float damage = amount * CategoryMultiplier(in attacker, category);
            float defense = math.max(0f, targetDefense - defenseShred);
            damage = math.max(1f, damage - defense);

            if (canCrit && attacker.CritRate > 0f)
            {
                float roll = attacker.CritSample;
                if (roll * 100f <= attacker.CritRate)
                {
                    damage *= 1f + attacker.CritDamage * 0.01f;
                    isCrit = true;
                }
            }

            return damage;
        }
    }

    public struct PlayerRuntimeStatsProxy
    {
        public float RangeMod;
        public float ExplosiveMod;
        public float AreaDamageMod;
        public float MeleeMod;
        public float CritRate;
        public float CritDamage;
        public float CritSample;
    }
}
