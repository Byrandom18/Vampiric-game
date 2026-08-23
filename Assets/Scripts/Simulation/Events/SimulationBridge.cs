using Unity.Collections;
using Vampiric.Combat;

namespace Vampiric.Simulation
{
    public static class SimulationBridge
    {
        public static NativeQueue<DamagePopupEvent> DamagePopups;
        public static NativeQueue<PlayerDamageEvent> PlayerDamage;
        public static NativeQueue<PickupCollectEvent> Pickups;
        public static NativeQueue<DeathEvent> Deaths;
        public static NativeQueue<SpawnProjectileRequest> EnemyProjectiles;

        public static NativeParallelMultiHashMap<int, EntityRef> EnemyHash;
        public static NativeParallelMultiHashMap<int, EntityRef> PickupHash;
        public static float HashCellSize = SpatialHash.DefaultCellSize;
        public static bool IsReady;

        public static void Initialize(int enemyCapacity = 2048)
        {
            Dispose();
            DamagePopups = new NativeQueue<DamagePopupEvent>(Allocator.Persistent);
            PlayerDamage = new NativeQueue<PlayerDamageEvent>(Allocator.Persistent);
            Pickups = new NativeQueue<PickupCollectEvent>(Allocator.Persistent);
            Deaths = new NativeQueue<DeathEvent>(Allocator.Persistent);
            EnemyProjectiles = new NativeQueue<SpawnProjectileRequest>(Allocator.Persistent);
            EnemyHash = new NativeParallelMultiHashMap<int, EntityRef>(enemyCapacity, Allocator.Persistent);
            PickupHash = new NativeParallelMultiHashMap<int, EntityRef>(512, Allocator.Persistent);
            IsReady = true;
        }

        public static void Dispose()
        {
            IsReady = false;
            if (DamagePopups.IsCreated) DamagePopups.Dispose();
            if (PlayerDamage.IsCreated) PlayerDamage.Dispose();
            if (Pickups.IsCreated) Pickups.Dispose();
            if (Deaths.IsCreated) Deaths.Dispose();
            if (EnemyProjectiles.IsCreated) EnemyProjectiles.Dispose();
            if (EnemyHash.IsCreated) EnemyHash.Dispose();
            if (PickupHash.IsCreated) PickupHash.Dispose();
        }
    }

    public struct EntityRef
    {
        public Unity.Entities.Entity Entity;
        public float2Packed Position;
        public float Radius;
        public float Health;
    }

    public struct float2Packed
    {
        public float X;
        public float Y;

        public Unity.Mathematics.float2 ToFloat2()
        {
            return new Unity.Mathematics.float2(X, Y);
        }

        public static float2Packed From(Unity.Mathematics.float2 value)
        {
            return new float2Packed { X = value.x, Y = value.y };
        }
    }
}
