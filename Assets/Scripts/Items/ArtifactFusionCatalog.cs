using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vampiric.Items
{
    [CreateAssetMenu(menuName = "Vampiric/Artifact Fusion Catalog")]
    public sealed class ArtifactFusionCatalog : ScriptableObject
    {
        [SerializeField] private List<Recipe> _recipes = new();

        [Serializable]
        public sealed class Recipe
        {
            public int FirstId;
            public int SecondId;
            public int ResultId;
        }

        public bool TryFind(int firstId, int secondId, out Recipe recipe)
        {
            for (int i = 0; i < _recipes.Count; i++)
            {
                var item = _recipes[i];
                if ((item.FirstId == firstId && item.SecondId == secondId) ||
                    (item.FirstId == secondId && item.SecondId == firstId))
                {
                    recipe = item;
                    return true;
                }
            }

            recipe = null;
            return false;
        }

        public static ArtifactFusionCatalog CreateDefault()
        {
            var catalog = CreateInstance<ArtifactFusionCatalog>();
            catalog._recipes = new List<Recipe>
            {
                new() { FirstId = 1, SecondId = 3, ResultId = 5 },
                new() { FirstId = 2, SecondId = 4, ResultId = 6 },
                new() { FirstId = 3, SecondId = 4, ResultId = 6 },
                new() { FirstId = 5, SecondId = 6, ResultId = 7 }
            };
            return catalog;
        }
    }
}
