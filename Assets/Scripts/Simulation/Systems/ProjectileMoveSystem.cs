using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Vampiric.Simulation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(HomingSystem))]
    public partial struct ProjectileMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            new MoveJob { DeltaTime = dt }.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct MoveJob : IJobEntity
        {
            public float DeltaTime;

            private void Execute(ref LocalTransform transform, in Velocity2D velocity, in ProjectileTag tag)
            {
                float2 pos = transform.Position.xy + velocity.Value * DeltaTime;
                transform.Position = new float3(pos.x, pos.y, 0f);
                if (math.lengthsq(velocity.Value) > 0.0001f)
                {
                    float angle = math.atan2(velocity.Value.y, velocity.Value.x);
                    transform.Rotation = quaternion.RotateZ(angle);
                }
            }
        }
    }
}
