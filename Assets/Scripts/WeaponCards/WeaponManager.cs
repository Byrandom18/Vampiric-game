// WeaponManager.cs
using UnityEngine;
using System.Collections.Generic;



public class WeaponManager : MonoBehaviour
{
    [System.Serializable]
    public class WeaponStatus
    {
        public bool isUnlocked = false;
        public int level = 0;
        public float damageMultiplier = 1f;
        public float speedMultiplier = 1f;
        public float lifetimeMultiplier = 1f;
        public float sizeMultiplier = 1f;
        public float intervalMultiplier = 1f;
        public int penetrateAdd = 0;
        public int countAdd = 0;
        public float defShredAdd = 0f;
    }

    public static WeaponManager Instance;

    [Header("Weapon References")]
    public ConeScript coneWeapon;
    public SpreadScript spreadWeapon;
    public ArmorBreakScript armorBreakWeapon;
    public MinigunScript minigunWeapon;
    public HomingSpreadScript homingWeapon;
    public BouncingScript bouncingWeapon;

    public Dictionary<WeaponType, WeaponStatus> weaponStatuses = new Dictionary<WeaponType, WeaponStatus>();
    public Dictionary<WeaponType, WeaponBase> weaponScripts = new Dictionary<WeaponType, WeaponBase>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeWeaponStatuses();
        MapWeaponScripts();
    }

    private void InitializeWeaponStatuses()
    {
        foreach (WeaponType type in System.Enum.GetValues(typeof(WeaponType)))
        {
            weaponStatuses[type] = new WeaponStatus();
        }
    }

    private void MapWeaponScripts()
    {
        weaponScripts[WeaponType.Cone] = coneWeapon;
        weaponScripts[WeaponType.Spread] = spreadWeapon;
        weaponScripts[WeaponType.ArmorBreak] = armorBreakWeapon;
        weaponScripts[WeaponType.Minigun] = minigunWeapon;
        weaponScripts[WeaponType.Homing] = homingWeapon;
        weaponScripts[WeaponType.Bouncing] = bouncingWeapon;

        // Только логическое отключение, не физическое
        foreach (var weaponType in weaponScripts.Keys)
        {
            SetWeaponActive(weaponType, false);
        }
    }

    public void ApplyCardEffect(CardData card)
    {
        if (card.isWeaponUnlock)
        {
            UnlockWeapon(card.weaponType);
        }
        else
        {
            UpgradeWeapon(card);
        }

        UpdateWeaponParameters(card.weaponType);
    }

    private void UnlockWeapon(WeaponType weaponType)
    {
        weaponStatuses[weaponType].isUnlocked = true;
        weaponStatuses[weaponType].level = 1;

        if (weaponScripts.ContainsKey(weaponType) && weaponScripts[weaponType] != null)
        {
            weaponScripts[weaponType].gameObject.SetActive(true);
            weaponScripts[weaponType].UpdateStats();

            // Активируем оружие в WeaponScript
            SetWeaponActive(weaponType, true);
        }
    }

    private void SetWeaponActive(WeaponType weaponType, bool active)
    {
        WeaponScript weaponScript = FindFirstObjectByType<WeaponScript>();
        if (weaponScript != null)
        {
            switch (weaponType)
            {
                case WeaponType.Cone: weaponScript.isConeActive = active; break;
                case WeaponType.Spread: weaponScript.isSpreadActive = active; break;
                case WeaponType.ArmorBreak: weaponScript.isArmorBreakActive = active; break;
                case WeaponType.Minigun: weaponScript.isMinigunActive = active; break;
                case WeaponType.Homing: weaponScript.isHomingActive = active; break;
                case WeaponType.Bouncing: weaponScript.isBouncingActive = active; break;
            }
        }
    }

    private void UpgradeWeapon(CardData card)
    {
        WeaponStatus status = weaponStatuses[card.weaponType];
        status.level++;

        status.damageMultiplier *= card.damageMultiplier;
        status.speedMultiplier *= card.speedMultiplier;
        status.lifetimeMultiplier *= card.lifetimeMultiplier;
        status.sizeMultiplier *= card.sizeMultiplier;
        status.intervalMultiplier *= card.intervalMultiplier;
        status.penetrateAdd += card.penetrateAdd;
        status.countAdd += card.countAdd;
        status.defShredAdd += card.defShredAdd;
    }

    private void UpdateWeaponParameters(WeaponType weaponType)
    {
        if (!weaponScripts.ContainsKey(weaponType) || weaponScripts[weaponType] == null) return;

        WeaponStatus status = weaponStatuses[weaponType];
        WeaponBase weapon = weaponScripts[weaponType];

        weapon.ApplyTemporaryMultipliers(
            status.damageMultiplier,
            status.speedMultiplier,
            status.lifetimeMultiplier,
            status.sizeMultiplier,
            status.intervalMultiplier,
            status.penetrateAdd,
            status.countAdd,
            status.defShredAdd
        );
    }

    public bool IsWeaponUnlocked(WeaponType weaponType)
    {
        return weaponStatuses.ContainsKey(weaponType) && weaponStatuses[weaponType].isUnlocked;
    }
}