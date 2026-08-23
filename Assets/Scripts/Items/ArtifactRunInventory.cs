using System;
using System.Collections.Generic;
using UnityEngine;
using Vampiric.Stats;
using Vampiric.UI;

namespace Vampiric.Items
{
    public sealed class ArtifactRunInventory : MonoBehaviour
    {
        public const int MaxEquipped = 6;
        public const int MaxBackpack = 6;

        public static ArtifactRunInventory Instance { get; private set; }

        private readonly List<ArtifactInstance> _equipped = new();
        private readonly List<ArtifactInstance> _backpack = new();

        public IReadOnlyList<ArtifactInstance> Equipped => _equipped;
        public IReadOnlyList<ArtifactInstance> Backpack => _backpack;

        public event Action Changed;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void LoadFromProfile(IEnumerable<ArtifactInstance> equipped, IEnumerable<ArtifactInstance> backpack)
        {
            _equipped.Clear();
            _backpack.Clear();
            if (equipped != null)
            {
                _equipped.AddRange(equipped);
            }

            if (backpack != null)
            {
                _backpack.AddRange(backpack);
            }

            ApplyToStats();
            Changed?.Invoke();
        }

        public bool TryCollectGenerated(int definitionId, uint seed)
        {
            var instance = ArtifactGenerator.RecreateFromSeed(definitionId, seed);
            return instance != null && TryCollect(instance);
        }

        public bool TryCollect(ArtifactInstance instance)
        {
            if (instance == null)
            {
                return false;
            }

            instance.AcquiredThisRun = true;
            if (_equipped.Count < MaxEquipped)
            {
                _equipped.Add(instance);
                ApplyToStats();
                Changed?.Invoke();
                return true;
            }

            if (_backpack.Count < MaxBackpack)
            {
                _backpack.Add(instance);
                Changed?.Invoke();
                return true;
            }

            ReplaceArtifactPanel.Show(instance, this);
            return false;
        }

        public void AcceptReplacement(ArtifactInstance incoming, bool fromEquipped, int index)
        {
            if (incoming == null)
            {
                return;
            }

            incoming.AcquiredThisRun = true;
            if (fromEquipped && index >= 0 && index < _equipped.Count)
            {
                _equipped[index] = incoming;
                ApplyToStats();
            }
            else if (!fromEquipped && index >= 0 && index < _backpack.Count)
            {
                _backpack[index] = incoming;
            }

            Changed?.Invoke();
        }

        public void ApplyToStats()
        {
            if (PlayerStats.Instance?.Sheet == null)
            {
                return;
            }

            var modifiers = new List<StatModifier>();
            for (int i = 0; i < _equipped.Count; i++)
            {
                foreach (var modifier in _equipped[i].AllModifiers)
                {
                    modifiers.Add(modifier);
                }
            }

            PlayerStats.Instance.Sheet.ReplaceSource(StatSheet.ArtifactSource, modifiers);
            PlayerStats.Instance.RefreshDerivedStats();
        }

        public void TickRunEnd()
        {
            AgeList(_equipped);
            AgeList(_backpack);
            ApplyToStats();
            Changed?.Invoke();
        }

        private static void AgeList(List<ArtifactInstance> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].AcquiredThisRun)
                {
                    list[i].AcquiredThisRun = false;
                    continue;
                }

                list[i].RemainingRuns -= 1;
                if (list[i].RemainingRuns <= 0)
                {
                    list.RemoveAt(i);
                }
            }
        }
    }
}
