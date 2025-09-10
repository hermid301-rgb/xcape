using UnityEngine;

namespace XCAPE.Core
{
    public class AdManager : MonoBehaviour
    {
        public bool IsPremium { get; private set; }

        public void Initialize()
        {
            Debug.Log("AdManager initialized (stub) - integrate AdMob SDK later");
        }

        public void ShowInterstitialAd()
        {
            if (IsPremium) return;
            Debug.Log("Show Interstitial (stub)");
        }

        public void ShowBanner(bool show)
        {
            if (IsPremium) return;
            Debug.Log($"Banner visible: {show}");
        }

        public void ShowRewarded(System.Action onReward)
        {
            if (IsPremium) { onReward?.Invoke(); return; }
            Debug.Log("Show Rewarded (stub)");
            onReward?.Invoke();
        }
    }
}
