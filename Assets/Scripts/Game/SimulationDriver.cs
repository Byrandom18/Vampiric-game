using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Vampiric.Combat;
using Vampiric.Items;
using ArtifactDefinition = Vampiric.Items.ArtifactDefinition;
using Vampiric.Presentation;
using Vampiric.Simulation;
using Vampiric.Stats;

namespace Vampiric.Game
{
    public sealed class SimulationDriver : MonoBehaviour
    {
        public static SimulationDriver Instance { get; private set; }

        [SerializeField] private GameObject _damageTextPrefab;
        [SerializeField] private GameObject _expPrefab;
        [SerializeField] private GameObject _goldPrefab;
        [SerializeField] private GameObject _magnetPrefab;
        [SerializeField] private GameObject _heartPrefab;
        [SerializeField] private GameObject _bombPrefab;

        private SimEntityFactory _factory;
        private readonly EnemyPrefabCache _enemyCache = new();
        private DamagePopupService _popups;
        private int _expPrefabId;
        private int _goldPrefabId;
        private int _magnetPrefabId;
        private int _heartPrefabId;
        private int _bombPrefabId;
        private bool _magnetActive;
        private float _magnetTimeLeft;

        public SimEntityFactory Factory => _factory;
        public EnemyPrefabCache EnemyCache => _enemyCache;
        public static event System.Action EnemyKilled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            SimulationBridge.Initialize();
        }

        private void Start()
        {
            PullScenePrefabs();
            EnsureWorld();
            RegisterCollectibles();
            _popups = new DamagePopupService(_damageTextPrefab);
        }

        private void PullScenePrefabs()
        {
            if (ItemPoolManager.Instance != null)
            {
                _expPrefab = _expPrefab != null ? _expPrefab : ItemPoolManager.Instance.expPrefab;
                _goldPrefab = _goldPrefab != null ? _goldPrefab : ItemPoolManager.Instance.goldPrefab;
                _magnetPrefab = _magnetPrefab != null ? _magnetPrefab : ItemPoolManager.Instance.magnetPrefab;
                _heartPrefab = _heartPrefab != null ? _heartPrefab : ItemPoolManager.Instance.heartPrefab;
                _bombPrefab = _bombPrefab != null ? _bombPrefab : ItemPoolManager.Instance.bombPrefab;
            }

            if (_damageTextPrefab == null)
            {
                var textManager = FindFirstObjectByType<DamageTextManager>();
                if (textManager != null)
                {
                    _damageTextPrefab = textManager.damageTextPrefab;
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            SimulationBridge.Dispose();
        }

        public void EnsureWorld()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                DefaultWorldInitialization.Initialize("Default World", false);
                world = World.DefaultGameObjectInjectionWorld;
            }

            if (world == null)
            {
                return;
            }

            _factory ??= new SimEntityFactory(world);
            if (_factory.PlayerEntity == Entity.Null)
            {
                Vector3 pos = transform.position;
                if (PlayerMovement.Instance != null)
                {
                    pos = PlayerMovement.Instance.transform.position;
                }

                _factory.CreatePlayer(new float2(pos.x, pos.y));
            }
        }

        private void RegisterCollectibles()
        {
            var registry = PresentationRegistry.Instance;
            if (registry == null)
            {
                return;
            }

            _expPrefabId = registry.RegisterPrefab(_expPrefab);
            _goldPrefabId = registry.RegisterPrefab(_goldPrefab);
            _magnetPrefabId = registry.RegisterPrefab(_magnetPrefab);
            _heartPrefabId = registry.RegisterPrefab(_heartPrefab);
            _bombPrefabId = registry.RegisterPrefab(_bombPrefab);
        }

        private void LateUpdate()
        {
            if (_factory == null)
            {
                return;
            }

            DrainEnemyProjectiles();
            DrainPickups();
            DrainPlayerDamage();
            DrainDeaths();
            DrainPopups();
            _factory.DestroyPending();
            PresentationRegistry.Instance?.Sync(_factory.EntityManager);
        }

        public void SyncPlayerFromStats(PlayerStats stats, StatSheet sheet)
        {
            if (_factory == null || stats == null || sheet == null)
            {
                return;
            }

            if (_magnetTimeLeft > 0f)
            {
                _magnetTimeLeft -= Time.deltaTime;
                if (_magnetTimeLeft <= 0f)
                {
                    _magnetActive = false;
                }
            }

            Vector3 pos = stats.transform.position;
            var runtime = sheet.ToRuntime(new float2(pos.x, pos.y), stats.invulnerability, _magnetActive);
            _factory.SyncPlayer(runtime.Position, runtime, stats.health, stats.maxHealth);
        }

        public Entity SpawnEnemy(GameObject prefab, Vector3 position, float healthMul = 1f, float damageMul = 1f, float defenseAdd = 0f)
        {
            EnsureWorld();
            var cached = _enemyCache.Get(prefab);
            var info = cached.Info;
            info.Health *= healthMul;
            info.ContactDamage *= damageMul;
            info.Defense += defenseAdd;
            info.ExplodeDamage *= damageMul;
            return _factory.SpawnEnemy(cached.PrefabId, new float2(position.x, position.y), info);
        }

        public void SpawnExp(Vector3 position, float value)
        {
            if (_factory == null || _expPrefabId == 0)
            {
                return;
            }

            _factory.SpawnPickup(_expPrefabId, new float2(position.x, position.y), new PickupData
            {
                Type = PickupType.Exp,
                Value = value,
                AttractRadius = 2f,
                Speed = 10f,
                WaitLeft = 0.35f
            });
        }

        public void SpawnArtifactPickup(Vector3 position, ArtifactInstance artifact)
        {
            if (_factory == null || _expPrefabId == 0 || artifact == null)
            {
                return;
            }

            _factory.SpawnPickup(_expPrefabId, new float2(position.x, position.y), new PickupData
            {
                Type = PickupType.Equipment,
                Value = 1f,
                AttractRadius = 2.5f,
                Speed = 10f,
                WaitLeft = 0.2f,
                ArtifactDefinitionId = artifact.DefinitionId,
                ArtifactSeed = artifact.Seed
            });
        }

        private void DrainEnemyProjectiles()
        {
            while (SimulationBridge.EnemyProjectiles.IsCreated &&
                   SimulationBridge.EnemyProjectiles.TryDequeue(out var request))
            {
                _factory.SpawnProjectile(request);
            }
        }

        private void DrainPickups()
        {
            while (SimulationBridge.Pickups.IsCreated && SimulationBridge.Pickups.TryDequeue(out var pickup))
            {
                ApplyPickup(pickup);
            }
        }

        private void DrainPlayerDamage()
        {
            while (SimulationBridge.PlayerDamage.IsCreated && SimulationBridge.PlayerDamage.TryDequeue(out var damage))
            {
                PlayerStats.Instance?.TakeDamage(damage.Amount);
            }
        }

        private void DrainDeaths()
        {
            while (SimulationBridge.Deaths.IsCreated && SimulationBridge.Deaths.TryDequeue(out var death))
            {
                var position = new Vector3(death.Position.x, death.Position.y, 0f);
                SpawnExp(position, Mathf.Max(1f, death.Exp));
                PlayerStats.Instance?.AddKill();
                EnemyKilled?.Invoke();

                if (death.HasExplode != 0)
                {
                    _factory.ApplyExplosion(death.Position, death.ExplodeRadius, death.ExplodeDamage);
                }

                if (death.HasSplit != 0 && PresentationRegistry.Instance != null &&
                    PresentationRegistry.Instance.Catalog.TryGet(death.SplitPrefabId, out var splitPrefab))
                {
                    SpawnEnemy(splitPrefab, position + Vector3.right * 0.25f);
                    SpawnEnemy(splitPrefab, position + Vector3.left * 0.25f);
                }

                TryDropArtifact(position);
            }
        }

        private void TryDropArtifact(Vector3 position)
        {
            if (UnityEngine.Random.value > 0.08f)
            {
                return;
            }

            ArtifactCatalog.EnsureDefaults();
            var definitions = new System.Collections.Generic.List<Vampiric.Items.ArtifactDefinition>(ArtifactCatalog.All);
            if (definitions.Count == 0)
            {
                return;
            }

            var definition = definitions[UnityEngine.Random.Range(0, definitions.Count)];
            var instance = ArtifactGenerator.Create(definition);
            SpawnArtifactPickup(position, instance);
        }

        private void DrainPopups()
        {
            while (SimulationBridge.DamagePopups.IsCreated && SimulationBridge.DamagePopups.TryDequeue(out var popup))
            {
                _popups?.Spawn(popup);
            }
        }

        private void ApplyPickup(PickupCollectEvent pickup)
        {
            var stats = PlayerStats.Instance;
            if (stats == null)
            {
                return;
            }

            switch ((PickupType)pickup.PickupType)
            {
                case PickupType.Exp:
                    stats.AddExp(pickup.Value);
                    break;
                case PickupType.Gold:
                    stats.AddGold(pickup.Value);
                    break;
                case PickupType.Gem:
                    stats.AddGems(Mathf.RoundToInt(pickup.Value));
                    break;
                case PickupType.Heart:
                    stats.AddHealth(stats.maxHealth * 0.3f);
                    break;
                case PickupType.Magnet:
                    _magnetActive = true;
                    _magnetTimeLeft = 10f;
                    break;
                case PickupType.Bomb:
                    if (PlayerMovement.Instance != null)
                    {
                        var pos = PlayerMovement.Instance.transform.position;
                        _factory.ApplyExplosion(new float2(pos.x, pos.y), 5f, stats.GetTotalDamage() * 5f);
                    }

                    break;
                case PickupType.Equipment:
                    ArtifactRunInventory.Instance?.TryCollectGenerated(pickup.ArtifactDefinitionId, pickup.ArtifactSeed);
                    break;
            }
        }
    }
}
