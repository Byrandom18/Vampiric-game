using UnityEngine;
using Vampiric.Combat;
using Vampiric.Items;
using Vampiric.Presentation;
using Vampiric.Weapons;

namespace Vampiric.Game
{
    public sealed class GameContentBootstrap : MonoBehaviour
    {
        public static GameContentBootstrap Instance { get; private set; }

        public WeaponEvolutionCatalog Evolutions { get; private set; }
        public ArtifactFusionCatalog Fusions { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Hub")
            {
                return;
            }

            if (FindFirstObjectByType<GameContentBootstrap>() != null)
            {
                return;
            }

            new GameObject("GameContentBootstrap").AddComponent<GameContentBootstrap>();
        }

        private void Awake()
        {
            Instance = this;
            ArtifactCatalog.EnsureDefaults();
            Evolutions = WeaponEvolutionCatalog.CreateDefault();
            Fusions = ArtifactFusionCatalog.CreateDefault();
        }

        private void Start()
        {
            EnsurePresentation();
            RegisterWeapons();
            LoadRunArtifacts();
        }

        private static void EnsurePresentation()
        {
            if (PresentationRegistry.Instance == null)
            {
                var go = new GameObject("PresentationRegistry");
                go.AddComponent<PresentationRegistry>();
            }

            if (SimulationDriver.Instance == null)
            {
                var go = new GameObject("SimulationDriver");
                go.AddComponent<SimulationDriver>();
            }

            if (WeaponFireDirector.Instance == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                (player != null ? player : gameObjectFallback()).AddComponent<WeaponFireDirector>();
            }

            if (ArtifactRunInventory.Instance == null)
            {
                var go = new GameObject("ArtifactRunInventory");
                go.AddComponent<ArtifactRunInventory>();
            }
        }

        private static GameObject gameObjectFallback()
        {
            return new GameObject("WeaponFireDirector");
        }

        private void RegisterWeapons()
        {
            var director = WeaponFireDirector.Instance;
            if (director == null)
            {
                return;
            }

            director.RegisterDefinition(Make(WeaponId.Cone, "Веер шторма", WeaponCategory.Ranged, FirePattern.Cone, FindPrefab<ConeScript>(), AimMode.Nearest));
            director.RegisterDefinition(Make(WeaponId.Spread, "Круговой залп", WeaponCategory.Area, FirePattern.CircleSpread, FindPrefab<SpreadScript>()));
            director.RegisterDefinition(Make(WeaponId.ArmorBreak, "Пробой брони", WeaponCategory.Ranged, FirePattern.ArmorBreak, FindPrefab<ArmorBreakScript>(), AimMode.Strongest));
            director.RegisterDefinition(Make(WeaponId.Minigun, "Миниган", WeaponCategory.Ranged, FirePattern.Minigun, FindPrefab<MinigunScript>()));
            director.RegisterDefinition(Make(WeaponId.Homing, "Самонаведение", WeaponCategory.Ranged, FirePattern.HomingSpread, FindPrefab<HomingSpreadScript>()));
            director.RegisterDefinition(Make(WeaponId.Bouncing, "Рикошет", WeaponCategory.Area, FirePattern.Bounce, FindPrefab<BouncingScript>(), AimMode.Strongest));
            director.RegisterDefinition(Make(WeaponId.Grenade, "Граната", WeaponCategory.Explosive, FirePattern.Grenade, FindPrefab<ArmorBreakScript>(), AimMode.Nearest, false, 1));
            director.RegisterDefinition(Make(WeaponId.ExplosiveMinigun, "Взрывной миниган", WeaponCategory.Explosive, FirePattern.Minigun, FindPrefab<MinigunScript>(), AimMode.Nearest, true, 1));
            director.RegisterDefinition(Make(WeaponId.HomingFan, "Самонаводящийся веер", WeaponCategory.Ranged, FirePattern.HomingSpread, FindPrefab<HomingSpreadScript>(), AimMode.Nearest, true, 1));
            director.RegisterDefinition(Make(WeaponId.RicochetCone, "Рикошетный конус", WeaponCategory.Area, FirePattern.Cone, FindPrefab<ConeScript>(), AimMode.Nearest, true, 1));
        }

        private static WeaponDefinition Make(
            WeaponId id,
            string name,
            WeaponCategory category,
            FirePattern pattern,
            GameObject projectile,
            AimMode aim = AimMode.Nearest,
            bool evolution = false,
            int maxLevel = 6)
        {
            var definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            definition.Configure(id, name, category, pattern, projectile, aim, evolution, maxLevel);
            return definition;
        }

        private static GameObject FindPrefab<T>() where T : MonoBehaviour
        {
            var behaviour = FindFirstObjectByType<T>();
            if (behaviour == null)
            {
                return null;
            }

            var field = typeof(T).GetField("projectilePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(behaviour) as GameObject;
        }

        private static void LoadRunArtifacts()
        {
            var profile = Meta.ProfileState.Current ?? Meta.ProfileState.LoadOrCreate();
            ArtifactRunInventory.Instance?.LoadFromProfile(profile.Data.Equipped, profile.Data.Backpack);
        }
    }
}
