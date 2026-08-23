using System;
using System.Collections.Generic;
using Vampiric.Stats;

namespace Vampiric.Items
{
    [Serializable]
    public sealed class ArtifactInstance
    {
        public string InstanceId;
        public int DefinitionId;
        public ItemRarity Rarity;
        public uint Seed;
        public int RemainingRuns;
        public bool AcquiredThisRun;
        public List<StatModifier> Affixes = new();

        public ArtifactDefinition Definition => ArtifactCatalog.GetDefinition(DefinitionId);

        public IEnumerable<StatModifier> AllModifiers
        {
            get
            {
                var definition = Definition;
                if (definition != null)
                {
                    foreach (var modifier in definition.FixedModifiers)
                    {
                        yield return modifier;
                    }
                }

                foreach (var affix in Affixes)
                {
                    yield return affix;
                }
            }
        }
    }
}
