using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using WelCome.Core;
using WelCome.Systems;

namespace WelCome.UI
{
    public sealed class ShopGoldBar : MonoBehaviour
    {
        public static ShopGoldBar Instance { get; private set; }

        [SerializeField] private Text amountLabel;
        [SerializeField] private RectTransform flyTarget;
        [SerializeField] private Sprite coinSprite;

        private int shownAmount;
        private int unshownGain;
        private int coinsLeft;
        private bool flightArmed;
        private bool holding;
        private float punch;
        private Vector3 iconBaseScale = Vector3.one;

        public Sprite CoinSprite => coinSprite;
        public RectTransform FlyTarget => flyTarget != null ? flyTarget : transform as RectTransform;

        private void OnEnable()
        {
            Instance = this;
            iconBaseScale = FlyTarget != null ? FlyTarget.localScale : Vector3.one;
            EventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
            shownAmount = ResourceSystem.Instance != null ? ResourceSystem.Instance.Gold : 0;
            unshownGain = 0;
            ShowAmount(shownAmount);
        }

        private void OnDisable()
        {
            if (Instance == this)
                Instance = null;
            EventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
        }

        private void Update()
        {
            if (FlyTarget == null)
                return;
            if (punch <= 0f)
            {
                FlyTarget.localScale = iconBaseScale;
                return;
            }

            punch -= Time.unscaledDeltaTime / 0.16f;
            float t = 1f - Mathf.Clamp01(punch);
            float hop = Mathf.Sin(t * Mathf.PI);
            FlyTarget.localScale = iconBaseScale * (1f + hop * 0.38f);
        }

        public bool TryGetFlyWorldPoint(Camera camera, out Vector3 worldPoint)
        {
            worldPoint = default;
            RectTransform target = FlyTarget;
            if (camera == null || target == null)
                return false;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                worldPoint = target.position;
                worldPoint.z = 0f;
                return true;
            }

            Vector3 screen = target.position;
            screen.z = Mathf.Abs(camera.transform.position.z);
            worldPoint = camera.ScreenToWorldPoint(screen);
            worldPoint.z = 0f;
            return true;
        }

        public void ExpectCoins(int count)
        {
            flightArmed = true;
            coinsLeft += Mathf.Max(1, count);
        }

        public void ReceiveCoin()
        {
            if (!flightArmed || coinsLeft <= 0)
            {
                SnapPending();
                return;
            }

            int step = coinsLeft == 1 ? unshownGain : Mathf.CeilToInt(unshownGain / (float)coinsLeft);
            step = Mathf.Clamp(step, 0, unshownGain);
            unshownGain -= step;
            coinsLeft--;
            shownAmount += step;
            ShowAmount(shownAmount);
            punch = 1f;
            if (coinsLeft == 0)
                flightArmed = false;
        }

        private void OnGoldChanged(GoldChangedEvent change)
        {
            if (change.Delta <= 0)
            {
                shownAmount = change.Current;
                unshownGain = 0;
                ShowAmount(shownAmount);
                return;
            }

            unshownGain += change.Delta;
            if (!holding)
                StartCoroutine(ReleaseIfUnclaimed());
        }

        private IEnumerator ReleaseIfUnclaimed()
        {
            holding = true;
            yield return null;
            if (!flightArmed)
                SnapPending();
            holding = false;
        }

        private void SnapPending()
        {
            shownAmount += unshownGain;
            unshownGain = 0;
            coinsLeft = 0;
            flightArmed = false;
            ShowAmount(shownAmount);
        }

        private void ShowAmount(int gold)
        {
            if (amountLabel != null)
                amountLabel.text = gold + "金";
        }
    }
}