using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Vampiric.Combat;

namespace Vampiric.Simulation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct EnemyRangedSystem : ISystem
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
            var random = Unity.Mathematics.Random.CreateFromIndex(
                (uint)math.max(1, (int)(SystemAPI.Time.ElapsedTime * 97f) + 3));

            foreach (var (transform, attack, contact, entity) in SystemAPI
                         .Query<RefRO<LocalTransform>, RefRW<EnemyRangedAttack>, RefRO<ContactDamage>>()
                         .WithAll<EnemyTag>()
                         .WithNone<PendingDestroy>()
                         .WithEntityAccess())
            {
                attack.ValueRW.CooldownLeft -= dt;
                if (attack.ValueRW.CooldownLeft > 0f)
                {
                    continue;
                }

                attack.ValueRW.CooldownLeft = attack.ValueRO.Interval;
                float2 origin = transform.ValueRO.Position.xy;
                float damage = contact.ValueRO.Amount * attack.ValueRO.DamageMultiplier;

                if (attack.ValueRO.IsMine != 0)
                {
                    int count = math.max(1, attack.ValueRO.MineCount);
                    for (int i = 0; i < count; i++)
                    {
                        float angle = random.NextFloat(0f, math.PI * 2f);
                        EnqueueShot(origin, new float2(math.cos(angle), math.sin(angle)), attack.ValueRO, damage);
                    }
                }
                else
                {
                    float2 dir = math.normalizesafe(player.Position - origin, new float2(1f, 0f));
                    EnqueueShot(origin, dir, attack.ValueRO, damage);
                }
            }
        }

        private static void EnqueueShot(float2 origin, float2 direction, in EnemyRangedAttack attack, float damage)
        {
            SimulationBridge.EnemyProjectiles.Enqueue(new SpawnProjectileRequest
            {
                Position = origin,
                Direction = direction,
                Speed = attack.ProjectileSpeed,
                Lifetime = attack.ProjectileLifetime,
                Radius = attack.ProjectileRadius,
                Scale = 1f,
                Damage = DamageRequest.Create(damage).WithCategory(WeaponCategory.Ranged).WithCrit(false).Build(),
                Faction = FactionId.Enemy,
                Pierce = 1,
                CompanionPrefabId = attack.ProjectilePrefabId
            });
        }
    }
}
