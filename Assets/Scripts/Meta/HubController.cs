using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vampiric.Items;
using Vampiric.Stats;
using Vampiric.Weapons;

namespace Vampiric.Meta
{
    public sealed class HubController : MonoBehaviour
    {
        private ProfileState _profile;
        private ArtifactFusionCatalog _fusion;
        private ArtifactInstance _fuseFirst;
        private ArtifactInstance _fuseSecond;
        private readonly List<ArtifactInstance> _runEquip = new();
        private readonly List<ArtifactInstance> _runBag = new();
        private Text _status;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootIfHubScene()
        {
            if (SceneManager.GetActiveScene().name != "Hub")
            {
                return;
            }

            if (FindFirstObjectByType<HubController>() != null)
            {
                return;
            }

            new GameObject("Hub").AddComponent<HubController>();
        }

        private void Start()
        {
            ArtifactCatalog.EnsureDefaults();
            _profile = ProfileState.LoadOrCreate();
            _fusion = ArtifactFusionCatalog.CreateDefault();
            BuildUi();
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("HubCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
                DontDestroyOnLoad(es);
            }

            CreateLabel(canvas.transform, "Хаб", new Vector2(0, 320), 36);
            _status = CreateLabel(canvas.transform, StatusText(), new Vector2(0, 270), 18);
            CreateButton(canvas.transform, "Начать забег", new Vector2(0, 210), StartRun);
            CreateButton(canvas.transform, "Скрестить выбранные", new Vector2(-220, 160), FuseSelected);
            CreateButton(canvas.transform, "Починить выбранный", new Vector2(220, 160), RepairSelected);

            float y = 100;
            foreach (var item in _profile.Data.Stash)
            {
                var captured = item;
                CreateButton(canvas.transform, Format(item), new Vector2(0, y), () => ToggleLoadout(captured));
                y -= 36;
            }
        }

        private void ToggleLoadout(ArtifactInstance item)
        {
            if (_runEquip.Contains(item) || _runBag.Contains(item))
            {
                _runEquip.Remove(item);
                _runBag.Remove(item);
            }
            else if (_runEquip.Count < ArtifactRunInventory.MaxEquipped)
            {
                _runEquip.Add(item);
            }
            else if (_runBag.Count < ArtifactRunInventory.MaxBackpack)
            {
                _runBag.Add(item);
            }

            if (_fuseFirst == null)
            {
                _fuseFirst = item;
            }
            else if (_fuseSecond == null && item != _fuseFirst)
            {
                _fuseSecond = item;
            }
            else
            {
                _fuseFirst = item;
                _fuseSecond = null;
            }

            _status.text = StatusText();
        }

        private void FuseSelected()
        {
            var result = _profile.TryFuse(_fuseFirst, _fuseSecond, _fusion);
            _status.text = result != null
                ? $"Получено: {result.Definition?.DisplayName}"
                : "Нет рецепта или предметы не выбраны.";
            _fuseFirst = null;
            _fuseSecond = null;
        }

        private void RepairSelected()
        {
            var target = _fuseFirst ?? _fuseSecond;
            _status.text = _profile.TryRepair(target) ? "Предмет починен." : "Не хватает золота или предмет не выбран.";
        }

        private void StartRun()
        {
            _profile.SetRunLoadout(_runEquip, _runBag);
            SceneManager.LoadScene("SampleScene");
        }

        private string StatusText()
        {
            return $"Золото: {_profile.Data.Gold:0}   Экип: {_runEquip.Count}/6   Рюкзак: {_runBag.Count}/6   Склад: {_profile.Data.Stash.Count}";
        }

        private static string Format(ArtifactInstance item)
        {
            string name = item.Definition != null ? item.Definition.DisplayName : item.DefinitionId.ToString();
            return $"{name} [{item.Rarity}] жизнь {item.RemainingRuns}";
        }

        private static Text CreateLabel(Transform parent, string text, Vector2 position, int size)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(1100, 50);
            var label = go.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.fontSize = size;
            label.text = text;
            return label;
        }

        private static void CreateButton(Transform parent, string text, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(420, 36);
            go.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.22f, 0.95f);
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
            label.text = text;
        }
    }
}
