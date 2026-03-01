using UnityEngine;
using UnityEngine.UI;
using WelCome.Models;
using WelCome.Systems;

namespace WelCome.UI
{
    public class AVGPanel : UIPanel
    {
        private Text _speakerText;
        private Text _dialogueText;

        protected override void BuildUI()
        {
            UIHelper.CreatePanel("BG", transform, new Color(0, 0, 0, 0.7f));

            var dialogueBox = UIHelper.CreatePanel("DialogueBox", transform, UIHelper.BgPanel);
            dialogueBox.GetComponent<RectTransform>().Set(0.05f, 0.05f, 0.95f, 0.35f);

            _speakerText = UIHelper.CreateText("Speaker", dialogueBox.transform,
                "", 24, TextAnchor.UpperLeft, UIHelper.AccentGold);
            _speakerText.GetComponent<RectTransform>().Set(0.03f, 0.75f, 0.5f, 0.95f);

            _dialogueText = UIHelper.CreateText("Dialogue", dialogueBox.transform,
                "", 20, TextAnchor.UpperLeft, UIHelper.TextWhite);
            _dialogueText.GetComponent<RectTransform>().Set(0.03f, 0.05f, 0.97f, 0.72f);

            var clickArea = UIHelper.CreatePanel("ClickArea", transform, new Color(0, 0, 0, 0),
                blockRaycast: true);
            var btn = clickArea.gameObject.AddComponent<Button>();
            btn.targetGraphic = clickArea;
            btn.onClick.AddListener(() => AVGSystem.Instance.Advance());
        }

        protected override void OnShow()
        {
            AVGSystem.Instance.OnLineDisplayed += OnLine;
            AVGSystem.Instance.OnDialogueEnded += OnEnd;
        }

        protected override void OnHide()
        {
            if (AVGSystem.Instance != null)
            {
                AVGSystem.Instance.OnLineDisplayed -= OnLine;
                AVGSystem.Instance.OnDialogueEnded -= OnEnd;
            }
        }

        private void OnLine(AVGDialogueLine line)
        {
            _speakerText.text = string.IsNullOrEmpty(line.Speaker) ? "" : line.Speaker;
            _dialogueText.text = line.Text;
        }

        private void OnEnd()
        {
            UIManager.Instance.HideOverlay<AVGPanel>();
        }
    }
}
