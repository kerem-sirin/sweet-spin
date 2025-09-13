using UnityEngine;

namespace SweetSpin
{
    /// <summary>
    /// Audio service implementation
    /// </summary>
    public class AudioService : IAudioService
    {
        private float masterVolume = 1f;

        public void PlaySpinSound()
        {
            Debug.Log("[Audio] Playing spin sound");
            // Implementation will use Unity AudioSource
        }

        public void PlayWinSound(WinTier tier)
        {
            Debug.Log($"[Audio] Playing {tier} win sound");
        }

        public void PlayButtonClick()
        {
            Debug.Log("[Audio] Button click");
        }

        public void StopAllSounds()
        {
            Debug.Log("[Audio] Stopping all sounds");
        }

        public void SetVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
        }
    }
}
