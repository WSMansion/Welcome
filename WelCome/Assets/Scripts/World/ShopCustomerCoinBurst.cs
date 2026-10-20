using UnityEngine;
using WelCome.UI;

namespace WelCome.World
{
    public sealed class ShopCustomerCoinBurst : MonoBehaviour
    {
        private static AudioClip bling;
        private AudioSource source;

        public void Play()
        {
            if (source == null)
            {
                source = gameObject.GetComponent<AudioSource>();
                if (source == null)
                    source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
            }
            source.PlayOneShot(GetBling());
            StopAllCoroutines();
            StartCoroutine(Burst());
        }

        private System.Collections.IEnumerator Burst()
        {
            Camera camera = Camera.main;
            Vector3 origin = transform.position + Vector3.up * 1.15f;
            const int count = 4;
            var coins = new SpriteRenderer[count];
            var scattered = new Vector3[count];
            Sprite sprite = ShopGoldBar.Instance != null ? ShopGoldBar.Instance.CoinSprite : null;
            if (sprite == null)
                sprite = FallbackCoin();
            float baseScale = sprite.bounds.size.y > 0.01f ? 0.42f / sprite.bounds.size.y : 1f;
            if (ShopGoldBar.Instance != null)
                ShopGoldBar.Instance.ExpectCoins(count);

            for (int i = 0; i < count; i++)
            {
                float angle = (-55f + i * 36f) * Mathf.Deg2Rad;
                scattered[i] = origin + new Vector3(Mathf.Sin(angle) * 0.42f, 0.18f + Mathf.Cos(angle) * 0.28f, 0f);
                GameObject coin = new GameObject("金币");
                coin.transform.position = origin;
                coin.transform.localScale = Vector3.zero;
                SpriteRenderer renderer = coin.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingLayerID = SortingLayer.NameToID("HUD");
                renderer.sortingOrder = 1400;
                coins[i] = renderer;
            }

            yield return StartCoroutine(Spread(coins, origin, scattered, baseScale, 0.32f));
            yield return new WaitForSeconds(0.08f);

            bool[] arrived = new bool[count];
            float elapsed = 0f;
            const float launchGap = 0.1f;
            const float flight = 0.4f;
            while (true)
            {
                elapsed += Time.deltaTime;
                bool pending = false;
                for (int i = 0; i < count; i++)
                {
                    if (arrived[i] || coins[i] == null)
                        continue;
                    if (elapsed < i * launchGap)
                    {
                        pending = true;
                        continue;
                    }

                    float fly = Mathf.Clamp01((elapsed - i * launchGap) / flight);
                    float eased = fly * fly * (3f - 2f * fly);
                    Vector3 target = scattered[i] + Vector3.up * 0.9f;
                    if (ShopGoldBar.Instance != null && camera != null
                        && ShopGoldBar.Instance.TryGetFlyWorldPoint(camera, out Vector3 barPoint))
                        target = barPoint;
                    Vector3 arc = Vector3.up * Mathf.Sin(fly * Mathf.PI) * 0.22f;
                    coins[i].transform.position = Vector3.Lerp(scattered[i], target, eased) + arc;
                    float scale = baseScale * Mathf.Lerp(1f, 0.4f, eased);
                    coins[i].transform.localScale = new Vector3(scale, scale, 1f);
                    if (fly < 1f)
                    {
                        pending = true;
                        continue;
                    }

                    arrived[i] = true;
                    Destroy(coins[i].gameObject);
                    coins[i] = null;
                    if (ShopGoldBar.Instance != null)
                        ShopGoldBar.Instance.ReceiveCoin();
                }

                if (!pending)
                    break;
                yield return null;
            }
        }

        private static System.Collections.IEnumerator Spread(SpriteRenderer[] coins, Vector3 origin, Vector3[] scattered, float baseScale, float duration)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float spread = Mathf.Clamp01(time / duration);
                float eased = 1f - (1f - spread) * (1f - spread);
                for (int i = 0; i < coins.Length; i++)
                {
                    if (coins[i] == null)
                        continue;
                    coins[i].transform.position = Vector3.Lerp(origin, scattered[i], eased);
                    float scale = baseScale * Mathf.Lerp(0.2f, 1f, eased);
                    coins[i].transform.localScale = new Vector3(scale, scale, 1f);
                }
                yield return null;
            }
        }

        private static Sprite fallbackCoin;
        private static Sprite FallbackCoin()
        {
            if (fallbackCoin != null)
                return fallbackCoin;
            Texture2D texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    float dx = x - 3.5f;
                    float dy = y - 3.5f;
                    bool inside = dx * dx + dy * dy <= 10f;
                    texture.SetPixel(x, y, inside ? Color.white : Color.clear);
                }
            }
            texture.Apply();
            fallbackCoin = Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 16f);
            return fallbackCoin;
        }

        private static AudioClip GetBling()
        {
            if (bling != null)
                return bling;
            const int rate = 44100;
            int length = Mathf.RoundToInt(rate * 0.28f);
            float[] samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float time = i / (float)rate;
                float envelope = Mathf.Exp(-time * 10f);
                float tone = Mathf.Sin(time * 988f * Mathf.PI * 2f) * 0.35f;
                if (time > 0.08f)
                    tone += Mathf.Sin((time - 0.08f) * 1480f * Mathf.PI * 2f) * 0.28f * Mathf.Exp(-(time - 0.08f) * 9f);
                samples[i] = tone * envelope;
            }
            bling = AudioClip.Create("coin-bling", length, 1, rate, false);
            bling.SetData(samples, 0);
            return bling;
        }
    }
}
