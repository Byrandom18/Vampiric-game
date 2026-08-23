using UnityEngine;
using UnityEngine.UI;
using Vampiric.Items;

namespace Vampiric.UI
{
    public sealed class ReplaceArtifactPanel : MonoBehaviour
    {
        public static void Show(ArtifactInstance incoming, ArtifactRunInventory inventory)
        {
            var existing = FindFirstObjectByType<ReplaceArtifactPanel>();
            if (existing == null)
            {
                var go = new GameObject("ReplaceArtifactPanel");
                existing = go.AddComponent<ReplaceArtifactPanel>();
            }

            existing.Open(incoming, inventory);
        }

        private ArtifactInstance _incoming;
        private ArtifactRunInventory _inventory;
        private GameObject _root;

        private void Open(ArtifactInstance incoming, ArtifactRunInventory inventory)
        {
            _incoming = incoming;
            _inventory = inventory;
            Time.timeScale = 0f;
            Build();
        }

        private void Build()
        {
            if (_root != null)
            {
                Destroy(_root);
            }

            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            _root = new GameObject("ReplaceRoot", typeof(RectTransform), typeof(Image));
            _root.transform.SetParent(canvas.transform, false);
            var image = _root.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.75f);
            var rect = _root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            CreateLabel(_root.transform, $"Новый предмет: {incomingName()}\nЭкипировка и рюкзак полны. Замените слот или откажитесь.", new Vector2(0, 280));

            int y = 180;
            for (int i = 0; i < _inventory.Equipped.Count; i++)
            {
                int index = i;
                CreateButton(_root.transform, $"Экип {i + 1}: {NameOf(_inventory.Equipped[i])}", new Vector2(0, y), () => Accept(true, index));
                y -= 40;
            }

            y -= 10;
            for (int i = 0; i < _inventory.Backpack.Count; i++)
            {
                int index = i;
                CreateButton(_root.transform, $"Рюкзак {i + 1}: {NameOf(_inventory.Backpack[i])}", new Vector2(0, y), () => Accept(false, index));
                y -= 40;
            }

            CreateButton(_root.transform, "Отказаться", new Vector2(0, -300), Close);
        }

        private void Accept(bool equipped, int index)
        {
            _inventory.AcceptReplacement(_incoming, equipped, index);
            Close();
        }

        private void Close()
        {
            Time.timeScale = 1f;
            if (_root != null)
            {
                Destroy(_root);
            }
        }

        private string incomingName()
        {
            return NameOf(_incoming);
        }

        private static string NameOf(ArtifactInstance instance)
        {
            return instance?.Definition != null ? instance.Definition.DisplayName : "Артефакт";
        }

        private static void CreateLabel(Transform parent, string text, Vector2 position)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(900, 80);
            var label = go.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.fontSize = 22;
            label.text = text;
        }

        private static void CreateButton(Transform parent, string text, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(640, 36);
            go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.95f);
            go.GetComponent<Button>().onClick.AddListener(action);

            var labelGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelGo.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.fontSize = 16;
            label.text = text;
        }
    }
}
