using Unity.Entities;
using Unity.Transforms;

namespace Vampiric.Simulation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(HitResolveSystem))]
    public partial struct SpatialHashSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SimulationBridge.IsReady)
            {
                return;
            }

            float cellSize = SimulationBridge.HashCellSize;
            var enemyHash = SimulationBridge.EnemyHash;
            var pickupHash = SimulationBridge.PickupHash;
            enemyHash.Clear();
            pickupHash.Clear();

            foreach (var (transform, radius, health, entity) in SystemAPI
                         .Query<RefRO<LocalTransform>, RefRO<ColliderRadius>, RefRO<Health>>()
                         .WithAll<EnemyTag>()
                         .WithNone<PendingDestroy>()
                         .WithEntityAccess())
            {
                enemyHash.Add(
                    SpatialHash.CellKey(transform.ValueRO.Position.xy, cellSize),
                    new EntityRef
                    {
                        Entity = entity,
                        Position = float2Packed.From(transform.ValueRO.Position.xy),
                        Radius = radius.ValueRO.Value,
                        Health = health.ValueRO.Current
                    });
            }

            foreach (var (transform, radius, pickup, entity) in SystemAPI
                         .Query<RefRO<LocalTransform>, RefRO<ColliderRadius>, RefRO<PickupData>>()
                         .WithAll<PickupTag>()
                         .WithNone<PendingDestroy>()
                         .WithEntityAccess())
            {
                pickupHash.Add(
                    SpatialHash.CellKey(transform.ValueRO.Position.xy, cellSize),
                    new EntityRef
                    {
                        Entity = entity,
                        Position = float2Packed.From(transform.ValueRO.Position.xy),
                        Radius = radius.ValueRO.Value,
                        Health = pickup.ValueRO.Value
                    });
            }

            SimulationBridge.EnemyHash = enemyHash;
            SimulationBridge.PickupHash = pickupHash;
        }
    }
}
