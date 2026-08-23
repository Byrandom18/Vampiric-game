using System.Collections.Generic;
using UnityEngine;
using Vampiric.Stats;

namespace Vampiric.Items
{
    public static class ArtifactGenerator
    {
        public static ArtifactInstance Create(ArtifactDefinition definition, uint seed = 0)
        {
            if (definition == null)
            {
                return null;
            }

            if (seed == 0)
            {
                seed = (uint)Random.Range(1, int.MaxValue);
            }

            var random = new Unity.Mathematics.Random(seed);
            AffixTable.GetAffixCountRange(definition.Rarity, out int min, out int max);
            int count = random.NextInt(min, max + 1);
            var available = new List<StatId>(AffixTable.Rollable);
            var instance = new ArtifactInstance
            {
                InstanceId = System.Guid.NewGuid().ToString("N"),
                DefinitionId = definition.Id,
                Rarity = definition.Rarity,
                Seed = seed,
                RemainingRuns = AffixTable.MaxLifespan(definition.Rarity),
                AcquiredThisRun = true
            };

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int index = random.NextInt(0, available.Count);
                var stat = available[index];
                available.RemoveAt(index);
                instance.Affixes.Add(new StatModifier(
                    stat,
                    AffixTable.BaseValue(stat) * AffixTable.RarityMultiplier(definition.Rarity)));
            }

            return instance;
        }

        public static ArtifactInstance RecreateFromSeed(int definitionId, uint seed)
        {
            return Create(ArtifactCatalog.GetDefinition(definitionId), seed);
        }
    }
}
