using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WelCome.Core;
using WelCome.Models;
using WelCome.Systems;
using EventType = WelCome.Models.EventType;

namespace WelCome.UI
{
    public class DayPhasePanel : UIPanel
    {
        #region Map Data

        struct Landmark
        {
            public string Name;
            public Vector2 Anchor;
        }

        static readonly Landmark[] Landmarks =
        {
            new() { Name = "城  门",  Anchor = new(0.50f, 0.85f) },
            new() { Name = "酒  馆",  Anchor = new(0.18f, 0.66f) },
            new() { Name = "集  市",  Anchor = new(0.80f, 0.68f) },
            new() { Name = "密  林",  Anchor = new(0.14f, 0.38f) },
            new() { Name = "工  坊",  Anchor = new(0.50f, 0.42f) },
            new() { Name = "贫民窟",  Anchor = new(0.84f, 0.38f) },
            new() { Name = "矿  洞",  Anchor = new(0.30f, 0.16f) },
        };

        static readonly (int a, int b)[] Paths =
        {
            (0, 1), (0, 2),
            (1, 3), (1, 4),
            (2, 4), (2, 5),
            (3, 4), (3, 6),
            (4, 5),
        };

        static readonly Dictionary<EventType, int[]> TypeToLandmark = new()
        {
            { EventType.Gather,    new[] { 6, 3 } },
            { EventType.Intel,     new[] { 1, 2 } },
            { EventType.Social,    new[] { 2, 5 } },
            { EventType.MainStory, new[] { 0, 4 } },
            { EventType.Risk,      new[] { 3, 5 } },
        };

        #endregion

        private Transform _mapArea;
        private Transform _eventLayer;
        private Text _apText;

        private readonly List<GameObject> _eventNodes = new();

        protected override void BuildUI()
        {
            UIHelper.CreatePanel("BG", transform, new Color(0.15f, 0.13f, 0.1f, 0.95f));
            BuildMap();
            BuildBottomBar();
        }

        private void BuildMap()
        {
            var mapBg = UIHelper.CreatePanel("MapBG", transform, UIHelper.Parchment);
            mapBg.GetComponent<RectTransform>().Set(0.02f, 0.1f, 0.98f, 0.92f);
            _mapArea = mapBg.transform;

            var frame = UIHelper.CreatePanel("FrameBot", _mapArea,
                new Color(0.4f, 0.3f, 0.2f, 0.4f));
            frame.GetComponent<RectTransform>().Set(0f, 0f, 1f, 0.008f);
            var frameTop = UIHelper.CreatePanel("FrameTop", _mapArea,
                new Color(0.4f, 0.3f, 0.2f, 0.4f));
            frameTop.GetComponent<RectTransform>().Set(0f, 0.992f, 1f, 1f);

            var title = UIHelper.CreateText("MapTitle", _mapArea,
                "~ 大魔法时代 · 城镇地图 ~", 18, TextAnchor.UpperCenter,
                new Color(0.4f, 0.3f, 0.2f, 0.5f));
            title.GetComponent<RectTransform>().Set(0.2f, 0.93f, 0.8f, 0.99f);
            title.fontStyle = FontStyle.Italic;

            foreach (var conn in Paths)
                UIHelper.DrawDottedPath(_mapArea, Landmarks[conn.a].Anchor,
                    Landmarks[conn.b].Anchor, new Color(0.45f, 0.35f, 0.25f, 0.25f), 14, 2.5f);

            foreach (var lm in Landmarks)
                CreateLandmarkLabel(lm);

            var eventLayerGo = new GameObject("EventLayer");
            eventLayerGo.transform.SetParent(_mapArea, false);
            var elRect = eventLayerGo.AddComponent<RectTransform>();
            elRect.anchorMin = Vector2.zero;
            elRect.anchorMax = Vector2.one;
            elRect.offsetMin = elRect.offsetMax = Vector2.zero;
            _eventLayer = eventLayerGo.transform;

            var laia = UIHelper.CreatePortrait("Laia", transform,
                "Characters/Laia_sprite", 140, 280);
            laia.anchorMin = laia.anchorMax = new Vector2(0.06f, 0.1f);
            laia.anchoredPosition = Vector2.zero;
        }

        private void CreateLandmarkLabel(Landmark lm)
        {
            var label = UIHelper.CreateText($"LM_{lm.Name}", _mapArea,
                lm.Name, 28, TextAnchor.MiddleCenter,
                new Color(0.38f, 0.28f, 0.18f, 0.3f));
            label.fontStyle = FontStyle.Bold;
            var rt = label.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = lm.Anchor;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(140, 40);
            rt.pivot = new Vector2(0.5f, 0.5f);
            label.raycastTarget = false;
        }

        private void BuildBottomBar()
        {
            var bar = UIHelper.CreatePanel("BottomBar", transform,
                new Color(0.12f, 0.1f, 0.08f, 0.9f));
            bar.GetComponent<RectTransform>().Set(0f, 0f, 1f, 0.1f);

            _apText = UIHelper.CreateText("AP", bar.transform, "", 22,
                TextAnchor.MiddleLeft, UIHelper.AccentGold);
            _apText.GetComponent<RectTransform>().Set(0.05f, 0.1f, 0.5f, 0.9f);

            UIHelper.CreateButton("SkipBtn", bar.transform, "跳过 > 入夜",
                () => DayEventSystem.Instance.SkipToNight(),
                new Color(0.5f, 0.35f, 0.2f), 20)
                .GetComponent<RectTransform>().Set(0.65f, 0.15f, 0.95f, 0.85f);
        }

        protected override void OnShow()
        {
            DayEventSystem.Instance.GenerateEventsForDay(GameManager.Instance.CurrentDay);
            RebuildEventNodes();
            RefreshAP();
        }

        private void RebuildEventNodes()
        {
            foreach (var go in _eventNodes)
                Destroy(go);
            _eventNodes.Clear();

            var events = DayEventSystem.Instance.AvailableEvents;
            var slotCounts = new Dictionary<int, int>();

            foreach (var evt in events)
            {
                if (evt.IsCompleted) continue;

                int lmIdx = GetLandmarkForEvent(evt);
                if (!slotCounts.ContainsKey(lmIdx)) slotCounts[lmIdx] = 0;
                int slot = slotCounts[lmIdx]++;

                Vector2 pos = GetEventPosition(lmIdx, slot);
                CreateEventNode(evt, pos, lmIdx);
            }
        }

        private int GetLandmarkForEvent(DayEventInstance evt)
        {
            if (TypeToLandmark.TryGetValue(evt.Type, out var locs) && locs.Length > 0)
                return locs[evt.ConfigId % locs.Length];
            return 4;
        }

        private Vector2 GetEventPosition(int lmIdx, int slot)
        {
            var basePos = Landmarks[lmIdx].Anchor;
            float angle = slot * 1.8f + lmIdx * 0.7f;
            float radius = 0.06f + slot * 0.03f;
            float ox = Mathf.Cos(angle) * radius;
            float oy = Mathf.Sin(angle) * radius * 0.7f;
            return new Vector2(
                Mathf.Clamp(basePos.x + ox, 0.05f, 0.95f),
                Mathf.Clamp(basePos.y + oy, 0.05f, 0.92f));
        }

        private void CreateEventNode(DayEventInstance evt, Vector2 anchor, int lmIdx)
        {
            var typeColor = GetTypeColor(evt.Type);
            var typeIcon = GetTypeIcon(evt.Type);

            var root = UIHelper.CreateRect($"Evt_{evt.ConfigId}", _eventLayer);
            root.anchorMin = root.anchorMax = anchor;
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = new Vector2(120, 56);
            root.pivot = new Vector2(0.5f, 0.5f);

            var bgImg = root.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0.18f, 0.15f, 0.12f, 0.92f);
            bgImg.raycastTarget = true;

            var accent = UIHelper.CreatePanel("Accent", root,
                typeColor);
            accent.GetComponent<RectTransform>().Set(0f, 0f, 0.04f, 1f);

            var icon = UIHelper.CreateText("Icon", root, typeIcon, 16,
                TextAnchor.MiddleCenter, typeColor);
            icon.GetComponent<RectTransform>().Set(0.04f, 0.5f, 0.22f, 1f);
            icon.fontStyle = FontStyle.Bold;

            var titleText = UIHelper.CreateText("Title", root, TruncateTitle(evt.Title, 5), 14,
                TextAnchor.MiddleLeft, UIHelper.TextWhite);
            titleText.GetComponent<RectTransform>().Set(0.22f, 0.42f, 0.98f, 0.95f);

            var costText = UIHelper.CreateText("Cost", root,
                $"AP:{evt.ApCost}", 11, TextAnchor.MiddleLeft, UIHelper.TextGray);
            costText.GetComponent<RectTransform>().Set(0.22f, 0.05f, 0.55f, 0.45f);

            var typeLabel = UIHelper.CreateText("Type", root,
                GetTypeLabel(evt.Type), 10, TextAnchor.MiddleRight, typeColor);
            typeLabel.GetComponent<RectTransform>().Set(0.55f, 0.05f, 0.96f, 0.45f);

            var capturedEvt = evt;
            var capturedLm = lmIdx;
            var btn = root.gameObject.AddComponent<Button>();
            btn.targetGraphic = bgImg;
            btn.onClick.AddListener(() => OnEventClicked(capturedEvt, capturedLm));

            var colors = btn.colors;
            colors.highlightedColor = new Color(0.28f, 0.24f, 0.2f, 0.95f);
            colors.pressedColor = new Color(0.12f, 0.1f, 0.08f, 0.95f);
            btn.colors = colors;

            _eventNodes.Add(root.gameObject);
        }

        private void OnEventClicked(DayEventInstance evt, int lmIdx)
        {
            string locName = Landmarks[lmIdx].Name.Replace(" ", "");
            string apInfo = $"AP消耗: {evt.ApCost}";
            string desc = string.IsNullOrEmpty(evt.Description) ? "" : $"\n\n<color=#C8B89A>{evt.Description}</color>";
            string body = $"<color=#888>[{GetTypeLabel(evt.Type)}]  {apInfo}  ·  {locName}附近</color>{desc}";

            UIManager.Instance.ShowPopup(evt.Title, body,
                onConfirm: () => ExecuteEvent(evt, lmIdx),
                onCancel: () => { });
        }

        private void ExecuteEvent(DayEventInstance evt, int lmIdx)
        {
            if (!DayEventSystem.Instance.ExecuteEvent(evt)) return;

            string locName = Landmarks[lmIdx].Name.Replace(" ", "");
            string narrative = GenerateNarrative(evt, locName);
            string attentionWarning = GetAttentionWarning();

            UIManager.Instance.ShowPopup(evt.Title, narrative + attentionWarning, () =>
            {
                if (GameManager.Instance.CurrentState != GameState.DayPhase) return;
                RebuildEventNodes();
                RefreshAP();
            });
        }

        private void RefreshAP()
        {
            var ts = TimeSystem.Instance;
            _apText.text = $"行动力: {ts.ActionPointsCurrent} / {ts.ActionPointsMax}";
        }

        #region Helpers

        private string TruncateTitle(string title, int maxChars)
        {
            return title.Length <= maxChars ? title : title[..(maxChars - 1)] + "…";
        }

        private Color GetTypeColor(EventType type) => type switch
        {
            EventType.Gather => new Color(0.4f, 0.75f, 0.4f),
            EventType.Intel => new Color(0.4f, 0.6f, 0.9f),
            EventType.Social => new Color(0.75f, 0.55f, 0.85f),
            EventType.MainStory => UIHelper.AccentGold,
            EventType.Risk => new Color(0.9f, 0.4f, 0.35f),
            _ => UIHelper.TextGray
        };

        private string GetTypeIcon(EventType type) => type switch
        {
            EventType.Gather => "♦",
            EventType.Intel => "◈",
            EventType.Social => "♥",
            EventType.MainStory => "★",
            EventType.Risk => "⚠",
            _ => "●"
        };

        private string GetTypeLabel(EventType type) => type switch
        {
            EventType.Gather => "采集",
            EventType.Intel => "情报",
            EventType.Social => "社交",
            EventType.MainStory => "主线",
            EventType.Risk => "风险",
            _ => "未知"
        };

        private string GenerateNarrative(DayEventInstance evt, string locName)
        {
            if (!string.IsNullOrEmpty(evt.ResultText))
                return evt.ResultText + "\n\n" + FormatRewards(evt);

            string story = evt.Type switch
            {
                EventType.Gather =>
                    $"你前往<b>{locName}</b>，完成了「{evt.Title}」。" +
                    $"\n\n<color=#C8B89A>{evt.Description}</color>" +
                    "\n\n经过一番仔细搜寻，你满载而归。",

                EventType.Intel =>
                    $"你在<b>{locName}</b>多方打听，完成了「{evt.Title}」。" +
                    $"\n\n<color=#C8B89A>{evt.Description}</color>" +
                    "\n\n这些情报或许会在未来派上用场。",

                EventType.Social =>
                    $"你在<b>{locName}</b>完成了「{evt.Title}」。" +
                    $"\n\n<color=#C8B89A>{evt.Description}</color>" +
                    "\n\n在这个魔法被垄断的时代，每一份信任都弥足珍贵。",

                EventType.MainStory =>
                    $"<size=26>「{evt.Title}」</size>" +
                    $"\n\n<color=#C8B89A>{evt.Description}</color>" +
                    "\n\n……命运的齿轮已然转动。",

                EventType.Risk =>
                    $"你冒着风险在<b>{locName}</b>执行了「{evt.Title}」。" +
                    $"\n\n<color=#C8B89A>{evt.Description}</color>" +
                    "\n\n好在一切顺利——但有人正在暗中注视。",

                _ => $"你完成了「{evt.Title}」。\n\n{evt.Description}"
            };

            string rewards = FormatRewards(evt);
            return string.IsNullOrEmpty(rewards) ? story : story + "\n\n" + rewards;
        }

        private string FormatRewards(DayEventInstance evt)
        {
            var lines = new List<string>();
            if (evt.GoldReward > 0)
                lines.Add($"<color=#EBCA6A>  金币 +{evt.GoldReward}</color>");
            if (evt.GoldReward < 0)
                lines.Add($"<color=#E05050>  金币 {evt.GoldReward}</color>");
            foreach (var kv in evt.MaterialRewards)
                lines.Add($"  素材(#{kv.Key}) x{kv.Value}");
            if (evt.AttentionDelta != 0)
            {
                string color = evt.AttentionDelta > 0 ? "#E8A040" : "#60B060";
                lines.Add($"<color={color}>  关注度 {(evt.AttentionDelta > 0 ? "+" : "")}{evt.AttentionDelta}</color>");
            }
            if (evt.SupportDelta != 0)
            {
                string color = evt.SupportDelta > 0 ? "#60B060" : "#E05050";
                lines.Add($"<color={color}>  民心 {(evt.SupportDelta > 0 ? "+" : "")}{evt.SupportDelta}</color>");
            }
            return lines.Count > 0 ? "——————————\n" + string.Join("\n", lines) : "";
        }

        private string GetAttentionWarning()
        {
            int attention = ResourceSystem.Instance.Attention;
            if (attention >= 80)
                return "\n\n<color=#FF4040><b>！危险！</b> 你的行踪已引起魔法公会的高度关注。</color>";
            if (attention >= 50)
                return "\n\n<color=#E8A040>你感到有人在暗中打量你……</color>";
            return "";
        }

        #endregion
    }
}
