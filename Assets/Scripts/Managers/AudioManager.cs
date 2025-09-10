using UnityEngine;

namespace XCAPE.Core
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        public void SetMasterVolume(float v)
        {
            AudioListener.volume = Mathf.Clamp01(v);
        }
        public void SetMusicVolume(float v)
        {
            if (musicSource) musicSource.volume = Mathf.Clamp01(v);
        }
        public void SetSFXVolume(float v)
        {
            if (sfxSource) sfxSource.volume = Mathf.Clamp01(v);
        }

        public void PlayMenuMusic() { }
        public void PlayGameplayMusic() { }
        public void PauseMusic() { if (musicSource) musicSource.Pause(); }
        public void ResumeMusic() { if (musicSource) musicSource.UnPause(); }
        public void PlayGameOverSound() { }
    }
}
