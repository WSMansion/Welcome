using System;
using UnityEngine;
using UnityEngine.UI;

namespace WelCome.UI
{
    public class PopupPanel : UIPanel
    {
        private Text _titleText;
        private Text _bodyText;
        private Action _onConfirm;
        private Action _onCancel;
        private GameObject _cancelBtnGo;

        private string _pendingTitle;
        private string _pendingBody;

        protected override void BuildUI()
        {
            UIHelper.CreatePanel("Dim", transform, new Color(0, 0, 0, 0.6f), blockRaycast: true);

            var box = UIHelper.CreatePanel("Box", transform, new Color(0.16f, 0.14f, 0.2f, 0.97f));
            box.GetComponent<RectTransform>().Set(0.15f, 0.15f, 0.85f, 0.85f);

            var border = UIHelper.CreatePanel("Border", box.transform,
                new Color(0.92f, 0.78f, 0.42f, 0.25f));
            border.GetComponent<RectTransform>().Set(0.01f, 0.01f, 0.99f, 0.99f);

            _titleText = UIHelper.CreateText("Title", box.transform, "",
                28, TextAnchor.MiddleCenter, UIHelper.AccentGold);
            _titleText.GetComponent<RectTransform>().Set(0.05f, 0.85f, 0.95f, 0.95f);
            _titleText.fontStyle = FontStyle.Bold;

            var divider = UIHelper.CreatePanel("Divider", box.transform,
                new Color(0.92f, 0.78f, 0.42f, 0.3f));
            divider.GetComponent<RectTransform>().Set(0.08f, 0.835f, 0.92f, 0.84f);

            var scroll = UIHelper.CreateScrollView("BodyScroll", box.transform);
            scroll.GetComponent<RectTransform>().Set(0.05f, 0.16f, 0.95f, 0.83f);

            var layout = scroll.content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 8, 8);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            scroll.content.gameObject.AddComponent<ContentSizeFitter>()
                .verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _bodyText = UIHelper.CreateText("Body", scroll.content, "",
                20, TextAnchor.UpperLeft, UIHelper.TextWhite);

            UIHelper.CreateButton("OkBtn", box.transform, "确定", OnOkClicked,
                UIHelper.AccentGold, 22)
                .GetComponent<RectTransform>().Set(0.52f, 0.04f, 0.85f, 0.13f);

            _cancelBtnGo = UIHelper.CreateButton("CancelBtn", box.transform, "取消",
                OnCancelClicked, new Color(0.45f, 0.4f, 0.35f), 22).gameObject;
            _cancelBtnGo.GetComponent<RectTransform>().Set(0.15f, 0.04f, 0.48f, 0.13f);
        }

        protected override void OnShow()
        {
            if (_titleText != null) _titleText.text = _pendingTitle ?? "";
            if (_bodyText != null) _bodyText.text = _pendingBody ?? "";
            _cancelBtnGo.SetActive(_onCancel != null);
        }

        public void SetContent(string title, string body, Action onConfirm = null, Action onCancel = null)
        {
            _pendingTitle = title;
            _pendingBody = body;
            _onConfirm = onConfirm;
            _onCancel = onCancel;

            if (_titleText != null) _titleText.text = title;
            if (_bodyText != null) _bodyText.text = body;
            if (_cancelBtnGo != null) _cancelBtnGo.SetActive(onCancel != null);
        }

        private void OnOkClicked()
        {
            var callback = _onConfirm;
            _onConfirm = null;
            _onCancel = null;
            UIManager.Instance.HideOverlay<PopupPanel>();
            callback?.Invoke();
        }

        private void OnCancelClicked()
        {
            var callback = _onCancel;
            _onConfirm = null;
            _onCancel = null;
            UIManager.Instance.HideOverlay<PopupPanel>();
            callback?.Invoke();
        }
    }
}
