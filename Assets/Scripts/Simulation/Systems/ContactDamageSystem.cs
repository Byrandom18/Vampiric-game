using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Vampiric.Combat;

namespace Vampiric.Simulation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EnemyMoveSystem))]
    public partial struct ContactDamageSystem : ISystem
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

            if (player.Invulnerable != 0)
            {
                return;
            }

            float dt = SystemAPI.Time.DeltaTime;
            foreach (var (transform, contact) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<ContactDamage>>()
                         .WithAll<EnemyTag>()
                         .WithNone<PendingDestroy>())
            {
                contact.ValueRW.CooldownLeft -= dt;
                if (contact.ValueRW.CooldownLeft > 0f)
                {
                    continue;
                }

                float reach = contact.ValueRO.HitRadius;
                if (math.lengthsq(transform.ValueRO.Position.xy - player.Position) <= reach * reach)
                {
                    SimulationBridge.PlayerDamage.Enqueue(new PlayerDamageEvent { Amount = contact.ValueRO.Amount });
                    contact.ValueRW.CooldownLeft = contact.ValueRO.Cooldown;
                }
            }
        }
    }
}
