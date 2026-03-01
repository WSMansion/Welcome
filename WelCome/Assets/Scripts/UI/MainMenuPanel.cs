using UnityEngine;
using UnityEngine.UI;
using WelCome.Core;

namespace WelCome.UI
{
    public class MainMenuPanel : UIPanel
    {
        private GameObject _continueBtn;
        private Text _saveInfo;
        private GameObject _loadBtn;

        protected override void BuildUI()
        {
            UIHelper.CreatePanel("BG", transform);

            var title = UIHelper.CreateText("Title", transform,
                "《大魔法时代》\n代号：开店",
                48, TextAnchor.MiddleCenter, UIHelper.AccentGold);
            title.GetComponent<RectTransform>().Set(0.2f, 0.58f, 0.8f, 0.82f);

            UIHelper.CreateText("Subtitle", transform,
                "在魔法被垄断的世界里，用一间小小的工坊，点燃一场革命",
                20, TextAnchor.MiddleCenter, UIHelper.TextGray)
                .GetComponent<RectTransform>().Set(0.15f, 0.50f, 0.85f, 0.57f);

            _continueBtn = UIHelper.CreateButton("ContinueBtn", transform, "继续游戏",
                OnContinue, new Color(0.3f, 0.5f, 0.35f))
                .gameObject;
            _continueBtn.GetComponent<RectTransform>().Set(0.35f, 0.39f, 0.65f, 0.47f);

            _saveInfo = UIHelper.CreateText("SaveInfo", transform, "",
                14, TextAnchor.MiddleCenter, UIHelper.TextGray);
            _saveInfo.GetComponent<RectTransform>().Set(0.2f, 0.47f, 0.8f, 0.51f);

            _loadBtn = UIHelper.CreateButton("LoadBtn", transform, "选择存档",
                OnLoadSlot, new Color(0.35f, 0.4f, 0.55f))
                .gameObject;
            _loadBtn.GetComponent<RectTransform>().Set(0.35f, 0.30f, 0.65f, 0.38f);

            UIHelper.CreateButton("StartBtn", transform, "新游戏",
                OnNewGame, UIHelper.AccentGold)
                .GetComponent<RectTransform>().Set(0.35f, 0.20f, 0.65f, 0.28f);

            UIHelper.CreateText("Version", transform,
                "v0.3.0 - Prototype", 14, TextAnchor.LowerRight, UIHelper.TextGray)
                .GetComponent<RectTransform>().Set(0.7f, 0.02f, 0.98f, 0.06f);
        }

        protected override void OnShow()
        {
            bool hasSave = SaveManager.HasAnySave();

            _continueBtn.SetActive(hasSave);
            _saveInfo.gameObject.SetActive(hasSave);
            _loadBtn.SetActive(hasSave);

            if (hasSave)
            {
                int recent = SaveManager.GetMostRecentSlot();
                if (recent >= 0)
                    _saveInfo.text = $"存档 {recent + 1}: {SaveManager.GetSlotSummary(recent) ?? ""}";
            }

            var settings = SaveManager.LoadSettings();
            SaveManager.ApplyAudioSettings(settings);
        }

        private void OnContinue()
        {
            int slot = SaveManager.GetMostRecentSlot();
            if (slot < 0) return;
            if (!SaveManager.Load(slot))
                UIManager.Instance.ShowPopup("读档失败", "存档文件损坏或不兼容，请选择其他存档。");
        }

        private void OnLoadSlot()
        {
            UIManager.Instance.ShowSaveSlots(SaveSlotMode.Load);
        }

        private void OnNewGame()
        {
            if (SaveManager.HasAnySave())
            {
                UIManager.Instance.ShowPopup(
                    "新游戏",
                    "开始新游戏会使用一个新存档位，确定吗？",
                    onConfirm: () =>
                    {
                        PickFirstEmptySlotAndStart();
                    },
                    onCancel: () => { });
            }
            else
            {
                SaveManager.ActiveSlot = 0;
                GameManager.Instance.StartGame();
            }
        }

        private void PickFirstEmptySlotAndStart()
        {
            for (int i = 0; i < SaveManager.SlotCount; i++)
            {
                if (!SaveManager.HasSlot(i))
                {
                    SaveManager.ActiveSlot = i;
                    GameManager.Instance.StartGame();
                    return;
                }
            }
            UIManager.Instance.ShowPopup("存档位已满",
                "所有存档位都已使用。请先在「选择存档」中删除一个旧存档。");
        }
    }
}
