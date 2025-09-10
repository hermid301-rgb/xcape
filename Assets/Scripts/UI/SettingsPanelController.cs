using UnityEngine;
using UnityEngine.UI;
using XCAPE.Core;

namespace XCAPE.UI
{
    public class SettingsPanelController : MonoBehaviour
    {
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle performanceToggle;

        void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            if (masterSlider)
            {
                masterSlider.value = gm.GetMasterVolume();
                masterSlider.onValueChanged.AddListener(gm.SetMasterVolume);
            }
            if (musicSlider)
            {
                musicSlider.value = gm.GetMusicVolume();
                musicSlider.onValueChanged.AddListener(gm.SetMusicVolume);
            }
            if (sfxSlider)
            {
                sfxSlider.value = gm.GetSFXVolume();
                sfxSlider.onValueChanged.AddListener(gm.SetSFXVolume);
            }
            if (performanceToggle)
            {
                performanceToggle.isOn = false;
                performanceToggle.onValueChanged.AddListener(gm.SetPerformanceMode);
            }
        }
    }
}
