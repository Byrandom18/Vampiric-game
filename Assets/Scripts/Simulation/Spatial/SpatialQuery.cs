using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Vampiric.Simulation
{
    public static class SpatialQuery
    {
        public static bool TryFindNearestEnemy(float2 origin, float maxDistance, out Entity entity, out float2 position)
        {
            return TryFindEnemy(origin, maxDistance, TargetMode.Nearest, out entity, out position);
        }

        public static bool TryFindEnemy(float2 origin, float maxDistance, TargetMode mode, out Entity entity, out float2 position)
        {
            entity = Entity.Null;
            position = origin;
            if (!SimulationBridge.IsReady)
            {
                return false;
            }

            var map = SimulationBridge.EnemyHash;
            float cellSize = SimulationBridge.HashCellSize;
            int cells = math.max(1, (int)math.ceil(maxDistance / cellSize));
            int2 center = SpatialHash.CellCoord(origin, cellSize);
            float maxSq = maxDistance * maxDistance;

            Entity best = Entity.Null;
            float2 bestPos = origin;
            float bestMetric = mode == TargetMode.Strongest ? -1f : float.MaxValue;
            int foundCount = 0;
            Entity randomPick = Entity.Null;
            float2 randomPos = origin;
            uint hash = (uint)(origin.x * 1000f + origin.y * 97f + 13f);

            for (int y = -cells; y <= cells; y++)
            {
                for (int x = -cells; x <= cells; x++)
                {
                    int key = SpatialHash.CellKey(center.x + x, center.y + y);
                    if (!map.TryGetFirstValue(key, out var entry, out var iterator))
                    {
                        continue;
                    }

                    do
                    {
                        float2 pos = entry.Position.ToFloat2();
                        float distSq = math.lengthsq(pos - origin);
                        if (distSq > maxSq)
                        {
                            continue;
                        }

                        foundCount++;
                        hash = hash * 1664525u + (uint)entry.Entity.Index;
                        if ((hash & 255) < 40 || foundCount == 1)
                        {
                            randomPick = entry.Entity;
                            randomPos = pos;
                        }

                        float metric = mode switch
                        {
                            TargetMode.Nearest => distSq,
                            TargetMode.Weakest => entry.Health,
                            TargetMode.Strongest => -entry.Health,
                            _ => distSq
                        };

                        if (mode == TargetMode.Random)
                        {
                            continue;
                        }

                        if (metric < bestMetric)
                        {
                            bestMetric = metric;
                            best = entry.Entity;
                            bestPos = pos;
                        }
                    }
                    while (map.TryGetNextValue(out entry, ref iterator));
                }
            }

            if (mode == TargetMode.Random)
            {
                if (randomPick == Entity.Null)
                {
                    return false;
                }

                entity = randomPick;
                position = randomPos;
                return true;
            }

            if (best == Entity.Null)
            {
                return false;
            }

            entity = best;
            position = bestPos;
            return true;
        }

        public static void DamageEnemiesInRadius(float2 origin, float radius, float damage, DynamicBuffer<HitRecord> ignore)
        {
        }
    }

    public enum TargetMode : byte
    {
        Nearest = 0,
        Random = 1,
        Mouse = 2,
        Weakest = 3,
        Strongest = 4
    }
}
