using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Vampiric.Game;
using Vampiric.Simulation;

namespace Vampiric.Presentation
{
    public sealed class PresentationRegistry : MonoBehaviour
    {
        public static PresentationRegistry Instance { get; private set; }

        [SerializeField] private Transform _poolRoot;
        [SerializeField] private int _prewarmPerPrefab = 16;

        private readonly Dictionary<int, Queue<GameObject>> _pools = new();
        private readonly Dictionary<Entity, GameObject> _bound = new();
        private readonly HashSet<GameObject> _inUse = new();
        private PrefabCatalog _catalog;

        public PrefabCatalog Catalog => _catalog ??= new PrefabCatalog();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (_poolRoot == null)
            {
                var root = new GameObject("CompanionPool");
                root.transform.SetParent(transform, false);
                _poolRoot = root.transform;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public int RegisterPrefab(GameObject prefab, int prewarm = -1)
        {
            int id = Catalog.Register(prefab);
            if (id == PrefabCatalog.MissingId)
            {
                return id;
            }

            if (!_pools.ContainsKey(id))
            {
                _pools[id] = new Queue<GameObject>();
                int count = prewarm < 0 ? _prewarmPerPrefab : prewarm;
                for (int i = 0; i < count; i++)
                {
                    _pools[id].Enqueue(CreateInactive(prefab));
                }
            }

            return id;
        }

        public GameObject Bind(Entity entity, int prefabId, Vector3 position)
        {
            Unbind(entity);
            if (!Catalog.TryGet(prefabId, out var prefab))
            {
                return null;
            }

            if (!_pools.TryGetValue(prefabId, out var pool))
            {
                pool = new Queue<GameObject>();
                _pools[prefabId] = pool;
            }

            GameObject instance = pool.Count > 0 ? pool.Dequeue() : CreateInactive(prefab);
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.identity;
            DisableLegacySimulation(instance);
            instance.SetActive(true);
            _bound[entity] = instance;
            _inUse.Add(instance);
            return instance;
        }

        public void Unbind(Entity entity)
        {
            if (!_bound.TryGetValue(entity, out var instance))
            {
                return;
            }

            _bound.Remove(entity);
            _inUse.Remove(instance);
            instance.SetActive(false);
            int prefabId = 0;
            if (World.DefaultGameObjectInjectionWorld != null &&
                World.DefaultGameObjectInjectionWorld.EntityManager.Exists(entity) &&
                World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<CompanionLink>(entity))
            {
                prefabId = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<CompanionLink>(entity).PrefabId;
            }

            if (prefabId != 0 && _pools.TryGetValue(prefabId, out var pool))
            {
                pool.Enqueue(instance);
            }
            else
            {
                Destroy(instance);
            }
        }

        public void Sync(EntityManager entityManager)
        {
            if (_bound.Count == 0)
            {
                return;
            }

            var stale = ListPool<Entity>.Get();
            foreach (var pair in _bound)
            {
                if (!entityManager.Exists(pair.Key) || entityManager.HasComponent<PendingDestroy>(pair.Key))
                {
                    stale.Add(pair.Key);
                    continue;
                }

                if (!entityManager.HasComponent<LocalTransform>(pair.Key))
                {
                    continue;
                }

                var transform = entityManager.GetComponentData<LocalTransform>(pair.Key);
                pair.Value.transform.position = new Vector3(transform.Position.x, transform.Position.y, 0f);
                pair.Value.transform.rotation = transform.Rotation;
                float scale = Mathf.Abs(transform.Scale);
                float facing = transform.Scale < 0f ? -1f : 1f;
                Vector3 local = pair.Value.transform.localScale;
                local.x = Mathf.Abs(local.x) * facing;
                if (scale > 0.01f && Mathf.Abs(local.y - scale) > 0.01f)
                {
                    local.y = scale;
                    local.z = 1f;
                }

                pair.Value.transform.localScale = local;
            }

            for (int i = 0; i < stale.Count; i++)
            {
                Unbind(stale[i]);
            }

            ListPool<Entity>.Release(stale);
        }

        private GameObject CreateInactive(GameObject prefab)
        {
            var instance = Instantiate(prefab, _poolRoot);
            instance.SetActive(false);
            DisableLegacySimulation(instance);
            return instance;
        }

        private static void DisableLegacySimulation(GameObject instance)
        {
            foreach (var behaviour in instance.GetComponents<MonoBehaviour>())
            {
                if (behaviour == null)
                {
                    continue;
                }

                string typeName = behaviour.GetType().Name;
                if (typeName is "EnemyClass" or "EnemyDamage" or "Drone" or "Elemental"
                    or "ExplosiveEnemy" or "SlimeDeath" or "Projectile" or "Bouncing"
                    or "SeekingMissile" or "Cyclone" or "CollectibleItem")
                {
                    behaviour.enabled = false;
                }
            }

            foreach (var body in instance.GetComponents<Rigidbody2D>())
            {
                body.simulated = false;
            }
        }
    }

    internal static class ListPool<T>
    {
        private static readonly Stack<List<T>> Pool = new();

        public static List<T> Get()
        {
            return Pool.Count > 0 ? Pool.Pop() : new List<T>(64);
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            Pool.Push(list);
        }
    }
}
