using System;
using System.Collections.Generic;
using Vampiric.Simulation;
using Vampiric.Stats;

namespace Vampiric.Stats
{
    public sealed class StatSheet
    {
        public const string ArtifactSource = "artifact";
        public const string CardSource = "card";
        public const string BaseSource = "base";

        private readonly Dictionary<StatId, float> _base = new();
        private readonly List<SourcedModifier> _modifiers = new();
        private readonly Dictionary<StatId, float> _cached = new();
        private bool _isDirty = true;

        public event Action Recalculated;

        public void SetBase(StatId id, float value)
        {
            _base[id] = value;
            _isDirty = true;
        }

        public void ReplaceSource(string source, IReadOnlyList<StatModifier> modifiers)
        {
            _modifiers.RemoveAll(item => item.Source == source);
            if (modifiers != null)
            {
                for (int i = 0; i < modifiers.Count; i++)
                {
                    _modifiers.Add(new SourcedModifier(source, modifiers[i]));
                }
            }

            _isDirty = true;
        }

        public void AddModifier(string source, StatModifier modifier)
        {
            _modifiers.Add(new SourcedModifier(source, modifier));
            _isDirty = true;
        }

        public void ClearSource(string source)
        {
            _modifiers.RemoveAll(item => item.Source == source);
            _isDirty = true;
        }

        public float Get(StatId id)
        {
            Recalculate();
            return _cached.TryGetValue(id, out float value) ? value : 0f;
        }

        public void Recalculate()
        {
            if (!_isDirty)
            {
                return;
            }

            _cached.Clear();
            foreach (var pair in _base)
            {
                _cached[pair.Key] = pair.Value;
            }

            for (int i = 0; i < _modifiers.Count; i++)
            {
                var modifier = _modifiers[i].Modifier;
                float current = _cached.TryGetValue(modifier.Id, out float value) ? value : 0f;
                _cached[modifier.Id] = modifier.Op == StatOp.Multiply
                    ? current * modifier.Value
                    : current + modifier.Value;
            }

            _isDirty = false;
            Recalculated?.Invoke();
        }

        public PlayerRuntimeStats ToRuntime(Unity.Mathematics.float2 position, bool invulnerable, bool magnet)
        {
            Recalculate();
            return new PlayerRuntimeStats
            {
                Position = position,
                Attack = Get(StatId.BaseAttack) * (1f + Get(StatId.AttackMod) * 0.01f) + Get(StatId.AttackFlat),
                DamageMod = Get(StatId.DamageMod),
                CritRate = Get(StatId.CritRate),
                CritDamage = Get(StatId.CritDamage),
                Defense = Get(StatId.Defense),
                DefenseShred = Get(StatId.DefenseShred),
                AreaMod = Get(StatId.AreaMod),
                ProjectileSpeed = Get(StatId.ProjectileSpeed),
                Durations = Get(StatId.Durations),
                CooldownReduction = UnityEngine.Mathf.Min(99f, Get(StatId.CooldownReduction)),
                ExtraProjectiles = (int)Get(StatId.ExtraProjectiles),
                RangeMod = Get(StatId.RangeMod),
                ExplosiveMod = Get(StatId.ExplosiveMod),
                AreaDamageMod = Get(StatId.AreaDamageMod),
                MeleeMod = Get(StatId.MeleeMod),
                PickupRadius = 0f,
                Invulnerable = invulnerable ? (byte)1 : (byte)0,
                MagnetActive = magnet ? (byte)1 : (byte)0
            };
        }

        private readonly struct SourcedModifier
        {
            public readonly string Source;
            public readonly StatModifier Modifier;

            public SourcedModifier(string source, StatModifier modifier)
            {
                Source = source;
                Modifier = modifier;
            }
        }
    }
}
