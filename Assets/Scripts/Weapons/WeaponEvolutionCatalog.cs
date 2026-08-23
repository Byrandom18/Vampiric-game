using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vampiric.Weapons
{
    [CreateAssetMenu(menuName = "Vampiric/Weapon Evolution Catalog")]
    public sealed class WeaponEvolutionCatalog : ScriptableObject
    {
        [SerializeField] private List<Recipe> _recipes = new();

        public IReadOnlyList<Recipe> Recipes => _recipes;

        [Serializable]
        public sealed class Recipe
        {
            public WeaponId First;
            public WeaponId Second;
            public WeaponId Result;
            public string DisplayName;
            public string Description;
        }

        public bool TryFind(WeaponId first, WeaponId second, out Recipe recipe)
        {
            for (int i = 0; i < _recipes.Count; i++)
            {
                var item = _recipes[i];
                if ((item.First == first && item.Second == second) ||
                    (item.First == second && item.Second == first))
                {
                    recipe = item;
                    return true;
                }
            }

            recipe = null;
            return false;
        }

        public static WeaponEvolutionCatalog CreateDefault()
        {
            var catalog = CreateInstance<WeaponEvolutionCatalog>();
            catalog._recipes = new List<Recipe>
            {
                new()
                {
                    First = WeaponId.Minigun,
                    Second = WeaponId.Grenade,
                    Result = WeaponId.ExplosiveMinigun,
                    DisplayName = "Взрывной миниган",
                    Description = "Объединяет миниган и гранаты в скорострельные разрывы."
                },
                new()
                {
                    First = WeaponId.Homing,
                    Second = WeaponId.Spread,
                    Result = WeaponId.HomingFan,
                    DisplayName = "Самонаводящийся веер",
                    Description = "Кольцо ракет, которые ищут цели сами."
                },
                new()
                {
                    First = WeaponId.Cone,
                    Second = WeaponId.Bouncing,
                    Result = WeaponId.RicochetCone,
                    DisplayName = "Рикошетный конус",
                    Description = "Веер снарядов, отскакивающих к новым врагам."
                }
            };
            return catalog;
        }
    }
}
