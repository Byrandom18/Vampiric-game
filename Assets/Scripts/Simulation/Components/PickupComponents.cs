using Unity.Entities;

namespace Vampiric.Simulation
{
    public enum PickupType : byte
    {
        Exp = 0,
        Gold = 1,
        Magnet = 2,
        Heart = 3,
        Bomb = 4,
        Gem = 5,
        Equipment = 6
    }

    public struct PickupTag : IComponentData
    {
    }

    public struct PickupData : IComponentData
    {
        public PickupType Type;
        public float Value;
        public float AttractRadius;
        public float Speed;
        public float WaitLeft;
        public byte IsAttracted;
        public byte CanCollect;
        public int ArtifactDefinitionId;
        public uint ArtifactSeed;
    }
}
