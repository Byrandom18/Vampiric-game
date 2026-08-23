using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Vampiric.Combat;
using Vampiric.Presentation;
using Vampiric.Simulation;

namespace Vampiric.Game
{
    public sealed class SimEntityFactory
    {
        private readonly World _world;
        private readonly SimArchetypes _archetypes;
        private Entity _playerEntity;

        public Entity PlayerEntity => _playerEntity;
        public EntityManager EntityManager => _world.EntityManager;

        public SimEntityFactory(World world)
        {
            _world = world;
            _archetypes = SimArchetypes.Create(world.EntityManager);
        }

        public Entity CreatePlayer(float2 position)
        {
            var em = EntityManager;
            _playerEntity = em.CreateEntity(_archetypes.Player);
            em.SetComponentData(_playerEntity, LocalTransform.FromPosition(new float3(position.x, position.y, 0f)));
            em.SetComponentData(_playerEntity, new ColliderRadius { Value = 0.4f });
            em.SetComponentData(_playerEntity, new SimFaction { Value = FactionId.Player });
            em.SetComponentData(_playerEntity, new Health { Current = 100f, Max = 100f });
            em.SetComponentData(_playerEntity, new PlayerRuntimeStats { Position = position });
            return _playerEntity;
        }

        public void SyncPlayer(float2 position, PlayerRuntimeStats stats, float health, float maxHealth)
        {
            if (_playerEntity == Entity.Null || !EntityManager.Exists(_playerEntity))
            {
                return;
            }

            stats.Position = position;
            EntityManager.SetComponentData(_playerEntity, stats);
            EntityManager.SetComponentData(_playerEntity, LocalTransform.FromPosition(new float3(position.x, position.y, 0f)));
            EntityManager.SetComponentData(_playerEntity, new Health { Current = health, Max = maxHealth });
        }

        public Entity SpawnEnemy(int prefabId, float2 position, EnemySpawnInfo info)
        {
            var em = EntityManager;
            var entity = em.CreateEntity(_archetypes.Enemy);
            em.SetComponentData(entity, LocalTransform.FromPositionRotationScale(
                new float3(position.x, position.y, 0f),
                quaternion.identity,
                info.Scale));
            em.SetComponentData(entity, new Velocity2D { Value = float2.zero });
            em.SetComponentData(entity, new ColliderRadius { Value = info.Radius });
            em.SetComponentData(entity, new SimFaction { Value = FactionId.Enemy });
            em.SetComponentData(entity, new Health { Current = info.Health, Max = info.Health });
            em.SetComponentData(entity, new Defense { Value = info.Defense });
            em.SetComponentData(entity, new LootValue { Exp = info.Exp });
            em.SetComponentData(entity, new EnemyMoveData
            {
                Speed = info.Speed,
                TeleportDistance = info.TeleportDistance,
                EnableDash = info.EnableDash,
                DashTriggerDistance = info.DashDistance,
                DashMultiplier = info.DashPower,
                DashDuration = info.DashDuration,
                DashCooldown = info.DashCooldown
            });
            em.SetComponentData(entity, new ContactDamage
            {
                Amount = info.ContactDamage,
                Cooldown = 0.45f,
                HitRadius = 0.4f
            });
            em.SetComponentData(entity, new CompanionLink { PrefabId = prefabId });

            if (info.HasRanged)
            {
                em.AddComponentData(entity, new EnemyRangedAttack
                {
                    Interval = info.RangedInterval,
                    CooldownLeft = info.RangedInterval,
                    ProjectileSpeed = info.RangedSpeed,
                    ProjectileLifetime = info.RangedLifetime,
                    DamageMultiplier = info.RangedDamageMul,
                    ProjectileRadius = 0.2f,
                    ProjectilePrefabId = info.RangedPrefabId,
                    IsMine = info.IsMine,
                    MineCount = info.MineCount
                });
            }

            if (info.HasExplode)
            {
                em.AddComponentData(entity, new ExplodeOnDeath
                {
                    Radius = info.ExplodeRadius,
                    Damage = info.ExplodeDamage
                });
            }

            if (info.HasSplit)
            {
                em.AddComponentData(entity, new SplitOnDeath
                {
                    PrefabId = info.SplitPrefabId,
                    Count = info.SplitCount
                });
            }

            PresentationRegistry.Instance?.Bind(entity, prefabId, new Vector3(position.x, position.y, 0f));
            return entity;
        }

        public Entity SpawnProjectile(in SpawnProjectileRequest request)
        {
            var em = EntityManager;
            var entity = em.CreateEntity(_archetypes.Projectile);
            float2 dir = math.normalizesafe(request.Direction, new float2(1f, 0f));
            float angle = math.atan2(dir.y, dir.x);
            em.SetComponentData(entity, LocalTransform.FromPositionRotationScale(
                new float3(request.Position.x, request.Position.y, 0f),
                quaternion.RotateZ(angle),
                math.max(0.1f, request.Scale)));
            em.SetComponentData(entity, new Velocity2D { Value = dir * request.Speed });
            em.SetComponentData(entity, new ColliderRadius { Value = math.max(0.08f, request.Radius) });
            em.SetComponentData(entity, new SimFaction { Value = request.Faction });
            em.SetComponentData(entity, new DamagePayload
            {
                Source = request.Damage.Source,
                Amount = request.Damage.Amount,
                Category = request.Damage.Category,
                DefenseShred = request.Damage.DefenseShred,
                CanCrit = request.Damage.CanCrit ? (byte)1 : (byte)0
            });
            em.SetComponentData(entity, new Pierce { Remaining = math.max(1, request.Pierce) });
            em.SetComponentData(entity, new Lifetime { Remaining = request.Lifetime });
            em.SetComponentData(entity, new CompanionLink { PrefabId = request.CompanionPrefabId });
            em.GetBuffer<HitRecord>(entity);

            if (request.Homing != 0)
            {
                em.AddComponentData(entity, new HomingData
                {
                    TurnRate = request.HomingTurn,
                    DetectionRange = request.HomingRange
                });
            }

            if (request.Bounce != 0)
            {
                em.AddComponentData(entity, new BounceData
                {
                    Remaining = math.max(1, request.BounceCount),
                    SearchRadius = request.BounceRadius
                });
            }

            if (request.CompanionPrefabId != 0)
            {
                PresentationRegistry.Instance?.Bind(
                    entity,
                    request.CompanionPrefabId,
                    new Vector3(request.Position.x, request.Position.y, 0f));
            }

            return entity;
        }

        public Entity SpawnPickup(int prefabId, float2 position, PickupData data, float lifetime = 90f)
        {
            var em = EntityManager;
            var entity = em.CreateEntity(_archetypes.Pickup);
            em.SetComponentData(entity, LocalTransform.FromPosition(new float3(position.x, position.y, 0f)));
            em.SetComponentData(entity, new Velocity2D());
            em.SetComponentData(entity, new ColliderRadius { Value = 0.25f });
            em.SetComponentData(entity, data);
            em.SetComponentData(entity, new Lifetime { Remaining = lifetime });
            em.SetComponentData(entity, new CompanionLink { PrefabId = prefabId });
            PresentationRegistry.Instance?.Bind(entity, prefabId, new Vector3(position.x, position.y, 0f));
            return entity;
        }

        public void DestroyPending()
        {
            var em = EntityManager;
            var query = em.CreateEntityQuery(ComponentType.ReadOnly<PendingDestroy>());
            using var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                PresentationRegistry.Instance?.Unbind(entities[i]);
                em.DestroyEntity(entities[i]);
            }
        }

        public void ApplyExplosion(float2 origin, float radius, float damage)
        {
            var em = EntityManager;
            var query = em.CreateEntityQuery(
                ComponentType.ReadOnly<EnemyTag>(),
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadWrite<Health>());
            using var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
            float radiusSq = radius * radius;
            for (int i = 0; i < entities.Length; i++)
            {
                var transform = em.GetComponentData<LocalTransform>(entities[i]);
                if (math.lengthsq(transform.Position.xy - origin) > radiusSq)
                {
                    continue;
                }

                var health = em.GetComponentData<Health>(entities[i]);
                health.Current -= damage;
                em.SetComponentData(entities[i], health);
                SimulationBridge.DamagePopups.Enqueue(new DamagePopupEvent
                {
                    Position = transform.Position.xy,
                    Amount = damage
                });

                if (health.Current <= 0f && !em.HasComponent<PendingDestroy>(entities[i]))
                {
                    em.AddComponent<PendingDestroy>(entities[i]);
                    SimulationBridge.Deaths.Enqueue(new DeathEvent
                    {
                        Position = transform.Position.xy,
                        Exp = em.HasComponent<LootValue>(entities[i])
                            ? em.GetComponentData<LootValue>(entities[i]).Exp
                            : 1f,
                        CompanionPrefabId = em.HasComponent<CompanionLink>(entities[i])
                            ? em.GetComponentData<CompanionLink>(entities[i]).PrefabId
                            : 0,
                        EntityIndex = entities[i].Index,
                        EntityVersion = entities[i].Version
                    });
                }
            }
        }
    }

    public struct EnemySpawnInfo
    {
        public float Health;
        public float Defense;
        public float ContactDamage;
        public float Speed;
        public float Exp;
        public float Radius;
        public float Scale;
        public float TeleportDistance;
        public byte EnableDash;
        public float DashDistance;
        public float DashPower;
        public float DashDuration;
        public float DashCooldown;
        public bool HasRanged;
        public float RangedInterval;
        public float RangedSpeed;
        public float RangedLifetime;
        public float RangedDamageMul;
        public int RangedPrefabId;
        public byte IsMine;
        public int MineCount;
        public bool HasExplode;
        public float ExplodeRadius;
        public float ExplodeDamage;
        public bool HasSplit;
        public int SplitPrefabId;
        public int SplitCount;
    }
}
