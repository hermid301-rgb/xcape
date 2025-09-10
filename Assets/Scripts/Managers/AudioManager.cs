using UnityEngine;

namespace XCAPE.Core
{
    public class AudioManager : MonoBehaviour
    {
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [Header("Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip strikeSFX;
    [SerializeField] private AudioClip spareSFX;

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

        public void PlayMenuMusic()
        {
            if (!musicSource || !menuMusic) return;
            musicSource.clip = menuMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
        public void PlayGameplayMusic()
        {
            if (!musicSource || !gameplayMusic) return;
            musicSource.clip = gameplayMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
        public void PauseMusic() { if (musicSource) musicSource.Pause(); }
        public void ResumeMusic() { if (musicSource) musicSource.UnPause(); }
        public void PlayGameOverSound() { }

        public void PlayStrike() { if (sfxSource && strikeSFX) sfxSource.PlayOneShot(strikeSFX); }
        public void PlaySpare() { if (sfxSource && spareSFX) sfxSource.PlayOneShot(spareSFX); }
        public void PlaySFX(AudioClip clip, float vol = 1f) { if (sfxSource && clip) sfxSource.PlayOneShot(clip, vol); }
    }
}
