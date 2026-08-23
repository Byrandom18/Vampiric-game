using Unity.Entities;
using Vampiric.Combat;

namespace Vampiric.Simulation
{
    public struct ProjectileTag : IComponentData
    {
    }

    public struct DamagePayload : IComponentData
    {
        public Entity Source;
        public float Amount;
        public WeaponCategory Category;
        public float DefenseShred;
        public byte CanCrit;
    }

    public struct Pierce : IComponentData
    {
        public int Remaining;
    }

    public struct HomingData : IComponentData
    {
        public float TurnRate;
        public float DetectionRange;
    }

    public struct BounceData : IComponentData
    {
        public int Remaining;
        public float SearchRadius;
    }

    public struct HitRecord : IBufferElementData
    {
        public Entity Target;
    }
}
