using Unity.Entities;

namespace Vampiric.Simulation
{
    public struct EnemyTag : IComponentData
    {
    }

    public struct EnemyMoveData : IComponentData
    {
        public float Speed;
        public float TeleportDistance;
        public byte EnableDash;
        public float DashTriggerDistance;
        public float DashMultiplier;
        public float DashDuration;
        public float DashCooldown;
        public float DashCooldownLeft;
        public float DashTimeLeft;
        public byte IsDashing;
        public float Facing;
    }

    public struct ContactDamage : IComponentData
    {
        public float Amount;
        public float Cooldown;
        public float CooldownLeft;
        public float HitRadius;
    }

    public struct EnemyRangedAttack : IComponentData
    {
        public float Interval;
        public float CooldownLeft;
        public float ProjectileSpeed;
        public float ProjectileLifetime;
        public float DamageMultiplier;
        public float ProjectileRadius;
        public int ProjectilePrefabId;
        public byte IsMine;
        public int MineCount;
    }

    public struct ExplodeOnDeath : IComponentData
    {
        public float Radius;
        public float Damage;
    }

    public struct SplitOnDeath : IComponentData
    {
        public int PrefabId;
        public int Count;
    }
}
