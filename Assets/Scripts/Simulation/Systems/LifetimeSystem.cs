using Unity.Burst;
using Unity.Entities;

namespace Vampiric.Simulation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct LifetimeSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            foreach (var (lifetime, entity) in SystemAPI.Query<RefRW<Lifetime>>().WithEntityAccess())
            {
                lifetime.ValueRW.Remaining -= dt;
                if (lifetime.ValueRO.Remaining <= 0f && !SystemAPI.HasComponent<PendingDestroy>(entity))
                {
                    ecb.AddComponent<PendingDestroy>(entity);
                }
            }

            ecb.Playback(state.EntityManager);
        }
    }
}
