using UnityEngine;
using System;
using System.Collections;

#if UNITY_ANDROID || UNITY_IOS
// Note: AdMob SDK will be imported via Google Mobile Ads Unity Plugin
// Add via Window > Package Manager > Add package from git URL:
// https://github.com/googleads/googleads-mobile-unity.git
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif
#endif

namespace XCAPE.Core
{
    /// <summary>
    /// AdManager handles all monetization through Google AdMob
    /// Supports banner, interstitial, and rewarded ads
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        [Header("Ad Configuration")]
        [SerializeField] private bool enableAds = true;
        [SerializeField] private bool testMode = true;
        
        [Header("Ad Unit IDs (Production)")]
        [SerializeField] private string androidBannerAdUnitId = "ca-app-pub-3940256099942544/6300978111"; // Test ID
        [SerializeField] private string androidInterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712"; // Test ID
        [SerializeField] private string androidRewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; // Test ID
        
        [SerializeField] private string iosBannerAdUnitId = "ca-app-pub-3940256099942544/2934735716"; // Test ID
        [SerializeField] private string iosInterstitialAdUnitId = "ca-app-pub-3940256099942544/4411468910"; // Test ID
        [SerializeField] private string iosRewardedAdUnitId = "ca-app-pub-3940256099942544/1712485313"; // Test ID

        [Header("Ad Timing")]
        [SerializeField] private int interstitialFrequency = 3; // Show every 3 games
        [SerializeField] private float bannerShowDelay = 2f;

        public bool IsPremium { get; private set; }
        public bool IsInitialized { get; private set; }
        
        // Events
        public static event Action<bool> OnAdLoadResult;
        public static event Action OnInterstitialClosed;
        public static event Action<bool> OnRewardedAdResult;

        // Ad instances
#if UNITY_ANDROID || UNITY_IOS
#if GOOGLE_MOBILE_ADS
        private BannerView bannerView;
        private InterstitialAd interstitialAd;
        private RewardedAd rewardedAd;
#endif
#endif

        private int gamesPlayedSinceLastAd = 0;
        private Action pendingRewardCallback;

        private void Start()
        {
            // Check premium status (PlayerPrefs or IAP)
            IsPremium = PlayerPrefs.GetInt("IsPremium", 0) == 1;
            
            if (!enableAds || IsPremium)
            {
                Debug.Log("[AdManager] Ads disabled (premium or setting)");
                IsInitialized = true;
                return;
            }

            StartCoroutine(InitializeWithDelay());
        }

        private IEnumerator InitializeWithDelay()
        {
            yield return new WaitForSeconds(1f); // Let the game initialize first
            Initialize();
        }

        public void Initialize()
        {
            if (IsInitialized || !enableAds || IsPremium) return;

            Debug.Log("[AdManager] Initializing AdMob...");

#if UNITY_ANDROID || UNITY_IOS
#if GOOGLE_MOBILE_ADS
            // Initialize AdMob SDK
            MobileAds.Initialize(initStatus =>
            {
                Debug.Log($"[AdManager] AdMob initialized: {initStatus}");
                IsInitialized = true;
                
                // Configure test devices in development
                if (testMode)
                {
                    var configuration = new RequestConfiguration.Builder()
                        .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.False)
                        .SetTestDeviceIds(new[] { "YOUR_TEST_DEVICE_ID" })
                        .build();
                    MobileAds.SetRequestConfiguration(configuration);
                }

                LoadBannerAd();
                LoadInterstitialAd();
                LoadRewardedAd();
            });
#else
            Debug.LogWarning("[AdManager] Google Mobile Ads SDK not found. Install from Package Manager.");
            IsInitialized = true;
#endif
#else
            Debug.Log("[AdManager] Platform not supported for ads");
            IsInitialized = true;
#endif
        }

        #region Banner Ads

        public void ShowBannerAd()
        {
            if (!enableAds || IsPremium || !IsInitialized) return;

            StartCoroutine(ShowBannerWithDelay());
        }

        private IEnumerator ShowBannerWithDelay()
        {
            yield return new WaitForSeconds(bannerShowDelay);
            
#if UNITY_ANDROID || UNITY_IOS
#if GOOGLE_MOBILE_ADS
            LoadBannerAd();
#endif
#endif
        }

        public void HideBannerAd()
        {
#if UNITY_ANDROID || UNITY_IOS
#if GOOGLE_MOBILE_ADS
            bannerView?.Hide();
#endif
#endif
        }

        private void LoadBannerAd()
        {
#if UNITY_ANDROID || UNITY_IOS
#if GOOGLE_MOBILE_ADS
            // Clean up existing banner
            bannerView?.Destroy();

            string adUnitId = GetBannerAdUnitId();
            
            bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
            
            // Register for ad events
            bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("[AdManager] Banner ad loaded");
                OnAdLoadResult?.Invoke(true);
            };
            
            bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogError($"[AdManager] Banner ad failed to load: {error}");
                OnAdLoadResult?.Invoke(false);
            };

            // Load the ad
            var request = new AdRequest();
            bannerView.LoadAd(request);
#endif
#endif
        }

        #endregion

        #region Interstitial Ads

        public void ShowInterstitialAd()
        {
            if (!enableAds || IsPremium || !IsInitialized)
            {
                OnInterstitialClosed?.Invoke();
                return;
            }

            gamesPlayedSinceLastAd++;
            
            if (gamesPlayedSinceLastAd < interstitialFrequency)
            {
                OnInterstitialClosed?.Invoke();
                return;
            }

#if UNITY_ANDROID || UNITY_IOS
#if GOOGLE_MOBILE_ADS
            if (interstitialAd != null && interstitialAd.CanShowAd())
            {
                interstitialAd.Show();
                gamesPlayedSinceLastAd = 0;
            }
            else
            {
                Debug.Log("[AdManager] Interstitial ad not ready");
                LoadInterstitialAd(); // Try to load a new one
                OnInterstitialClosed?.Invoke();
            }
#else
            OnInterstitialClosed?.Invoke();
#endif
#else
            OnInterstitialClosed?.Invoke();
#endif
        }

        private void LoadInterstitialAd()
        {
#if UNITY_ANDROID || UNITY_IOS
#if GOOGLE_MOBILE_ADS
            // Clean up existing ad
            interstitialAd?.Destroy();

            string adUnitId = GetInterstitialAdUnitId();
            
            var request = new AdRequest();
            
            InterstitialAd.Load(adUnitId, request, (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError($"[AdManager] Interstitial ad failed to load: {error}");
                    return;
                }
                
                Debug.Log("[AdManager] Interstitial ad loaded");
                interstitialAd = ad;
                
                // Register for ad events
                interstitialAd.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("[AdManager] Interstitial ad closed");
                    OnInterstitialClosed?.Invoke();
                    LoadInterstitialAd(); // Preload next ad
                };
                
                interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    Debug.LogError($"[AdManager] Interstitial ad failed to show: {error}");
                    OnInterstitialClosed?.Invoke();
                };
            });
#endif
#endif
        }

        #endregion

        #region Rewarded Ads

        public void ShowRewardedAd(Action<bool> onRewardCallback)
        {
            if (!enableAds || !IsInitialized)
            {
                onRewardCallback?.Invoke(IsPremium); // Premium users always get rewards
                return;
            }

            if (IsPremium)
            {
                onRewardCallback?.Invoke(true);
                return;
            }

            pendingRewardCallback = () => onRewardCallback?.Invoke(true);

#if UNITY_ANDROID || UNITY_IOS
#if GOOGLE_MOBILE_ADS
            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                rewardedAd.Show((Reward reward) =>
                {
                    Debug.Log($"[AdManager] User earned reward: {reward.Amount} {reward.Type}");
                    pendingRewardCallback?.Invoke();
                    pendingRewardCallback = null;
                    OnRewardedAdResult?.Invoke(true);
                });
            }
            else
            {
                Debug.Log("[AdManager] Rewarded ad not ready");
                LoadRewardedAd(); // Try to load a new one
                onRewardCallback?.Invoke(false);
                OnRewardedAdResult?.Invoke(false);
            }
#else
            onRewardCallback?.Invoke(false);
            OnRewardedAdResult?.Invoke(false);
#endif
#else
            onRewardCallback?.Invoke(false);
            OnRewardedAdResult?.Invoke(false);
#endif
        }

        private void LoadRewardedAd()
        {
#if UNITY_ANDROID || UNITY_IOS
#if GOOGLE_MOBILE_ADS
            // Clean up existing ad
            rewardedAd?.Destroy();

            string adUnitId = GetRewardedAdUnitId();
            
            var request = new AdRequest();
            
            RewardedAd.Load(adUnitId, request, (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError($"[AdManager] Rewarded ad failed to load: {error}");
                    return;
                }
                
                Debug.Log("[AdManager] Rewarded ad loaded");
                rewardedAd = ad;
                
                // Register for ad events
                rewardedAd.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("[AdManager] Rewarded ad closed");
                    LoadRewardedAd(); // Preload next ad
                };
                
                rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    Debug.LogError($"[AdManager] Rewarded ad failed to show: {error}");
                    pendingRewardCallback = null;
                    OnRewardedAdResult?.Invoke(false);
                };
            });
#endif
#endif
        }

        #endregion

        #region Utility Methods

        private string GetBannerAdUnitId()
        {
#if UNITY_ANDROID
            return androidBannerAdUnitId;
#elif UNITY_IOS
            return iosBannerAdUnitId;
#else
            return "";
#endif
        }

        private string GetInterstitialAdUnitId()
        {
#if UNITY_ANDROID
            return androidInterstitialAdUnitId;
#elif UNITY_IOS
            return iosInterstitialAdUnitId;
#else
            return "";
#endif
        }

        private string GetRewardedAdUnitId()
        {
#if UNITY_ANDROID
            return androidRewardedAdUnitId;
#elif UNITY_IOS
            return iosRewardedAdUnitId;
#else
            return "";
#endif
        }

        public void SetPremiumStatus(bool isPremium)
        {
            IsPremium = isPremium;
            PlayerPrefs.SetInt("IsPremium", isPremium ? 1 : 0);
            PlayerPrefs.Save();
            
            if (isPremium)
            {
                HideBannerAd();
                Debug.Log("[AdManager] Premium activated - ads disabled");
            }
        }

        public bool IsRewardedAdReady()
        {
            if (!enableAds || IsPremium) return true; // Premium users always have "rewards"
            
#if UNITY_ANDROID || UNITY_IOS
#if GOOGLE_MOBILE_ADS
            return rewardedAd != null && rewardedAd.CanShowAd();
#endif
#endif
            return false;
        }

        #endregion

        #region Unity Lifecycle

        private void OnApplicationPause(bool pauseStatus)
        {
            // AdMob recommends pausing ads when app is backgrounded
            if (pauseStatus)
            {
                HideBannerAd();
            }
            else if (IsInitialized && !IsPremium)
            {
                ShowBannerAd();
            }
        }

        private void OnDestroy()
        {
#if UNITY_ANDROID || UNITY_IOS
#if GOOGLE_MOBILE_ADS
            bannerView?.Destroy();
            interstitialAd?.Destroy();
            rewardedAd?.Destroy();
#endif
#endif
        }

        #endregion
    }
}
