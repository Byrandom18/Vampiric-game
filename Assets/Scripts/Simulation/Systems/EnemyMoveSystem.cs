using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Vampiric.Simulation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct EnemyMoveSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<PlayerRuntimeStats>(out var player))
            {
                return;
            }

            float dt = SystemAPI.Time.DeltaTime;
            float2 playerPos = player.Position;
            new MoveJob
            {
                DeltaTime = dt,
                PlayerPosition = playerPos
            }.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct MoveJob : IJobEntity
        {
            public float DeltaTime;
            public float2 PlayerPosition;

            private void Execute(
                ref LocalTransform transform,
                ref Velocity2D velocity,
                ref EnemyMoveData move)
            {
                float2 pos = transform.Position.xy;
                float2 toPlayer = PlayerPosition - pos;
                float distanceSq = math.lengthsq(toPlayer);
                float distance = math.sqrt(distanceSq);

                if (distance > move.TeleportDistance && move.TeleportDistance > 0f)
                {
                    float2 dir = distance > 0.001f ? toPlayer / distance : new float2(1f, 0f);
                    pos = PlayerPosition - dir * (move.TeleportDistance * 0.9f);
                    transform.Position = new float3(pos.x, pos.y, 0f);
                    velocity.Value = float2.zero;
                    return;
                }

                float2 direction = distance > 0.001f ? toPlayer / distance : float2.zero;
                if (direction.x != 0f)
                {
                    move.Facing = math.sign(direction.x);
                }

                float speed = move.Speed;
                if (move.EnableDash != 0)
                {
                    if (move.IsDashing != 0)
                    {
                        move.DashTimeLeft -= DeltaTime;
                        speed *= move.DashMultiplier;
                        if (move.DashTimeLeft <= 0f)
                        {
                            move.IsDashing = 0;
                            move.DashCooldownLeft = move.DashCooldown;
                        }
                    }
                    else
                    {
                        move.DashCooldownLeft -= DeltaTime;
                        if (move.DashCooldownLeft <= 0f && distance < move.DashTriggerDistance)
                        {
                            move.IsDashing = 1;
                            move.DashTimeLeft = move.DashDuration;
                            speed *= move.DashMultiplier;
                        }
                    }
                }

                velocity.Value = direction * speed;
                pos += velocity.Value * DeltaTime;
                transform.Position = new float3(pos.x, pos.y, 0f);
                if (move.Facing != 0f)
                {
                    transform.Scale = math.abs(transform.Scale) * move.Facing;
                }
            }
        }
    }
}
