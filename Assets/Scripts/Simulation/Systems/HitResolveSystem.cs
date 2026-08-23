using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Vampiric.Combat;

namespace Vampiric.Simulation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ProjectileMoveSystem))]
    public partial struct HitResolveSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SimulationBridge.IsReady || !SystemAPI.TryGetSingleton<PlayerRuntimeStats>(out var playerStats))
            {
                return;
            }

            var random = Unity.Mathematics.Random.CreateFromIndex(
                (uint)math.max(1, (int)(SystemAPI.Time.ElapsedTime * 1000.000f) + 1));

            var attacker = new PlayerRuntimeStatsProxy
            {
                RangeMod = playerStats.RangeMod,
                ExplosiveMod = playerStats.ExplosiveMod,
                AreaDamageMod = playerStats.AreaDamageMod,
                MeleeMod = playerStats.MeleeMod,
                CritRate = playerStats.CritRate,
                CritDamage = playerStats.CritDamage,
                CritSample = 1f
            };

            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            float cellSize = SimulationBridge.HashCellSize;
            var enemyHash = SimulationBridge.EnemyHash;

            foreach (var (transform, radius, faction, payload, pierce, entity) in SystemAPI
                         .Query<RefRO<LocalTransform>, RefRO<ColliderRadius>, RefRO<SimFaction>, RefRO<DamagePayload>, RefRW<Pierce>>()
                         .WithAll<ProjectileTag>()
                         .WithNone<PendingDestroy>()
                         .WithEntityAccess())
            {
                if (faction.ValueRO.Value != FactionId.Player)
                {
                    TryHitPlayer(ref state, transform.ValueRO.Position.xy, radius.ValueRO.Value, payload.ValueRO, playerStats, ref ecb, entity);
                    continue;
                }

                bool hasHits = SystemAPI.HasBuffer<HitRecord>(entity);
                DynamicBuffer<HitRecord> hits = default;
                if (hasHits)
                {
                    hits = SystemAPI.GetBuffer<HitRecord>(entity);
                }

                float2 pos = transform.ValueRO.Position.xy;
                int2 center = SpatialHash.CellCoord(pos, cellSize);
                bool destroyed = false;

                for (int y = -1; y <= 1 && !destroyed; y++)
                {
                    for (int x = -1; x <= 1 && !destroyed; x++)
                    {
                        int key = SpatialHash.CellKey(center.x + x, center.y + y);
                        if (!enemyHash.TryGetFirstValue(key, out var entry, out var iterator))
                        {
                            continue;
                        }

                        do
                        {
                            if (AlreadyHit(hasHits, hits, entry.Entity))
                            {
                                continue;
                            }

                            float2 enemyPos = entry.Position.ToFloat2();
                            float reach = radius.ValueRO.Value + entry.Radius;
                            if (math.lengthsq(enemyPos - pos) > reach * reach)
                            {
                                continue;
                            }

                            if (!SystemAPI.HasComponent<Health>(entry.Entity) ||
                                SystemAPI.HasComponent<PendingDestroy>(entry.Entity))
                            {
                                continue;
                            }

                            attacker.CritSample = random.NextFloat();
                            float defense = SystemAPI.HasComponent<Defense>(entry.Entity)
                                ? SystemAPI.GetComponent<Defense>(entry.Entity).Value
                                : 0f;

                            float damage = DamageMath.Resolve(
                                payload.ValueRO.Amount,
                                payload.ValueRO.Category,
                                payload.ValueRO.DefenseShred,
                                payload.ValueRO.CanCrit != 0,
                                defense,
                                in attacker,
                                out bool isCrit);

                            var health = SystemAPI.GetComponent<Health>(entry.Entity);
                            health.Current -= damage;
                            SystemAPI.SetComponent(entry.Entity, health);

                            if (SystemAPI.HasComponent<Defense>(entry.Entity) && payload.ValueRO.DefenseShred > 0f)
                            {
                                var def = SystemAPI.GetComponent<Defense>(entry.Entity);
                                def.Value = math.max(0f, def.Value - payload.ValueRO.DefenseShred);
                                SystemAPI.SetComponent(entry.Entity, def);
                            }

                            SimulationBridge.DamagePopups.Enqueue(new DamagePopupEvent
                            {
                                Position = enemyPos + new float2(0f, 0.5f),
                                Amount = damage,
                                IsCrit = isCrit ? (byte)1 : (byte)0
                            });

                            if (hasHits)
                            {
                                hits.Add(new HitRecord { Target = entry.Entity });
                            }

                            if (health.Current <= 0f)
                            {
                                QueueDeath(ref state, ref ecb, entry.Entity, enemyPos);
                            }

                            if (SystemAPI.HasComponent<BounceData>(entity))
                            {
                                var bounce = SystemAPI.GetComponent<BounceData>(entity);
                                bounce.Remaining -= 1;
                                SystemAPI.SetComponent(entity, bounce);
                                if (bounce.Remaining <= 0)
                                {
                                    ecb.AddComponent<PendingDestroy>(entity);
                                    destroyed = true;
                                    break;
                                }

                                if (!TryRetargetBounce(pos, bounce.SearchRadius, hasHits, hits, enemyHash, cellSize, out float2 nextDir))
                                {
                                    ecb.AddComponent<PendingDestroy>(entity);
                                    destroyed = true;
                                    break;
                                }

                                SystemAPI.SetComponent(entity, new Velocity2D
                                {
                                    Value = nextDir * math.length(SystemAPI.GetComponent<Velocity2D>(entity).Value)
                                });
                            }
                            else
                            {
                                pierce.ValueRW.Remaining -= 1;
                                if (pierce.ValueRW.Remaining <= 0)
                                {
                                    ecb.AddComponent<PendingDestroy>(entity);
                                    destroyed = true;
                                    break;
                                }
                            }
                        }
                        while (enemyHash.TryGetNextValue(out entry, ref iterator));
                    }
                }
            }

            ecb.Playback(state.EntityManager);
        }

        private static bool AlreadyHit(bool hasHits, DynamicBuffer<HitRecord> hits, Entity target)
        {
            if (!hasHits)
            {
                return false;
            }

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].Target == target)
                {
                    return true;
                }
            }

            return false;
        }

        private void TryHitPlayer(
            ref SystemState state,
            float2 pos,
            float radius,
            in DamagePayload payload,
            in PlayerRuntimeStats player,
            ref EntityCommandBuffer ecb,
            Entity projectile)
        {
            if (player.Invulnerable != 0)
            {
                return;
            }

            float reach = radius + 0.4f;
            if (math.lengthsq(pos - player.Position) > reach * reach)
            {
                return;
            }

            SimulationBridge.PlayerDamage.Enqueue(new PlayerDamageEvent { Amount = payload.Amount });
            if (!SystemAPI.HasComponent<PendingDestroy>(projectile))
            {
                ecb.AddComponent<PendingDestroy>(projectile);
            }
        }

        private static bool TryRetargetBounce(
            float2 pos,
            float searchRadius,
            bool hasHits,
            DynamicBuffer<HitRecord> hits,
            NativeParallelMultiHashMap<int, EntityRef> enemyHash,
            float cellSize,
            out float2 direction)
        {
            direction = new float2(1f, 0f);
            float searchSq = searchRadius * searchRadius;
            int2 center = SpatialHash.CellCoord(pos, cellSize);
            int cells = (int)math.ceil(searchRadius / cellSize);
            bool found = false;
            float2 foundPos = pos;

            for (int y = -cells; y <= cells; y++)
            {
                for (int x = -cells; x <= cells; x++)
                {
                    int key = SpatialHash.CellKey(center.x + x, center.y + y);
                    if (!enemyHash.TryGetFirstValue(key, out var entry, out var iterator))
                    {
                        continue;
                    }

                    do
                    {
                        if (AlreadyHit(hasHits, hits, entry.Entity))
                        {
                            continue;
                        }

                        float2 enemyPos = entry.Position.ToFloat2();
                        float distSq = math.lengthsq(enemyPos - pos);
                        if (distSq <= searchSq)
                        {
                            foundPos = enemyPos;
                            found = true;
                            break;
                        }
                    }
                    while (enemyHash.TryGetNextValue(out entry, ref iterator));
                }
            }

            if (!found)
            {
                return false;
            }

            direction = math.normalizesafe(foundPos - pos, new float2(1f, 0f));
            return true;
        }

        private void QueueDeath(ref SystemState state, ref EntityCommandBuffer ecb, Entity entity, float2 position)
        {
            if (SystemAPI.HasComponent<PendingDestroy>(entity))
            {
                return;
            }

            float exp = SystemAPI.HasComponent<LootValue>(entity)
                ? SystemAPI.GetComponent<LootValue>(entity).Exp
                : 1f;
            var death = new DeathEvent
            {
                Position = position,
                Exp = exp,
                EntityIndex = entity.Index,
                EntityVersion = entity.Version
            };

            if (SystemAPI.HasComponent<ExplodeOnDeath>(entity))
            {
                var explode = SystemAPI.GetComponent<ExplodeOnDeath>(entity);
                death.HasExplode = 1;
                death.ExplodeRadius = explode.Radius;
                death.ExplodeDamage = explode.Damage;
            }

            if (SystemAPI.HasComponent<SplitOnDeath>(entity))
            {
                var split = SystemAPI.GetComponent<SplitOnDeath>(entity);
                death.HasSplit = 1;
                death.SplitPrefabId = split.PrefabId;
                death.SplitCount = split.Count;
            }

            if (SystemAPI.HasComponent<CompanionLink>(entity))
            {
                death.CompanionPrefabId = SystemAPI.GetComponent<CompanionLink>(entity).PrefabId;
            }

            SimulationBridge.Deaths.Enqueue(death);
            ecb.AddComponent<PendingDestroy>(entity);
        }
    }
}
