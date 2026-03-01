using UnityEngine;
using UnityEngine.UI;
using WelCome.Core;

namespace WelCome.UI
{
    public class SettingsPanel : UIPanel
    {
        private Slider _masterSlider;
        private Slider _musicSlider;
        private Slider _sfxSlider;
        private Text _masterLabel;
        private Text _musicLabel;
        private Text _sfxLabel;

        protected override void BuildUI()
        {
            UIHelper.CreatePanel("Dim", transform, new Color(0, 0, 0, 0.65f), blockRaycast: true);

            var box = UIHelper.CreatePanel("Box", transform, new Color(0.14f, 0.13f, 0.18f, 0.98f));
            box.GetComponent<RectTransform>().Set(0.25f, 0.12f, 0.75f, 0.88f);

            var border = UIHelper.CreatePanel("Border", box.transform,
                new Color(0.92f, 0.78f, 0.42f, 0.2f));
            border.GetComponent<RectTransform>().Set(0.01f, 0.01f, 0.99f, 0.99f);

            UIHelper.CreateText("Title", box.transform, "设    置",
                32, TextAnchor.MiddleCenter, UIHelper.AccentGold)
                .GetComponent<RectTransform>().Set(0.1f, 0.88f, 0.9f, 0.97f);

            var divider = UIHelper.CreatePanel("Div", box.transform,
                new Color(0.92f, 0.78f, 0.42f, 0.25f));
            divider.GetComponent<RectTransform>().Set(0.1f, 0.87f, 0.9f, 0.875f);

            float y = 0.78f;
            (_masterSlider, _masterLabel) = CreateVolumeRow(box.transform, "主音量", ref y);
            (_musicSlider, _musicLabel) = CreateVolumeRow(box.transform, "音乐", ref y);
            (_sfxSlider, _sfxLabel) = CreateVolumeRow(box.transform, "音效", ref y);

            _masterSlider.onValueChanged.AddListener(v =>
            {
                _masterLabel.text = $"主音量: {Mathf.RoundToInt(v * 100)}%";
                AudioListener.volume = v;
            });
            _musicSlider.onValueChanged.AddListener(v =>
                _musicLabel.text = $"音乐: {Mathf.RoundToInt(v * 100)}%");
            _sfxSlider.onValueChanged.AddListener(v =>
                _sfxLabel.text = $"音效: {Mathf.RoundToInt(v * 100)}%");

            var divider2 = UIHelper.CreatePanel("Div2", box.transform,
                new Color(0.5f, 0.5f, 0.5f, 0.15f));
            divider2.GetComponent<RectTransform>().Set(0.1f, y + 0.01f, 0.9f, y + 0.015f);
            y -= 0.02f;

            // TODO: 手动存档功能暂未开放，后续版本启用
            // UIHelper.CreateButton("SaveBtn", box.transform, "手动存档",
            //     OnManualSave, new Color(0.35f, 0.45f, 0.55f), 20)
            //     .GetComponent<RectTransform>().Set(0.15f, y - 0.07f, 0.85f, y);
            // y -= 0.09f;

            UIHelper.CreateButton("LoadBtn", box.transform, "存档管理",
                OnSaveSlots, new Color(0.3f, 0.5f, 0.35f), 20)
                .GetComponent<RectTransform>().Set(0.15f, y - 0.07f, 0.85f, y);
            y -= 0.09f;

            UIHelper.CreateButton("MainMenuBtn", box.transform, "返回主菜单",
                OnReturnToMenu, new Color(0.55f, 0.35f, 0.25f), 20)
                .GetComponent<RectTransform>().Set(0.15f, y - 0.07f, 0.85f, y);

            UIHelper.CreateButton("CloseBtn", box.transform, "关闭",
                OnClose, new Color(0.4f, 0.38f, 0.35f), 20)
                .GetComponent<RectTransform>().Set(0.25f, 0.04f, 0.75f, 0.12f);
        }

        protected override void OnShow()
        {
            var settings = SaveManager.LoadSettings();
            _masterSlider.SetValueWithoutNotify(settings.masterVolume);
            _musicSlider.SetValueWithoutNotify(settings.musicVolume);
            _sfxSlider.SetValueWithoutNotify(settings.sfxVolume);
            _masterLabel.text = $"主音量: {Mathf.RoundToInt(settings.masterVolume * 100)}%";
            _musicLabel.text = $"音乐: {Mathf.RoundToInt(settings.musicVolume * 100)}%";
            _sfxLabel.text = $"音效: {Mathf.RoundToInt(settings.sfxVolume * 100)}%";
        }

        private (Slider slider, Text label) CreateVolumeRow(Transform parent, string name, ref float y)
        {
            var label = UIHelper.CreateText($"{name}Label", parent, $"{name}: 100%",
                18, TextAnchor.MiddleLeft, UIHelper.TextWhite);
            label.GetComponent<RectTransform>().Set(0.08f, y - 0.02f, 0.4f, y + 0.03f);

            var sliderRect = UIHelper.CreateRect($"{name}Slider", parent);
            sliderRect.Set(0.4f, y - 0.01f, 0.92f, y + 0.02f);

            var sliderBg = UIHelper.CreatePanel("BG", sliderRect, new Color(0.25f, 0.23f, 0.2f));
            sliderBg.GetComponent<RectTransform>().Set(0f, 0.3f, 1f, 0.7f);

            var fillArea = UIHelper.CreateRect("Fill Area", sliderRect);
            fillArea.Set(0f, 0.3f, 1f, 0.7f);

            var fill = UIHelper.CreateRect("Fill", fillArea);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            var fillImg = fill.gameObject.AddComponent<Image>();
            fillImg.color = UIHelper.AccentGold;

            var handleArea = UIHelper.CreateRect("Handle Slide Area", sliderRect);
            handleArea.Set(0f, 0f, 1f, 1f);
            handleArea.offsetMin = new Vector2(5, 0);
            handleArea.offsetMax = new Vector2(-5, 0);

            var handle = UIHelper.CreateRect("Handle", handleArea);
            handle.sizeDelta = new Vector2(16, 0);
            var handleImg = handle.gameObject.AddComponent<Image>();
            handleImg.color = Color.white;

            var slider = sliderRect.gameObject.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImg;

            y -= 0.08f;
            return (slider, label);
        }

        private void OnManualSave()
        {
            UIManager.Instance.HideOverlay<SettingsPanel>();
            UIManager.Instance.ShowSaveSlots(SaveSlotMode.Save,
                () => UIManager.Instance.ShowOverlay<SettingsPanel>());
        }

        private void OnSaveSlots()
        {
            UIManager.Instance.HideOverlay<SettingsPanel>();
            UIManager.Instance.ShowSaveSlots(SaveSlotMode.Load,
                () => UIManager.Instance.ShowOverlay<SettingsPanel>());
        }

        private void OnReturnToMenu()
        {
            UIManager.Instance.HideOverlay<SettingsPanel>();
            GameManager.Instance.ReturnToMainMenu();
        }

        private void OnClose()
        {
            SaveManager.SaveSettings(
                _masterSlider.value,
                _musicSlider.value,
                _sfxSlider.value);
            UIManager.Instance.HideOverlay<SettingsPanel>();
        }
    }
}
