using System.Collections.Generic;
using UnityEngine;

namespace Vampiric.Game
{
    public sealed class PrefabCatalog
    {
        public const int MissingId = 0;

        private readonly Dictionary<int, GameObject> _idToPrefab = new();
        private readonly Dictionary<GameObject, int> _prefabToId = new();
        private int _nextId = 1;

        public int Register(GameObject prefab)
        {
            if (prefab == null)
            {
                return MissingId;
            }

            if (_prefabToId.TryGetValue(prefab, out int existing))
            {
                return existing;
            }

            int id = _nextId++;
            _prefabToId[prefab] = id;
            _idToPrefab[id] = prefab;
            return id;
        }

        public bool TryGet(int id, out GameObject prefab)
        {
            return _idToPrefab.TryGetValue(id, out prefab);
        }

        public GameObject Get(int id)
        {
            return _idToPrefab.TryGetValue(id, out var prefab) ? prefab : null;
        }
    }
}
