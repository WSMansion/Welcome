using System.Globalization;
using UnityEngine;

namespace WelCome.World
{
    public static class ShopPurchaseTuning
    {
        public static float LookMinSeconds = 3f;
        public static float LookMaxSeconds = 5f;
        public static float OfferSeconds = 5f;
        public static float ReactionSeconds = 2f;
        public static int InterestChance = 40;
        public static int TooExpensiveChance = 30;
        public static int LookAgainChance = 20;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Load()
        {
            TextAsset asset = Resources.Load<TextAsset>("Luban/Output/Json/tbshopbehavior");
            if (asset == null)
                return;
            string json = asset.text;
            LookMinSeconds = Read(json, "browse_look_min_seconds", LookMinSeconds);
            LookMaxSeconds = Read(json, "browse_look_max_seconds", LookMaxSeconds);
            OfferSeconds = Read(json, "purchase_offer_seconds", OfferSeconds);
            ReactionSeconds = Read(json, "purchase_reaction_seconds", ReactionSeconds);
            InterestChance = Mathf.RoundToInt(Read(json, "purchase_interest_chance", InterestChance));
            TooExpensiveChance = Mathf.RoundToInt(Read(json, "purchase_too_expensive_chance", TooExpensiveChance));
            LookAgainChance = Mathf.RoundToInt(Read(json, "purchase_look_again_chance", LookAgainChance));
        }

        public static float LookDuration()
        {
            float low = Mathf.Max(0.2f, Mathf.Min(LookMinSeconds, LookMaxSeconds));
            float high = Mathf.Max(low, Mathf.Max(LookMinSeconds, LookMaxSeconds));
            return Random.Range(low, high);
        }

        private static float Read(string json, string key, float fallback)
        {
            string token = "\"" + key + "\"";
            int index = json.IndexOf(token, System.StringComparison.Ordinal);
            if (index < 0)
                return fallback;
            int colon = json.IndexOf(':', index + token.Length);
            if (colon < 0)
                return fallback;
            int start = colon + 1;
            while (start < json.Length && char.IsWhiteSpace(json[start]))
                start++;
            int end = start;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '.' || json[end] == '-'))
                end++;
            if (end <= start)
                return fallback;
            if (float.TryParse(json.Substring(start, end - start), NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                return value;
            return fallback;
        }
    }
}
