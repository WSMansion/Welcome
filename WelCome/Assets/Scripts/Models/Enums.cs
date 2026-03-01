using UnityEngine;

namespace WelCome.Models
{
    public enum EventType
    {
        Gather = 1,
        Intel = 2,
        Social = 3,
        MainStory = 4,
        Risk = 5
    }

    public enum ProductQuality
    {
        Normal = 1,
        Fine = 2,
        Excellent = 3
    }

    public enum CustomerState
    {
        Waiting,
        Served,
        Left
    }

    public static class NeedTagHelper
    {
        public static string GetName(int tag) => tag switch
        {
            1 => "照明",
            2 => "保温",
            3 => "清洁",
            4 => "省力",
            5 => "奢侈",
            6 => "农用",
            _ => "杂货"
        };

        public static string GetIcon(int tag) => tag switch
        {
            1 => "☀",
            2 => "♨",
            3 => "✦",
            4 => "⚙",
            5 => "♦",
            6 => "♣",
            _ => "●"
        };

        public static Color GetColor(int tag) => tag switch
        {
            1 => new Color(1f, 0.85f, 0.3f),
            2 => new Color(0.9f, 0.45f, 0.25f),
            3 => new Color(0.4f, 0.8f, 0.9f),
            4 => new Color(0.7f, 0.7f, 0.7f),
            5 => new Color(0.85f, 0.55f, 0.85f),
            6 => new Color(0.45f, 0.75f, 0.35f),
            _ => new Color(0.6f, 0.6f, 0.6f)
        };
    }
}
