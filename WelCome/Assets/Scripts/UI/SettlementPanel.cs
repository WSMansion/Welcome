using UnityEngine;
using UnityEngine.UI;
using WelCome.Core;
using WelCome.Models;
using WelCome.Systems;

namespace WelCome.UI
{
    public class SettlementPanel : UIPanel
    {
        private Text _titleText;
        private Text _summaryText;

        protected override void BuildUI()
        {
            UIHelper.CreatePanel("BG", transform, UIHelper.BgDark);

            _titleText = UIHelper.CreateText("Title", transform,
                "", 36, TextAnchor.MiddleCenter, UIHelper.AccentGold);
            _titleText.GetComponent<RectTransform>().Set(0.1f, 0.78f, 0.9f, 0.88f);

            _summaryText = UIHelper.CreateText("Summary", transform,
                "", 26, TextAnchor.MiddleCenter, UIHelper.TextWhite);
            _summaryText.GetComponent<RectTransform>().Set(0.15f, 0.35f, 0.85f, 0.75f);

            UIHelper.CreateButton("NextDayBtn", transform, "进入下一天",
                () => GameManager.Instance.AdvanceToNextDay(), UIHelper.AccentGold)
                .GetComponent<RectTransform>().Set(0.35f, 0.15f, 0.65f, 0.25f);
        }

        protected override void OnShow()
        {
            var result = SettlementSystem.Instance.CalculateSettlement();
            _titleText.text = $"第 {result.DayNumber} 天 - 结算";
            _summaryText.text =
                $"今日营收：<color=#EBCA6A>{result.GoldEarned} G</color>\n\n" +
                $"服务顾客：{result.CustomersServed} 人\n\n" +
                $"当前金币：{result.GoldAfter} G\n\n" +
                $"关注度：{result.AttentionChange}    民心：{result.SupportChange}";
        }
    }
}
