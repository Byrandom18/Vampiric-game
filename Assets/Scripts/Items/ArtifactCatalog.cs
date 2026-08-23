using System.Collections.Generic;
using Vampiric.Stats;

namespace Vampiric.Items
{
    public static class ArtifactCatalog
    {
        private static readonly Dictionary<int, ArtifactDefinition> Definitions = new();

        public static void Register(ArtifactDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            Definitions[definition.Id] = definition;
        }

        public static ArtifactDefinition GetDefinition(int id)
        {
            return Definitions.TryGetValue(id, out var definition) ? definition : null;
        }

        public static IEnumerable<ArtifactDefinition> All => Definitions.Values;

        public static void EnsureDefaults()
        {
            if (Definitions.Count > 0)
            {
                return;
            }

            Register(Create(1, "Патрон", ItemRarity.Rare, new StatModifier(StatId.ProjectileSpeed, 30f)));
            Register(Create(2, "Краеугольная линза", ItemRarity.Rare, new StatModifier(StatId.RangeMod, 20f)));
            Register(Create(3, "Мушка винтовки", ItemRarity.Rare, new StatModifier(StatId.ExtraProjectiles, 1f)));
            Register(Create(4, "Серебряный слиток", ItemRarity.Rare, new StatModifier(StatId.Penetration, 1f)));
            Register(Create(5, "Прицел сокола", ItemRarity.Epic,
                new StatModifier(StatId.RangeMod, 25f),
                new StatModifier(StatId.ExtraProjectiles, 1f)));
            Register(Create(6, "Серебряная пуля", ItemRarity.Epic,
                new StatModifier(StatId.Penetration, 2f),
                new StatModifier(StatId.ProjectileSpeed, 40f)));
            Register(Create(7, "Снаряд Богов", ItemRarity.Legendary,
                new StatModifier(StatId.RangeMod, 30f),
                new StatModifier(StatId.ExtraProjectiles, 2f),
                new StatModifier(StatId.Penetration, 3f),
                new StatModifier(StatId.ProjectileSpeed, 50f)));
        }

        private static ArtifactDefinition Create(int id, string name, ItemRarity rarity, params StatModifier[] modifiers)
        {
            var definition = UnityEngine.ScriptableObject.CreateInstance<ArtifactDefinition>();
            definition.Configure(id, name, rarity, modifiers);
            return definition;
        }
    }
}
