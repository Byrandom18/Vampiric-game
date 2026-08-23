using System.Collections.Generic;
using UnityEngine;
using Vampiric.Combat;

namespace Vampiric.Weapons
{
    public sealed class WeaponLoadout
    {
        public sealed class Slot
        {
            public WeaponDefinition Definition;
            public int Level;
            public bool IsUnlocked;
            public float DamageMul = 1f;
            public float SpeedMul = 1f;
            public float LifetimeMul = 1f;
            public float SizeMul = 1f;
            public float IntervalMul = 1f;
            public int PierceAdd;
            public int CountAdd;
            public float ShredAdd;
            public float CooldownLeft;
        }

        private readonly Dictionary<WeaponId, Slot> _slots = new();

        public IEnumerable<Slot> ActiveSlots
        {
            get
            {
                foreach (var slot in _slots.Values)
                {
                    if (slot.IsUnlocked)
                    {
                        yield return slot;
                    }
                }
            }
        }

        public void Register(WeaponDefinition definition)
        {
            if (definition == null || _slots.ContainsKey(definition.Id))
            {
                return;
            }

            _slots[definition.Id] = new Slot { Definition = definition };
        }

        public Slot Get(WeaponId id)
        {
            return _slots.TryGetValue(id, out var slot) ? slot : null;
        }

        public bool IsUnlocked(WeaponId id)
        {
            return _slots.TryGetValue(id, out var slot) && slot.IsUnlocked;
        }

        public int GetLevel(WeaponId id)
        {
            return _slots.TryGetValue(id, out var slot) ? slot.Level : 0;
        }

        public bool IsMaxLevel(WeaponId id)
        {
            return _slots.TryGetValue(id, out var slot) &&
                   slot.IsUnlocked &&
                   slot.Definition != null &&
                   slot.Level >= slot.Definition.MaxLevel;
        }

        public void Unlock(WeaponId id)
        {
            if (!_slots.TryGetValue(id, out var slot) || slot.IsUnlocked)
            {
                return;
            }

            slot.IsUnlocked = true;
            slot.Level = 1;
        }

        public void Remove(WeaponId id)
        {
            if (_slots.TryGetValue(id, out var slot))
            {
                slot.IsUnlocked = false;
                slot.Level = 0;
            }
        }

        public void ApplyUpgrade(WeaponId id, CardData card)
        {
            if (!_slots.TryGetValue(id, out var slot) || card == null)
            {
                return;
            }

            if (!slot.IsUnlocked)
            {
                Unlock(id);
            }

            slot.Level = Mathf.Min(slot.Level + 1, slot.Definition != null ? slot.Definition.MaxLevel : 6);
            slot.DamageMul *= card.damageMultiplier;
            slot.SpeedMul *= card.speedMultiplier;
            slot.LifetimeMul *= card.lifetimeMultiplier;
            slot.SizeMul *= card.sizeMultiplier;
            slot.IntervalMul *= card.intervalMultiplier;
            slot.PierceAdd += card.penetrateAdd;
            slot.CountAdd += card.countAdd;
            slot.ShredAdd += card.defShredAdd;
        }

        public static WeaponId FromLegacy(WeaponType type)
        {
            return type switch
            {
                WeaponType.Cone => WeaponId.Cone,
                WeaponType.Spread => WeaponId.Spread,
                WeaponType.ArmorBreak => WeaponId.ArmorBreak,
                WeaponType.Minigun => WeaponId.Minigun,
                WeaponType.Homing => WeaponId.Homing,
                WeaponType.Bouncing => WeaponId.Bouncing,
                _ => WeaponId.None
            };
        }
    }
}
