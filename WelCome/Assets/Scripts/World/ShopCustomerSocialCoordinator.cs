using System.Collections.Generic;
using UnityEngine;

namespace WelCome.World
{
    /// <summary>
    /// Finds nearby visitors who can greet or chat without blocking the doorway or service counter.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShopCustomerSocialCoordinator : MonoBehaviour
    {
        private readonly List<ShopCustomerWanderAI> customers = new List<ShopCustomerWanderAI>();
        private cfg.ShopBehavior settings;
        private float nextCheckTime;

        public void Initialize(cfg.ShopBehavior behaviorSettings)
        {
            if (settings == null)
                settings = behaviorSettings;
        }

        public void Register(ShopCustomerWanderAI customer)
        {
            if (customer != null && !customers.Contains(customer))
                customers.Add(customer);
        }

        public void Unregister(ShopCustomerWanderAI customer)
        {
            customers.Remove(customer);
        }

        private void Update()
        {
            if (settings == null || Time.time < nextCheckTime)
                return;

            nextCheckTime = Time.time + Mathf.Max(0.1f, settings.SocialCheckInterval);
            TryStartNearbyInteraction();
        }

        private void TryStartNearbyInteraction()
        {
            for (int firstIndex = 0; firstIndex < customers.Count; firstIndex++)
            {
                ShopCustomerWanderAI first = customers[firstIndex];
                if (first == null || !first.IsAtSafeSocialPosition(settings.DoorKeepoutRadius))
                    continue;

                for (int secondIndex = firstIndex + 1; secondIndex < customers.Count; secondIndex++)
                {
                    ShopCustomerWanderAI second = customers[secondIndex];
                    if (second == null || !second.IsAtSafeSocialPosition(settings.DoorKeepoutRadius)
                        || !first.IsInSameSocialArea(second))
                        continue;

                    float distance = Vector2.Distance(first.transform.position, second.transform.position);
                    bool familiar = first.IsAcquaintedWith(second);
                    if (familiar && distance <= settings.GreetingRecognitionRadius
                        && TryChance(settings.GreetingChance, Mathf.Min(first.Sociability, second.Sociability)))
                    {
                        if (TryBuildMeetingTargets(first, second, distance, out Vector2 firstTarget,
                                out Vector2 secondTarget, out bool approachBeforeChat)
                            && StartPairInteraction(first, second, 0f, 0f, true,
                                firstTarget, secondTarget, approachBeforeChat,
                                settings.GreetingApproachSpeedMultiplier, settings.GreetingApproachMaxSeconds))
                            return;
                    }

                    if (first.IsPausedAtDestination && second.IsPausedAtDestination
                        && distance <= settings.ConversationRadius
                        && TryChance(settings.ConversationChance, Mathf.Min(first.Sociability, second.Sociability)))
                    {
                        StartPairInteraction(first, second, settings.ConversationMinSeconds,
                            settings.ConversationMaxSeconds, false,
                            first.transform.position, second.transform.position, false, 1f, 0.2f);
                        return;
                    }
                }
            }
        }

        private static bool TryChance(int chancePercent, float socialFactor)
        {
            float chance = Mathf.Clamp01(chancePercent / 100f) * Mathf.Clamp01(socialFactor);
            return Random.value < chance;
        }

        private bool TryBuildMeetingTargets(ShopCustomerWanderAI first, ShopCustomerWanderAI second,
            float distance, out Vector2 firstTarget, out Vector2 secondTarget, out bool approachBeforeChat)
        {
            Vector2 firstPosition = first.transform.position;
            Vector2 secondPosition = second.transform.position;
            approachBeforeChat = distance > settings.GreetingRadius * 0.85f;
            if (!approachBeforeChat)
            {
                firstTarget = firstPosition;
                secondTarget = secondPosition;
                return true;
            }

            Vector2 axis = (secondPosition - firstPosition).normalized;
            if (axis.sqrMagnitude <= 0.0001f)
                axis = Vector2.right;
            Vector2 midpoint = (firstPosition + secondPosition) * 0.5f;
            float halfSeparation = Mathf.Min(0.35f, Mathf.Max(0.2f, settings.GreetingRadius * 0.35f));
            firstTarget = midpoint - axis * halfSeparation;
            secondTarget = midpoint + axis * halfSeparation;

            return first.CanReachSocialMeetingPoint(firstTarget, settings.DoorKeepoutRadius)
                && second.CanReachSocialMeetingPoint(secondTarget, settings.DoorKeepoutRadius);
        }

        private static bool StartPairInteraction(ShopCustomerWanderAI first, ShopCustomerWanderAI second,
            float minimumDuration, float maximumDuration, bool greeting,
            Vector2 firstTarget, Vector2 secondTarget, bool approachBeforeChat,
            float approachSpeedMultiplier, float approachMaxSeconds)
        {
            float low = Mathf.Max(0.2f, Mathf.Min(minimumDuration, maximumDuration));
            float high = Mathf.Max(low, Mathf.Max(minimumDuration, maximumDuration));
            float duration = Random.Range(low, high);
            if (!first.TryStartSocialInteraction(second, duration, greeting, false, firstTarget,
                    approachBeforeChat, approachSpeedMultiplier, approachMaxSeconds))
                return false;

            if (second.TryStartSocialInteraction(first, duration, greeting, false, secondTarget,
                    approachBeforeChat, approachSpeedMultiplier, approachMaxSeconds))
                return true;

            first.CancelSocialInteractionFromPartner(second);
            return false;
        }
    }
}
