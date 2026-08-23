using System.Collections.Generic;
using UnityEngine;
using Vampiric.Presentation;

namespace Vampiric.Game
{
    public sealed class EnemyPrefabCache
    {
        private readonly Dictionary<GameObject, CachedEnemy> _cache = new();

        public CachedEnemy Get(GameObject prefab)
        {
            if (prefab == null)
            {
                return default;
            }

            if (_cache.TryGetValue(prefab, out var cached))
            {
                return cached;
            }

            cached = Read(prefab);
            _cache[prefab] = cached;
            return cached;
        }

        private CachedEnemy Read(GameObject prefab)
        {
            var info = new EnemySpawnInfo
            {
                Health = 10f,
                ContactDamage = 1f,
                Speed = 2f,
                Exp = 1f,
                Radius = 0.35f,
                Scale = 1f,
                TeleportDistance = 25f,
                DashPower = 2f,
                DashDuration = 1f,
                DashCooldown = 5f,
                DashDistance = 3f
            };

            var damage = prefab.GetComponent<EnemyDamage>();
            if (damage != null)
            {
                info.Health = damage.baseHealth;
                info.ContactDamage = damage.baseDamage;
                info.Defense = damage.baseDefence;
                info.Exp = damage.baseExpValue;
            }

            var movement = prefab.GetComponent<EnemyClass>();
            if (movement != null)
            {
                info.Speed = ReadPrivateFloat(movement, "baseSpeed", info.Speed);
            }

            var drone = prefab.GetComponent<Drone>();
            if (drone != null)
            {
                info.HasRanged = true;
                info.RangedInterval = ReadSerialized(drone, "baseShootInterval", 3f);
                info.RangedSpeed = ReadSerialized(drone, "baseProjectileSpeed", 5f);
                info.RangedLifetime = ReadSerialized(drone, "baseProjectileLifetime", 5f);
                info.RangedDamageMul = 1f;
                info.IsMine = ReadSerialized(drone, "isMine", false) ? (byte)1 : (byte)0;
                info.MineCount = ReadSerialized(drone, "projectilePerShoot", 4);
                var projectile = ReadSerialized<GameObject>(drone, "projectilePrefab", null);
                if (projectile != null && PresentationRegistry.Instance != null)
                {
                    info.RangedPrefabId = PresentationRegistry.Instance.RegisterPrefab(projectile);
                }
            }

            var elemental = prefab.GetComponent<Elemental>();
            if (elemental != null)
            {
                info.HasRanged = ReadSerialized(elemental, "isElite", false);
                info.RangedInterval = ReadSerialized(elemental, "baseShootInterval", 5f);
                info.RangedSpeed = ReadSerialized(elemental, "baseProjectileSpeed", 2f);
                info.RangedLifetime = ReadSerialized(elemental, "baseProjectileLifetime", 15f);
                info.RangedDamageMul = 5f;
                var cyclone = ReadSerialized<GameObject>(elemental, "projectilePrefab", null);
                if (cyclone != null && PresentationRegistry.Instance != null)
                {
                    info.RangedPrefabId = PresentationRegistry.Instance.RegisterPrefab(cyclone);
                }
            }

            var explosive = prefab.GetComponent<ExplosiveEnemy>();
            if (explosive != null)
            {
                info.HasExplode = true;
                info.ExplodeRadius = ReadSerialized(explosive, "explosionRadius", 1f);
                info.ExplodeDamage = info.ContactDamage;
            }

            var split = prefab.GetComponent<SlimeDeath>();
            if (split != null && ReadSerialized(split, "canDoppel", false) && split.summons != null && split.summons.Count > 0)
            {
                info.HasSplit = true;
                info.SplitCount = 2;
                var child = split.summons[0];
                if (child != null && PresentationRegistry.Instance != null)
                {
                    info.SplitPrefabId = PresentationRegistry.Instance.RegisterPrefab(child);
                }
            }

            int prefabId = PresentationRegistry.Instance != null
                ? PresentationRegistry.Instance.RegisterPrefab(prefab)
                : 0;

            return new CachedEnemy { PrefabId = prefabId, Info = info, SplitPrefab = split != null && split.summons.Count > 0 ? split.summons[0] : null };
        }

        private static float ReadPrivateFloat(object target, string name, float fallback)
        {
            var field = target.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field != null && field.GetValue(target) is float value ? value : fallback;
        }

        private static T ReadSerialized<T>(object target, string name, T fallback)
        {
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
            var field = target.GetType().GetField(name, flags);
            if (field != null && field.GetValue(target) is T value)
            {
                return value;
            }

            return fallback;
        }

        public struct CachedEnemy
        {
            public int PrefabId;
            public EnemySpawnInfo Info;
            public GameObject SplitPrefab;
        }
    }
}
