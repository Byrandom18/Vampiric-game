using System.Collections.Generic;
using UnityEngine;
using Vampiric.Weapons;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance;

    [Header("Weapon References")]
    public ConeScript coneWeapon;
    public SpreadScript spreadWeapon;
    public ArmorBreakScript armorBreakWeapon;
    public MinigunScript minigunWeapon;
    public HomingSpreadScript homingWeapon;
    public BouncingScript bouncingWeapon;

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

        DisableLegacyWeapons();
    }

    private void DisableLegacyWeapons()
    {
        if (coneWeapon != null) coneWeapon.enabled = false;
        if (spreadWeapon != null) spreadWeapon.enabled = false;
        if (armorBreakWeapon != null) armorBreakWeapon.enabled = false;
        if (minigunWeapon != null) minigunWeapon.enabled = false;
        if (homingWeapon != null) homingWeapon.enabled = false;
        if (bouncingWeapon != null) bouncingWeapon.enabled = false;
    }

    public void ApplyCardEffect(CardData card)
    {
        var director = WeaponFireDirector.Instance;
        if (director == null || card == null)
        {
            return;
        }

        if (card.isEvolution)
        {
            director.Loadout.Remove((WeaponId)card.firstWeaponId);
            director.Loadout.Remove((WeaponId)card.secondWeaponId);
            director.Loadout.Unlock((WeaponId)card.resultWeaponId);
            return;
        }

        if (card.resultWeaponId != 0 && card.isWeaponUnlock)
        {
            director.Loadout.Unlock((WeaponId)card.resultWeaponId);
            return;
        }

        var weaponId = WeaponLoadout.FromLegacy(card.weaponType);
        if (card.isWeaponUnlock)
        {
            director.Loadout.Unlock(weaponId);
            return;
        }

        director.Loadout.ApplyUpgrade(weaponId, card);
    }

    public bool IsWeaponUnlocked(WeaponType weaponType)
    {
        var director = WeaponFireDirector.Instance;
        return director != null && director.Loadout.IsUnlocked(WeaponLoadout.FromLegacy(weaponType));
    }

    public bool IsWeaponUnlocked(WeaponId weaponId)
    {
        var director = WeaponFireDirector.Instance;
        return director != null && director.Loadout.IsUnlocked(weaponId);
    }

    public Dictionary<WeaponType, bool> GetLegacyUnlockMap()
    {
        return new Dictionary<WeaponType, bool>();
    }
}
