using UnityEngine;
using UnityEngine.UI;
using WelCome.Core;
using WelCome.Systems;

namespace WelCome.UI
{
    public class HUDPanel : UIPanel
    {
        private Text _dayText;
        private Text _phaseText;
        private Text _goldText;
        private Text _apText;
        private Text _mpText;
        private Text _attentionText;
        private Text _supportText;

        protected override void BuildUI()
        {
            var topBar = UIHelper.CreatePanel("TopBar", transform, new Color(0.1f, 0.1f, 0.14f, 0.85f));
            topBar.GetComponent<RectTransform>().Set(0f, 0.92f, 1f, 1f);

            float x = 0.02f;
            _dayText = CreateHUDLabel("Day", topBar.transform, ref x, 0.1f);
            _phaseText = CreateHUDLabel("Phase", topBar.transform, ref x, 0.1f);
            _goldText = CreateHUDLabel("Gold", topBar.transform, ref x, 0.12f);
            _apText = CreateHUDLabel("AP", topBar.transform, ref x, 0.12f);
            _mpText = CreateHUDLabel("MP", topBar.transform, ref x, 0.12f);
            _attentionText = CreateHUDLabel("Attention", topBar.transform, ref x, 0.12f);
            _supportText = CreateHUDLabel("Support", topBar.transform, ref x, 0.12f);

            UIHelper.CreateButton("SettingsBtn", topBar.transform, "⚙",
                () => UIManager.Instance.ShowOverlay<SettingsPanel>(),
                new Color(0.3f, 0.3f, 0.35f), 22)
                .GetComponent<RectTransform>().Set(0.93f, 0.1f, 0.99f, 0.9f);
        }

        protected override void OnShow()
        {
            RefreshAll();
            SubscribeEvents();
        }

        protected override void OnHide()
        {
            UnsubscribeEvents();
        }

        private Text CreateHUDLabel(string name, Transform parent, ref float x, float width)
        {
            var txt = UIHelper.CreateText(name, parent, "", 20, TextAnchor.MiddleCenter);
            txt.GetComponent<RectTransform>().Set(x, 0.1f, x + width, 0.9f);
            x += width + 0.01f;
            return txt;
        }

        private void RefreshAll()
        {
            var gm = GameManager.Instance;
            var res = ResourceSystem.Instance;
            var time = TimeSystem.Instance;

            _dayText.text = $"第 {gm.CurrentDay} 天";
            _phaseText.text = gm.CurrentState == GameState.DayPhase ? "白天" : "夜晚";
            _goldText.text = $"金币: {res.Gold}";
            _apText.text = $"行动力: {time.ActionPointsCurrent}/{time.ActionPointsMax}";
            _mpText.text = $"魔力: {res.MagicPointsCurrent}/{res.MagicPointsMax}";
            _attentionText.text = $"关注度: {res.Attention}";
            _supportText.text = $"民心: {res.Support}";
        }

        private void SubscribeEvents()
        {
            EventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
            EventBus.Subscribe<ActionPointsChangedEvent>(OnAPChanged);
            EventBus.Subscribe<MagicPointsChangedEvent>(OnMPChanged);
            EventBus.Subscribe<AttentionChangedEvent>(OnAttentionChanged);
            EventBus.Subscribe<SupportChangedEvent>(OnSupportChanged);
            EventBus.Subscribe<GameStateChangedEvent>(OnStateChanged);
        }

        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
            EventBus.Unsubscribe<ActionPointsChangedEvent>(OnAPChanged);
            EventBus.Unsubscribe<MagicPointsChangedEvent>(OnMPChanged);
            EventBus.Unsubscribe<AttentionChangedEvent>(OnAttentionChanged);
            EventBus.Unsubscribe<SupportChangedEvent>(OnSupportChanged);
            EventBus.Unsubscribe<GameStateChangedEvent>(OnStateChanged);
        }

        private void OnGoldChanged(GoldChangedEvent e) => _goldText.text = $"金币: {e.Current}";
        private void OnAPChanged(ActionPointsChangedEvent e) => _apText.text = $"行动力: {e.Current}/{e.Max}";
        private void OnMPChanged(MagicPointsChangedEvent e) => _mpText.text = $"魔力: {e.Current}/{e.Max}";
        private void OnAttentionChanged(AttentionChangedEvent e) => _attentionText.text = $"关注度: {e.Current}";
        private void OnSupportChanged(SupportChangedEvent e) => _supportText.text = $"民心: {e.Current}";
        private void OnStateChanged(GameStateChangedEvent e)
        {
            _dayText.text = $"第 {GameManager.Instance.CurrentDay} 天";
            _phaseText.text = e.NewState == GameState.DayPhase ? "白天" : "夜晚";
        }
    }
}
