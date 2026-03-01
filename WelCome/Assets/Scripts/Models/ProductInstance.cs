using UnityEngine;

namespace WelCome.Models
{
    public class ProductInstance
    {
        public int RecipeId;
        public string Name;
        public int BasePrice;
        public ProductQuality Quality;
        public int NeedTag;
        public int DaysOld;

        public int SellingPrice
        {
            get
            {
                float qualityMul = Quality switch
                {
                    ProductQuality.Normal => 1f,
                    ProductQuality.Fine => 1.8f,
                    ProductQuality.Excellent => 3f,
                    _ => 1f
                };
                float freshness = Mathf.Max(0.3f, 1f - DaysOld * 0.15f);
                return Mathf.CeilToInt(BasePrice * qualityMul * freshness);
            }
        }
    }
}
