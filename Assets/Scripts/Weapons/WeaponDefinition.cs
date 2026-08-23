using UnityEngine;
using Vampiric.Combat;

namespace Vampiric.Weapons
{
    [CreateAssetMenu(menuName = "Vampiric/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private WeaponId _id;
        [SerializeField] private string _displayName;
        [SerializeField] private WeaponCategory _category;
        [SerializeField] private FirePattern _pattern;
        [SerializeField] private int _maxLevel = 6;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private float _baseInterval = 1f;
        [SerializeField] private float _baseSpeed = 10f;
        [SerializeField] private float _baseLifetime = 3f;
        [SerializeField] private float _baseDamage = 10f;
        [SerializeField] private int _basePierce = 1;
        [SerializeField] private float _baseSize = 1f;
        [SerializeField] private int _baseCount = 1;
        [SerializeField] private float _baseDefShred;
        [SerializeField] private float _coneAngle = 45f;
        [SerializeField] private float _spreadAngle = 15f;
        [SerializeField] private float _homingTurn = 3f;
        [SerializeField] private float _homingRange = 10f;
        [SerializeField] private float _bounceRadius = 10f;
        [SerializeField] private AimMode _aimMode = AimMode.Nearest;
        [SerializeField] private bool _isEvolution;

        public WeaponId Id => _id;
        public string DisplayName => _displayName;
        public WeaponCategory Category => _category;
        public FirePattern Pattern => _pattern;
        public int MaxLevel => _maxLevel;
        public GameObject ProjectilePrefab => _projectilePrefab;
        public float BaseInterval => _baseInterval;
        public float BaseSpeed => _baseSpeed;
        public float BaseLifetime => _baseLifetime;
        public float BaseDamage => _baseDamage;
        public int BasePierce => _basePierce;
        public float BaseSize => _baseSize;
        public int BaseCount => _baseCount;
        public float BaseDefShred => _baseDefShred;
        public float ConeAngle => _coneAngle;
        public float SpreadAngle => _spreadAngle;
        public float HomingTurn => _homingTurn;
        public float HomingRange => _homingRange;
        public float BounceRadius => _bounceRadius;
        public AimMode Aim => _aimMode;
        public bool IsEvolution => _isEvolution;

        public void Configure(
            WeaponId id,
            string displayName,
            WeaponCategory category,
            FirePattern pattern,
            GameObject projectile,
            AimMode aim = AimMode.Nearest,
            bool evolution = false,
            int maxLevel = 6)
        {
            _id = id;
            _displayName = displayName;
            _category = category;
            _pattern = pattern;
            _projectilePrefab = projectile;
            _aimMode = aim;
            _isEvolution = evolution;
            _maxLevel = maxLevel;
        }
    }
}
