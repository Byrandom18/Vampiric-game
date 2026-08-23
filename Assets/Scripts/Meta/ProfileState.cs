using System.Collections.Generic;
using Vampiric.Items;
using Vampiric.Stats;

namespace Vampiric.Meta
{
    public sealed class ProfileState
    {
        public static ProfileState Current { get; private set; } = LoadOrCreate();

        public GameSaveData Data { get; private set; }

        public static ProfileState LoadOrCreate()
        {
            var state = new ProfileState { Data = SaveService.Load() };
            Current = state;
            return state;
        }

        public void Persist()
        {
            SaveService.Save(Data);
        }

        public IEnumerable<ArtifactInstance> Stash => Data.Stash;

        public bool TryRepair(ArtifactInstance artifact)
        {
            if (artifact == null)
            {
                return false;
            }

            int cost = AffixTable.RepairCost(artifact.Rarity);
            if (Data.Gold < cost)
            {
                return false;
            }

            Data.Gold -= cost;
            artifact.RemainingRuns = AffixTable.MaxLifespan(artifact.Rarity);
            Persist();
            return true;
        }

        public ArtifactInstance TryFuse(ArtifactInstance first, ArtifactInstance second, ArtifactFusionCatalog catalog)
        {
            var result = ArtifactFusion.Combine(first, second, catalog);
            if (result == null)
            {
                return null;
            }

            Data.Stash.Remove(first);
            Data.Stash.Remove(second);
            Data.Stash.Add(result);
            Persist();
            return result;
        }

        public void SetRunLoadout(List<ArtifactInstance> equipped, List<ArtifactInstance> backpack)
        {
            Data.Equipped = new List<ArtifactInstance>(equipped);
            Data.Backpack = new List<ArtifactInstance>(backpack);
            Persist();
        }

        public void CommitFinishedRun(float gold, int gems)
        {
            Data.Gold = gold;
            Data.Gems = gems;
            if (ArtifactRunInventory.Instance != null)
            {
                ArtifactRunInventory.Instance.TickRunEnd();
                Data.Equipped = new List<ArtifactInstance>(ArtifactRunInventory.Instance.Equipped);
                Data.Backpack = new List<ArtifactInstance>(ArtifactRunInventory.Instance.Backpack);
            }

            SaveService.MergeRunIntoStash(Data);
            Data.Equipped.Clear();
            Data.Backpack.Clear();
            Persist();
        }
    }
}
