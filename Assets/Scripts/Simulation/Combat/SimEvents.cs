using Unity.Mathematics;

namespace Vampiric.Combat
{
    public struct DamagePopupEvent
    {
        public float2 Position;
        public float Amount;
        public byte IsCrit;
    }

    public struct PlayerDamageEvent
    {
        public float Amount;
    }

    public struct PickupCollectEvent
    {
        public byte PickupType;
        public float Value;
        public int ArtifactDefinitionId;
        public uint ArtifactSeed;
    }

    public struct DeathEvent
    {
        public float2 Position;
        public float Exp;
        public byte HasExplode;
        public float ExplodeRadius;
        public float ExplodeDamage;
        public byte HasSplit;
        public int SplitPrefabId;
        public int SplitCount;
        public int CompanionPrefabId;
        public int EntityIndex;
        public int EntityVersion;
    }

    public struct SpawnProjectileRequest
    {
        public float2 Position;
        public float2 Direction;
        public float Speed;
        public float Lifetime;
        public float Radius;
        public float Scale;
        public DamageRequest Damage;
        public FactionId Faction;
        public int Pierce;
        public byte Homing;
        public float HomingTurn;
        public float HomingRange;
        public byte Bounce;
        public int BounceCount;
        public float BounceRadius;
        public int CompanionPrefabId;
    }
}
