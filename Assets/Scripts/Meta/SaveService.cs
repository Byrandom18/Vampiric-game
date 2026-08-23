using System.IO;
using UnityEngine;
using Vampiric.Items;

namespace Vampiric.Meta
{
    public static class SaveService
    {
        private const string FileName = "vampiric_profile.json";

        public static string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public static GameSaveData Load()
        {
            ArtifactCatalog.EnsureDefaults();
            if (!File.Exists(Path))
            {
                return new GameSaveData();
            }

            try
            {
                return JsonUtility.FromJson<GameSaveData>(File.ReadAllText(Path)) ?? new GameSaveData();
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Failed to load save: {exception.Message}");
                return new GameSaveData();
            }
        }

        public static void Save(GameSaveData data)
        {
            File.WriteAllText(Path, JsonUtility.ToJson(data, true));
        }

        public static GameSaveData CaptureRun(float gold, int gems)
        {
            var data = Load();
            data.Gold = gold;
            data.Gems = gems;
            data.Equipped = new System.Collections.Generic.List<ArtifactInstance>();
            data.Backpack = new System.Collections.Generic.List<ArtifactInstance>();

            if (ArtifactRunInventory.Instance != null)
            {
                data.Equipped.AddRange(ArtifactRunInventory.Instance.Equipped);
                data.Backpack.AddRange(ArtifactRunInventory.Instance.Backpack);
            }

            return data;
        }

        public static void MergeRunIntoStash(GameSaveData data)
        {
            data.Stash ??= new System.Collections.Generic.List<ArtifactInstance>();
            if (data.Equipped != null)
            {
                data.Stash.AddRange(data.Equipped);
            }

            if (data.Backpack != null)
            {
                data.Stash.AddRange(data.Backpack);
            }
        }
    }
}
