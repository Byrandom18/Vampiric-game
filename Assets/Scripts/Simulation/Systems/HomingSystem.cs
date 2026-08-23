using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Vampiric.Combat;

namespace Vampiric.Simulation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SpatialHashSystem))]
    public partial struct HomingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SimulationBridge.IsReady)
            {
                return;
            }

            float dt = SystemAPI.Time.DeltaTime;
            new SteerJob
            {
                EnemyHash = SimulationBridge.EnemyHash,
                CellSize = SimulationBridge.HashCellSize,
                DeltaTime = dt
            }.Schedule();
        }

        [BurstCompile]
        public partial struct SteerJob : IJobEntity
        {
            [ReadOnly] public NativeParallelMultiHashMap<int, EntityRef> EnemyHash;
            public float CellSize;
            public float DeltaTime;

            private void Execute(ref Velocity2D velocity, in LocalTransform transform, in HomingData homing, in SimFaction faction)
            {
                if (faction.Value != FactionId.Player)
                {
                    return;
                }

                float2 pos = transform.Position.xy;
                float bestDistSq = homing.DetectionRange * homing.DetectionRange;
                float2 bestPos = pos;
                bool found = false;

                int2 center = SpatialHash.CellCoord(pos, CellSize);
                int cells = 2;
                for (int y = -cells; y <= cells; y++)
                {
                    for (int x = -cells; x <= cells; x++)
                    {
                        int key = SpatialHash.CellKey(center.x + x, center.y + y);
                        if (!EnemyHash.TryGetFirstValue(key, out var entry, out var iterator))
                        {
                            continue;
                        }

                        do
                        {
                            float2 enemyPos = entry.Position.ToFloat2();
                            float distSq = math.lengthsq(enemyPos - pos);
                            if (distSq < bestDistSq)
                            {
                                bestDistSq = distSq;
                                bestPos = enemyPos;
                                found = true;
                            }
                        }
                        while (EnemyHash.TryGetNextValue(out entry, ref iterator));
                    }
                }

                if (!found)
                {
                    return;
                }

                float speed = math.length(velocity.Value);
                if (speed < 0.01f)
                {
                    speed = 6f;
                }

                float2 desired = math.normalizesafe(bestPos - pos, new float2(1f, 0f));
                float2 current = math.normalizesafe(velocity.Value, desired);
                float2 steered = math.normalizesafe(math.lerp(current, desired, math.saturate(homing.TurnRate * DeltaTime)), desired);
                velocity.Value = steered * speed;
            }
        }
    }
}
