using System;
using System.Collections.Generic;
using Vampiric.Items;

namespace Vampiric.Meta
{
    [Serializable]
    public sealed class GameSaveData
    {
        public float Gold;
        public int Gems;
        public List<ArtifactInstance> Stash = new();
        public List<ArtifactInstance> Equipped = new();
        public List<ArtifactInstance> Backpack = new();
    }
}
