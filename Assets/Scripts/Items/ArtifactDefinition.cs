using System.Collections.Generic;
using UnityEngine;
using Vampiric.Stats;

namespace Vampiric.Items
{
    [CreateAssetMenu(menuName = "Vampiric/Artifact Definition")]
    public sealed class ArtifactDefinition : ScriptableObject
    {
        [SerializeField] private int _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private ItemRarity _rarity;
        [SerializeField] private List<StatModifier> _fixedModifiers = new();

        public int Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public ItemRarity Rarity => _rarity;
        public IReadOnlyList<StatModifier> FixedModifiers => _fixedModifiers;

        public void Configure(int id, string displayName, ItemRarity rarity, params StatModifier[] modifiers)
        {
            _id = id;
            _displayName = displayName;
            _rarity = rarity;
            _fixedModifiers = new List<StatModifier>(modifiers);
        }
    }
}
