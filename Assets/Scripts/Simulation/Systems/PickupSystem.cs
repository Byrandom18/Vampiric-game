using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Vampiric.Combat;

namespace Vampiric.Simulation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SpatialHashSystem))]
    public partial struct PickupSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SimulationBridge.IsReady || !SystemAPI.TryGetSingleton<PlayerRuntimeStats>(out var player))
            {
                return;
            }

            float dt = SystemAPI.Time.DeltaTime;
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            var consumed = new NativeHashSet<Entity>(64, Allocator.Temp);

            foreach (var (transform, velocity, pickup, radius, entity) in SystemAPI
                         .Query<RefRW<LocalTransform>, RefRW<Velocity2D>, RefRW<PickupData>, RefRO<ColliderRadius>>()
                         .WithAll<PickupTag>()
                         .WithNone<PendingDestroy>()
                         .WithEntityAccess())
            {
                if (consumed.Contains(entity))
                {
                    continue;
                }

                if (pickup.ValueRO.WaitLeft > 0f)
                {
                    pickup.ValueRW.WaitLeft -= dt;
                    if (pickup.ValueRW.WaitLeft <= 0f)
                    {
                        pickup.ValueRW.CanCollect = 1;
                    }
                }

                float2 pos = transform.ValueRO.Position.xy;
                float distSq = math.lengthsq(pos - player.Position);
                float attract = player.MagnetActive != 0
                    ? pickup.ValueRO.AttractRadius * 6f
                    : pickup.ValueRO.AttractRadius + player.PickupRadius;

                if (pickup.ValueRO.CanCollect != 0 && distSq <= attract * attract)
                {
                    pickup.ValueRW.IsAttracted = 1;
                }

                if (pickup.ValueRO.IsAttracted != 0)
                {
                    float2 dir = math.normalizesafe(player.Position - pos, float2.zero);
                    float2 next = pos + dir * pickup.ValueRO.Speed * dt;
                    transform.ValueRW.Position = new float3(next.x, next.y, 0f);
                    velocity.ValueRW.Value = dir * pickup.ValueRO.Speed;
                    pos = next;
                    distSq = math.lengthsq(pos - player.Position);
                }

                if (pickup.ValueRO.CanCollect != 0 && distSq <= 0.35f * 0.35f)
                {
                    SimulationBridge.Pickups.Enqueue(new PickupCollectEvent
                    {
                        PickupType = (byte)pickup.ValueRO.Type,
                        Value = pickup.ValueRO.Value,
                        ArtifactDefinitionId = pickup.ValueRO.ArtifactDefinitionId,
                        ArtifactSeed = pickup.ValueRO.ArtifactSeed
                    });
                    ecb.AddComponent<PendingDestroy>(entity);
                    consumed.Add(entity);
                    continue;
                }

                if (pickup.ValueRO.Type != PickupType.Exp || pickup.ValueRO.CanCollect == 0 || pickup.ValueRO.IsAttracted != 0)
                {
                    continue;
                }

                TryMergeExp(ref state, ref ecb, entity, pos, pickup.ValueRO.Value, radius.ValueRO.Value, consumed);
            }

            consumed.Dispose();
            ecb.Playback(state.EntityManager);
        }

        private void TryMergeExp(
            ref SystemState state,
            ref EntityCommandBuffer ecb,
            Entity self,
            float2 pos,
            float value,
            float radius,
            NativeHashSet<Entity> consumed)
        {
            var pickupHash = SimulationBridge.PickupHash;
            float cellSize = SimulationBridge.HashCellSize;
            int2 center = SpatialHash.CellCoord(pos, cellSize);
            const float mergeRadius = 1.5f;

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    int key = SpatialHash.CellKey(center.x + x, center.y + y);
                    if (!pickupHash.TryGetFirstValue(key, out var entry, out var iterator))
                    {
                        continue;
                    }

                    do
                    {
                        if (entry.Entity == self || consumed.Contains(entry.Entity))
                        {
                            continue;
                        }

                        if (!SystemAPI.HasComponent<PickupData>(entry.Entity) ||
                            SystemAPI.HasComponent<PendingDestroy>(entry.Entity))
                        {
                            continue;
                        }

                        var other = SystemAPI.GetComponent<PickupData>(entry.Entity);
                        if (other.Type != PickupType.Exp || other.CanCollect == 0)
                        {
                            continue;
                        }

                        float2 otherPos = entry.Position.ToFloat2();
                        if (math.lengthsq(otherPos - pos) > mergeRadius * mergeRadius)
                        {
                            continue;
                        }

                        if (value >= other.Value)
                        {
                            var selfData = SystemAPI.GetComponent<PickupData>(self);
                            selfData.Value += other.Value;
                            SystemAPI.SetComponent(self, selfData);
                            ecb.AddComponent<PendingDestroy>(entry.Entity);
                            consumed.Add(entry.Entity);
                        }

                        return;
                    }
                    while (pickupHash.TryGetNextValue(out entry, ref iterator));
                }
            }
        }
    }
}
