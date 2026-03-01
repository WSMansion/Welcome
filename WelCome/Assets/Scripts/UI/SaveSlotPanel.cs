using System;
using UnityEngine;
using UnityEngine.UI;
using WelCome.Core;

namespace WelCome.UI
{
    public enum SaveSlotMode { Save, Load }

    public class SaveSlotPanel : UIPanel
    {
        private Text _titleText;
        private Transform _slotContainer;
        private SaveSlotMode _mode;
        private Action _onDone;

        private readonly SlotUI[] _slots = new SlotUI[SaveManager.SlotCount];

        struct SlotUI
        {
            public GameObject root;
            public Text summary;
            public GameObject actionBtn;
            public Text actionLabel;
            public GameObject deleteBtn;
        }

        public void SetMode(SaveSlotMode mode, Action onDone = null)
        {
            _mode = mode;
            _onDone = onDone;
        }

        protected override void BuildUI()
        {
            UIHelper.CreatePanel("Dim", transform, new Color(0, 0, 0, 0.65f), blockRaycast: true);

            var box = UIHelper.CreatePanel("Box", transform, new Color(0.14f, 0.13f, 0.18f, 0.98f));
            box.GetComponent<RectTransform>().Set(0.2f, 0.1f, 0.8f, 0.9f);

            var border = UIHelper.CreatePanel("Border", box.transform,
                new Color(0.92f, 0.78f, 0.42f, 0.2f));
            border.GetComponent<RectTransform>().Set(0.01f, 0.01f, 0.99f, 0.99f);

            _titleText = UIHelper.CreateText("Title", box.transform, "存档管理",
                30, TextAnchor.MiddleCenter, UIHelper.AccentGold);
            _titleText.GetComponent<RectTransform>().Set(0.1f, 0.89f, 0.9f, 0.97f);
            _titleText.fontStyle = FontStyle.Bold;

            var divider = UIHelper.CreatePanel("Div", box.transform,
                new Color(0.92f, 0.78f, 0.42f, 0.25f));
            divider.GetComponent<RectTransform>().Set(0.08f, 0.88f, 0.92f, 0.885f);

            _slotContainer = box.transform;

            for (int i = 0; i < SaveManager.SlotCount; i++)
                BuildSlotRow(i);

            UIHelper.CreateButton("CloseBtn", box.transform, "返回",
                OnClose, new Color(0.4f, 0.38f, 0.35f), 20)
                .GetComponent<RectTransform>().Set(0.3f, 0.03f, 0.7f, 0.1f);
        }

        private void BuildSlotRow(int idx)
        {
            float top = 0.84f - idx * 0.26f;
            float bottom = top - 0.22f;

            var card = UIHelper.CreatePanel($"Slot{idx}", _slotContainer,
                new Color(0.18f, 0.17f, 0.22f, 0.95f));
            card.GetComponent<RectTransform>().Set(0.05f, bottom, 0.95f, top);

            var slotLabel = UIHelper.CreateText($"Label{idx}", card.transform,
                $"存档 {idx + 1}", 22, TextAnchor.MiddleLeft, UIHelper.AccentGold);
            slotLabel.GetComponent<RectTransform>().Set(0.03f, 0.6f, 0.3f, 0.95f);
            slotLabel.fontStyle = FontStyle.Bold;

            var summary = UIHelper.CreateText($"Info{idx}", card.transform, "",
                17, TextAnchor.MiddleLeft, UIHelper.TextGray);
            summary.GetComponent<RectTransform>().Set(0.03f, 0.1f, 0.6f, 0.6f);

            int slot = idx;
            var actionBtn = UIHelper.CreateButton($"Action{idx}", card.transform, "",
                () => OnSlotAction(slot), UIHelper.AccentGold, 18);
            actionBtn.GetComponent<RectTransform>().Set(0.62f, 0.25f, 0.82f, 0.75f);
            var actionLabel = actionBtn.GetComponentInChildren<Text>();

            var deleteBtn = UIHelper.CreateButton($"Del{idx}", card.transform, "删除",
                () => OnDeleteSlot(slot), new Color(0.6f, 0.3f, 0.25f), 16);
            deleteBtn.GetComponent<RectTransform>().Set(0.84f, 0.25f, 0.97f, 0.75f);

            _slots[idx] = new SlotUI
            {
                root = card.gameObject,
                summary = summary,
                actionBtn = actionBtn.gameObject,
                actionLabel = actionLabel,
                deleteBtn = deleteBtn.gameObject
            };
        }

        protected override void OnShow()
        {
            _titleText.text = _mode == SaveSlotMode.Save ? "选择存档位 — 保存" : "选择存档位 — 读取";

            for (int i = 0; i < SaveManager.SlotCount; i++)
                RefreshSlot(i);
        }

        private void RefreshSlot(int idx)
        {
            bool exists = SaveManager.HasSlot(idx);
            bool isActive = SaveManager.ActiveSlot == idx;
            var s = _slots[idx];

            if (exists)
            {
                string info = SaveManager.GetSlotSummary(idx) ?? "存档数据异常";
                s.summary.text = isActive ? $"{info}\n<color=#88DD88>◆ 当前存档</color>" : info;
                s.summary.color = UIHelper.TextWhite;
            }
            else
            {
                s.summary.text = "—— 空 ——";
                s.summary.color = UIHelper.TextGray;
            }

            if (_mode == SaveSlotMode.Save)
            {
                s.actionLabel.text = exists ? "覆盖" : "保存";
                s.actionBtn.SetActive(true);
                s.deleteBtn.SetActive(exists);
            }
            else
            {
                s.actionLabel.text = "读取";
                s.actionBtn.SetActive(exists);
                s.deleteBtn.SetActive(exists);
            }
        }

        private void OnSlotAction(int slot)
        {
            if (_mode == SaveSlotMode.Save)
            {
                if (SaveManager.HasSlot(slot))
                {
                    UIManager.Instance.ShowPopup("覆盖确认",
                        $"确定要覆盖存档 {slot + 1} 吗？",
                        onConfirm: () => { DoSave(slot); },
                        onCancel: () => { });
                }
                else
                {
                    DoSave(slot);
                }
            }
            else
            {
                DoLoad(slot);
            }
        }

        private void DoSave(int slot)
        {
            SaveManager.Save(slot);
            RefreshSlot(slot);
            for (int i = 0; i < SaveManager.SlotCount; i++) RefreshSlot(i);
            UIManager.Instance.ShowPopup("保存成功", $"已保存到存档 {slot + 1}。");
        }

        private void DoLoad(int slot)
        {
            UIManager.Instance.HideOverlay<SaveSlotPanel>();
            _onDone?.Invoke();
            if (!SaveManager.Load(slot))
                UIManager.Instance.ShowPopup("读档失败", "存档文件损坏或不兼容。");
        }

        private void OnDeleteSlot(int slot)
        {
            UIManager.Instance.ShowPopup("删除确认",
                $"确定要删除存档 {slot + 1} 吗？此操作不可恢复。",
                onConfirm: () =>
                {
                    SaveManager.DeleteSlot(slot);
                    RefreshSlot(slot);
                },
                onCancel: () => { });
        }

        private void OnClose()
        {
            UIManager.Instance.HideOverlay<SaveSlotPanel>();
            _onDone?.Invoke();
        }
    }
}
