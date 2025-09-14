using UnityEngine;

namespace SweetSpin
{
    /// <summary>
    /// Audio service interface for sound management
    /// </summary>
    public interface IAudioService
    {
        // Core SFX
        void PlaySpinSound();
        void StopSpinSound();
        void PlayReelStop(int reelIndex);
        void PlayWinSound(WinTier tier);
        void PlayWinLineSound(int lineIndex);
        void PlayButtonClick();
        void PlayButtonHover();
        void PlayCoinDrop();

        // Music Control
        void PlayMusic(AudioClip musicClip, bool fadeIn = true);
        void StopMusic(bool fadeOut = true);
        void DuckMusic();
        void RestoreMusic();

        // General Control
        void StopAllSounds();

        // Volume Control
        void SetVolume(float volume); // Master volume (legacy)
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
        void ToggleMusic();
        void ToggleSFX();

        // Getters for UI
        float GetMasterVolume();
        float GetMusicVolume();
        float GetSFXVolume();
        bool IsMusicEnabled();
        bool IsSFXEnabled();
    }
}