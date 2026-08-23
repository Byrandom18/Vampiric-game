using Unity.Mathematics;
using UnityEngine;
using Vampiric.Combat;
using Vampiric.Game;
using Vampiric.Presentation;
using Vampiric.Simulation;
using Vampiric.Stats;

namespace Vampiric.Weapons
{
    public sealed class WeaponFireDirector : MonoBehaviour
    {
        public static WeaponFireDirector Instance { get; private set; }

        [SerializeField] private float _aimRange = 28f;

        private WeaponLoadout _loadout;
        private readonly System.Collections.Generic.Dictionary<WeaponId, int> _prefabIds = new();

        public WeaponLoadout Loadout => _loadout ??= new WeaponLoadout();

        private void Awake()
        {
            Instance = this;
            _loadout ??= new WeaponLoadout();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void RegisterDefinition(WeaponDefinition definition)
        {
            Loadout.Register(definition);
            if (definition != null && definition.ProjectilePrefab != null && PresentationRegistry.Instance != null)
            {
                _prefabIds[definition.Id] = PresentationRegistry.Instance.RegisterPrefab(definition.ProjectilePrefab);
            }
        }

        private void Update()
        {
            if (SimulationDriver.Instance == null || PlayerStats.Instance == null)
            {
                return;
            }

            float dt = Time.deltaTime;
            var stats = PlayerStats.Instance;
            var sheet = stats.Sheet;
            float2 origin = new float2(transform.position.x, transform.position.y);

            foreach (var slot in Loadout.ActiveSlots)
            {
                slot.CooldownLeft -= dt;
                if (slot.CooldownLeft > 0f)
                {
                    continue;
                }

                Fire(slot, origin, sheet, stats);
            }
        }

        private void Fire(WeaponLoadout.Slot slot, float2 origin, StatSheet sheet, PlayerStats stats)
        {
            var def = slot.Definition;
            if (def == null)
            {
                return;
            }

            float interval = def.BaseInterval * slot.IntervalMul * (1f - sheet.Get(StatId.CooldownReduction) / 100f);
            interval = Mathf.Max(0.05f, interval);
            if (def.Pattern == FirePattern.Bounce)
            {
                int count = def.BaseCount + slot.CountAdd + sheet.GetInt(StatId.ExtraProjectiles);
                interval /= 1f + count / 2f;
            }

            slot.CooldownLeft = interval;

            float2 direction = ResolveAim(def.Aim, origin);
            int projectileCount = Mathf.Max(1, def.BaseCount + slot.CountAdd + (int)sheet.Get(StatId.ExtraProjectiles));
            float damage = def.BaseDamage * slot.DamageMul * stats.atk * (1f + sheet.Get(StatId.DamageMod) / 100f);
            if (def.Pattern == FirePattern.ArmorBreak)
            {
                damage *= 1f + (projectileCount - 1) / 2f;
                projectileCount = 1;
            }

            float speed = def.BaseSpeed * slot.SpeedMul * (1f + sheet.Get(StatId.ProjectileSpeed) / 100f);
            float lifetime = def.BaseLifetime * slot.LifetimeMul * (1f + sheet.Get(StatId.Durations) / 100f);
            float size = def.BaseSize * slot.SizeMul * (1f + sheet.Get(StatId.AreaMod) / 100f);
            int pierce = def.BasePierce + slot.PierceAdd + (int)sheet.Get(StatId.Penetration);
            float shred = def.BaseDefShred + slot.ShredAdd + sheet.Get(StatId.DefenseShred);
            int prefabId = _prefabIds.TryGetValue(def.Id, out int id) ? id : 0;

            var factory = SimulationDriver.Instance.Factory;
            var request = new SpawnProjectileRequest
            {
                Position = origin,
                Speed = speed,
                Lifetime = lifetime,
                Radius = 0.15f * size,
                Scale = size,
                Damage = DamageRequest.Create(damage)
                    .WithSource(factory.PlayerEntity)
                    .WithCategory(def.Category)
                    .WithShred(shred)
                    .WithCrit(true)
                    .Build(),
                Faction = FactionId.Player,
                Pierce = Mathf.Max(1, pierce),
                CompanionPrefabId = prefabId
            };

            switch (def.Pattern)
            {
                case FirePattern.CircleSpread:
                case FirePattern.HomingSpread:
                    SpawnRing(factory, def, request, projectileCount, def.Pattern == FirePattern.HomingSpread || def.Id == WeaponId.HomingFan);
                    break;
                case FirePattern.Minigun:
                    SpawnMinigun(factory, def, request, direction, projectileCount);
                    break;
                case FirePattern.Bounce:
                    request.Direction = direction;
                    request.Bounce = 1;
                    request.BounceCount = Mathf.Max(1, pierce);
                    request.BounceRadius = def.BounceRadius;
                    factory.SpawnProjectile(request);
                    break;
                case FirePattern.Grenade:
                    SpawnGrenades(factory, request, direction, projectileCount);
                    break;
                default:
                    SpawnCone(factory, def, request, direction, projectileCount);
                    break;
            }

            if (def.Id == WeaponId.ExplosiveMinigun)
            {
                SpawnMinigun(factory, def, request, direction, projectileCount);
            }
        }

        private void SpawnCone(SimEntityFactory factory, WeaponDefinition def, SpawnProjectileRequest request, float2 main, int count)
        {
            if (def.Id == WeaponId.RicochetCone)
            {
                request.Bounce = 1;
                request.BounceCount = Mathf.Max(2, request.Pierce);
                request.BounceRadius = def.BounceRadius;
            }

            if (count <= 1)
            {
                request.Direction = main;
                factory.SpawnProjectile(request);
                return;
            }

            float start = -def.ConeAngle * 0.5f;
            float step = def.ConeAngle / (count - 1);
            for (int i = 0; i < count; i++)
            {
                request.Direction = Rotate(main, start + step * i);
                factory.SpawnProjectile(request);
            }
        }

        private void SpawnRing(SimEntityFactory factory, WeaponDefinition def, SpawnProjectileRequest request, int count, bool homing)
        {
            float step = 360f / count;
            for (int i = 0; i < count; i++)
            {
                float angle = i * step + UnityEngine.Random.Range(-def.SpreadAngle, def.SpreadAngle);
                request.Direction = Angle(angle);
                if (homing)
                {
                    request.Homing = 1;
                    request.HomingTurn = def.HomingTurn;
                    request.HomingRange = def.HomingRange;
                }

                factory.SpawnProjectile(request);
            }
        }

        private void SpawnMinigun(SimEntityFactory factory, WeaponDefinition def, SpawnProjectileRequest request, float2 main, int count)
        {
            if (def.Id == WeaponId.ExplosiveMinigun)
            {
                request.Damage = DamageRequest.Create(request.Damage.Amount)
                    .WithSource(request.Damage.Source)
                    .WithCategory(WeaponCategory.Explosive)
                    .WithShred(request.Damage.DefenseShred)
                    .WithCrit(true)
                    .Build();
            }

            for (int i = 0; i < count; i++)
            {
                float deviation = UnityEngine.Random.Range(-def.ConeAngle * 0.5f, def.ConeAngle * 0.5f);
                request.Direction = Rotate(main, deviation);
                factory.SpawnProjectile(request);
            }
        }

        private void SpawnGrenades(SimEntityFactory factory, SpawnProjectileRequest request, float2 main, int count)
        {
            request.Damage = DamageRequest.Create(request.Damage.Amount)
                .WithSource(request.Damage.Source)
                .WithCategory(WeaponCategory.Explosive)
                .WithShred(request.Damage.DefenseShred)
                .WithCrit(true)
                .Build();
            request.Pierce = 1;
            for (int i = 0; i < count; i++)
            {
                request.Direction = Rotate(main, (i - (count - 1) * 0.5f) * 12f);
                factory.SpawnProjectile(request);
            }
        }

        private float2 ResolveAim(AimMode mode, float2 origin)
        {
            if (mode == AimMode.Mouse && Camera.main != null)
            {
                Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                return math.normalizesafe(new float2(mouse.x, mouse.y) - origin, new float2(1f, 0f));
            }

            var targetMode = mode switch
            {
                AimMode.Random => TargetMode.Random,
                AimMode.Weakest => TargetMode.Weakest,
                AimMode.Strongest => TargetMode.Strongest,
                _ => TargetMode.Nearest
            };

            if (SpatialQuery.TryFindEnemy(origin, _aimRange, targetMode, out _, out var position))
            {
                return math.normalizesafe(position - origin, new float2(1f, 0f));
            }

            return new float2(1f, 0f);
        }

        private static float2 Rotate(float2 vector, float degrees)
        {
            float rad = math.radians(degrees);
            float s = math.sin(rad);
            float c = math.cos(rad);
            return new float2(vector.x * c - vector.y * s, vector.x * s + vector.y * c);
        }

        private static float2 Angle(float degrees)
        {
            float rad = math.radians(degrees);
            return new float2(math.cos(rad), math.sin(rad));
        }
    }

    public static class StatSheetExtensions
    {
        public static int GetInt(this StatSheet sheet, StatId id)
        {
            return Mathf.RoundToInt(sheet.Get(id));
        }
    }
}
