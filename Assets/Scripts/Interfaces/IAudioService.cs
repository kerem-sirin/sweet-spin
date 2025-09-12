using UnityEngine;

namespace SweetSpin
{
    /// <summary>
    /// Audio service for sound management
    /// </summary>
    public interface IAudioService
    {
        void PlaySpinSound();
        void PlayWinSound(WinTier tier);
        void PlayButtonClick();
        void StopAllSounds();
        void SetVolume(float volume);
    }
}
