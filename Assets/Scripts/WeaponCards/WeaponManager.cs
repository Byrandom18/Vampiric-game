//// WeaponManager.cs
//using UnityEngine;
//using System.Collections.Generic;

//public class WeaponManager : MonoBehaviour
//{
//    [System.Serializable]
//    public class WeaponStatus
//    {
//        public bool isUnlocked = false;
//        public int level = 0;
//        public float damageMultiplier = 1f;
//        public float speedMultiplier = 1f;
//        public float lifetimeMultiplier = 1f;
//        public float sizeMultiplier = 1f;
//        public float intervalMultiplier = 1f;
//        public int penetrateAdd = 0;
//        public int countAdd = 0;
//        public float defShredAdd = 0f;
//    }

//    public static WeaponManager Instance;

//    [Header("Weapon References")]
//    public ConeScript coneWeapon;
//    public SpreadScript spreadWeapon;
//    public ArmorBreakScript armorBreakWeapon;
//    public MinigunScript minigunWeapon;
//    public HomingSpreadScript homingWeapon;
//    public BouncingScript bouncingWeapon;

//    public Dictionary<WeaponType, WeaponStatus> weaponStatuses = new Dictionary<WeaponType, WeaponStatus>();

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }

//        InitializeWeaponStatuses();
//    }

//    private void InitializeWeaponStatuses()
//    {
//        foreach (WeaponType type in System.Enum.GetValues(typeof(WeaponType)))
//        {
//            weaponStatuses[type] = new WeaponStatus();
//        }
//    }

//    public void ApplyCardEffect(CardData card)
//    {
//        if (card.isWeaponUnlock)
//        {
//            UnlockWeapon(card.weaponType);
//        }
//        else
//        {
//            UpgradeWeapon(card);
//        }

//        UpdateWeaponParameters(card.weaponType);
//    }

//    private void UnlockWeapon(WeaponType weaponType)
//    {
//        weaponStatuses[weaponType].isUnlocked = true;
//        weaponStatuses[weaponType].level = 1;
//        ActivateWeapon(weaponType);
//    }

//    private void UpgradeWeapon(CardData card)
//    {
//        WeaponStatus status = weaponStatuses[card.weaponType];
//        status.level++;

//        // Apply multipliers
//        status.damageMultiplier *= card.damageMultiplier;
//        status.speedMultiplier *= card.speedMultiplier;
//        status.lifetimeMultiplier *= card.lifetimeMultiplier;
//        status.sizeMultiplier *= card.sizeMultiplier;
//        status.intervalMultiplier *= card.intervalMultiplier;

//        // Apply additive bonuses
//        status.penetrateAdd += card.penetrateAdd;
//        status.countAdd += card.countAdd;
//        status.defShredAdd += card.defShredAdd;
//    }

//    private void UpdateWeaponParameters(WeaponType weaponType)
//    {
//        WeaponStatus status = weaponStatuses[weaponType];

//        switch (weaponType)
//        {
//            case WeaponType.Cone:
//                UpdateWeaponStats(coneWeapon, status);
//                break;
//            case WeaponType.Axe:
//                UpdateWeaponStats(axeWeapon, status);
//                break;
//                // Add other weapons...
//        }
//    }

//    private void UpdateWeaponStats(WeaponBase weapon, WeaponStatus status)
//    {
//        weapon.baseDamage *= status.damageMultiplier;
//        weapon.baseSpeed *= status.speedMultiplier;
//        weapon.baseLifetime *= status.lifetimeMultiplier;
//        weapon.baseSize *= status.sizeMultiplier;
//        weapon.baseShootInterval *= status.intervalMultiplier;
//        weapon.basePenetrate += status.penetrateAdd;
//        weapon.baseCount += status.countAdd;
//        weapon.baseDefShred += status.defShredAdd;

//        // Reset multipliers for next upgrade
//        ResetMultipliers(status);
//    }

//    private void ResetMultipliers(WeaponStatus status)
//    {
//        status.damageMultiplier = 1f;
//        status.speedMultiplier = 1f;
//        status.lifetimeMultiplier = 1f;
//        status.sizeMultiplier = 1f;
//        status.intervalMultiplier = 1f;
//    }

//    private void ActivateWeapon(WeaponType weaponType)
//    {
//        switch (weaponType)
//        {
//            case WeaponType.Sword:
//                swordWeapon.gameObject.SetActive(true);
//                break;
//            case WeaponType.Axe:
//                axeWeapon.gameObject.SetActive(true);
//                break;
//                // Add other weapons...
//        }
//    }
//}