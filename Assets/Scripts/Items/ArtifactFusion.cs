using System.Collections.Generic;
using UnityEngine;
using Vampiric.Stats;

namespace Vampiric.Items
{
    public static class ArtifactFusion
    {
        public static ArtifactInstance Combine(
            ArtifactInstance first,
            ArtifactInstance second,
            ArtifactFusionCatalog catalog)
        {
            if (first == null || second == null || catalog == null)
            {
                return null;
            }

            if (!catalog.TryFind(first.DefinitionId, second.DefinitionId, out var recipe))
            {
                return null;
            }

            var resultDefinition = ArtifactCatalog.GetDefinition(recipe.ResultId);
            if (resultDefinition == null)
            {
                return null;
            }

            AffixTable.GetAffixCountRange(resultDefinition.Rarity, out int min, out int max);
            int count = Random.Range(min, max + 1);
            var pool = new List<StatModifier>();
            pool.AddRange(first.Affixes);
            pool.AddRange(second.Affixes);

            var result = new ArtifactInstance
            {
                InstanceId = System.Guid.NewGuid().ToString("N"),
                DefinitionId = resultDefinition.Id,
                Rarity = resultDefinition.Rarity,
                RemainingRuns = AffixTable.MaxLifespan(resultDefinition.Rarity),
                AcquiredThisRun = false,
                Seed = (uint)Random.Range(1, int.MaxValue)
            };

            var used = new HashSet<StatId>();
            var available = new List<StatId>(AffixTable.Rollable);
            for (int i = 0; i < count; i++)
            {
                bool inherit = pool.Count > 0 && Random.value <= 0.6f;
                if (inherit)
                {
                    int index = Random.Range(0, pool.Count);
                    var affix = pool[index];
                    pool.RemoveAt(index);
                    if (used.Add(affix.Id))
                    {
                        available.Remove(affix.Id);
                        result.Affixes.Add(affix);
                        continue;
                    }
                }

                if (available.Count == 0)
                {
                    break;
                }

                int roll = Random.Range(0, available.Count);
                var stat = available[roll];
                available.RemoveAt(roll);
                used.Add(stat);
                result.Affixes.Add(new StatModifier(
                    stat,
                    AffixTable.BaseValue(stat) * AffixTable.RarityMultiplier(resultDefinition.Rarity)));
            }

            return result;
        }
    }
}
