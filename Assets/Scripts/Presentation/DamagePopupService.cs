using System.Collections.Generic;
using UnityEngine;
using Vampiric.Combat;

namespace Vampiric.Presentation
{
    public sealed class DamagePopupService
    {
        private readonly GameObject _prefab;
        private readonly Queue<GameObject> _pool = new();

        public DamagePopupService(GameObject prefab)
        {
            _prefab = prefab;
        }

        public void Spawn(DamagePopupEvent popup)
        {
            if (_prefab == null)
            {
                return;
            }

            var instance = _pool.Count > 0 ? _pool.Dequeue() : Object.Instantiate(_prefab);
            instance.SetActive(true);
            instance.transform.position = new Vector3(popup.Position.x, popup.Position.y, 0f);
            var text = instance.GetComponent<DamageText>();
            if (text != null)
            {
                Color color = popup.IsCrit != 0 ? Color.yellow : Color.red;
                float size = popup.IsCrit != 0 ? 6f : 4f;
                text.Initialize(Mathf.RoundToInt(popup.Amount), color, size, instance.transform.position);
            }

            Object.Destroy(instance, 2.2f);
        }
    }
}
