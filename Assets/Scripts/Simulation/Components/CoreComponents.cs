using Unity.Entities;
using Unity.Mathematics;
using Vampiric.Combat;

namespace Vampiric.Simulation
{
    public struct SimFaction : IComponentData
    {
        public FactionId Value;
    }

    public struct Health : IComponentData
    {
        public float Current;
        public float Max;
    }

    public struct Defense : IComponentData
    {
        public float Value;
    }

    public struct Velocity2D : IComponentData
    {
        public float2 Value;
    }

    public struct ColliderRadius : IComponentData
    {
        public float Value;
    }

    public struct Lifetime : IComponentData
    {
        public float Remaining;
    }

    public struct LootValue : IComponentData
    {
        public float Exp;
    }

    public struct CompanionLink : IComponentData
    {
        public int PrefabId;
    }

    public struct PendingDestroy : IComponentData
    {
    }

    public struct PlayerTag : IComponentData
    {
    }

    public struct PlayerRuntimeStats : IComponentData
    {
        public float2 Position;
        public float Attack;
        public float DamageMod;
        public float CritRate;
        public float CritDamage;
        public float Defense;
        public float DefenseShred;
        public float AreaMod;
        public float ProjectileSpeed;
        public float Durations;
        public float CooldownReduction;
        public int ExtraProjectiles;
        public float RangeMod;
        public float ExplosiveMod;
        public float AreaDamageMod;
        public float MeleeMod;
        public float PickupRadius;
        public byte Invulnerable;
        public byte MagnetActive;
    }
}
