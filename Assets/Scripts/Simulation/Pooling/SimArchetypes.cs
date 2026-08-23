using Unity.Entities;
using Unity.Transforms;

namespace Vampiric.Simulation
{
    public struct SimArchetypes
    {
        public EntityArchetype Enemy;
        public EntityArchetype Projectile;
        public EntityArchetype Pickup;
        public EntityArchetype Player;

        public static SimArchetypes Create(EntityManager entityManager)
        {
            return new SimArchetypes
            {
                Enemy = entityManager.CreateArchetype(
                    typeof(LocalTransform),
                    typeof(Velocity2D),
                    typeof(ColliderRadius),
                    typeof(SimFaction),
                    typeof(Health),
                    typeof(Defense),
                    typeof(LootValue),
                    typeof(EnemyTag),
                    typeof(EnemyMoveData),
                    typeof(ContactDamage),
                    typeof(CompanionLink)),
                Projectile = entityManager.CreateArchetype(
                    typeof(LocalTransform),
                    typeof(Velocity2D),
                    typeof(ColliderRadius),
                    typeof(SimFaction),
                    typeof(ProjectileTag),
                    typeof(DamagePayload),
                    typeof(Pierce),
                    typeof(Lifetime),
                    typeof(CompanionLink),
                    typeof(HitRecord)),
                Pickup = entityManager.CreateArchetype(
                    typeof(LocalTransform),
                    typeof(Velocity2D),
                    typeof(ColliderRadius),
                    typeof(PickupTag),
                    typeof(PickupData),
                    typeof(Lifetime),
                    typeof(CompanionLink)),
                Player = entityManager.CreateArchetype(
                    typeof(LocalTransform),
                    typeof(ColliderRadius),
                    typeof(SimFaction),
                    typeof(PlayerTag),
                    typeof(PlayerRuntimeStats),
                    typeof(Health))
            };
        }
    }
}
