using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace SweetSpin
{
    /// <summary>
    /// Enhanced audio service implementation with music and SFX management
    /// </summary>
    public class AudioService : MonoBehaviour, IAudioService
    {
        private const string musicVolumeParameter = "MusicVolume";
        private const string sfxVolumeParameter = "SFXVolume";
        private const float MIN_MIXER_DB = -80f;
        private const float MAX_MIXER_DB = 0f;

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;

        [Header("Music Clips")]
        [SerializeField] private AudioClip mainMusic;

        [Header("Audio Ducking")]
        [SerializeField, Range(0f, 1f)] private float musicDuckingLevel = 0.3f; // Music volume during gameplay (30% of normal)
        [SerializeField] private float duckFadeTime = 0.5f; // Time to fade music down/up

        [Header("Button SFX")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip buttonHoverSound;

        [Header("Reel SFX")]
        [SerializeField] private AudioClip reelStartSound;
        [SerializeField] private AudioClip reelStopSound;
        [SerializeField] private AudioClip reelSpinLoopSound; // Continuous spinning sound

        [Header("Win SFX")]
        [SerializeField] private AudioClip smallWinSound;
        [SerializeField] private AudioClip mediumWinSound;
        [SerializeField] private AudioClip bigWinSound;
        [SerializeField] private AudioClip megaWinSound;
        [SerializeField] private AudioClip jackpotWinSound;

        [Header("Win Line SFX")]
        [SerializeField] private AudioClip payLineSound; // Sound for each line animation

        [Header("Special SFX")]
        [SerializeField] private AudioClip noWinSound;
        [SerializeField] private AudioClip coinDropSound;

        [Header("Volume Settings")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

        [Header("Audio Settings")]
        [SerializeField] private bool enableMusic = true;
        [SerializeField] private bool enableSFX = true;
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float fadeOutDuration = 0.5f;

        private AudioSource musicSource;
        private AudioSource[] sfxSources;
        private int sfxSourceCount = 5; // Pool size for SFX

        private Coroutine duckingCoroutine;
        private float originalMusicVolume;
        private bool isMusicDucked = false;

        private List<AudioSource> availableSfxSources;
        private AudioSource currentSpinLoopSource;
        private Coroutine musicFadeCoroutine;
        private int nextSfxSourceIndex = 0;

        private void Awake()
        {
            InitializeAudioSources();
            LoadVolumeSettings();
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Start menu music
            PlayMusic(mainMusic);
        }

        private void InitializeAudioSources()
        {
            // Create music source if not assigned
            if (musicSource == null)
            {
                GameObject musicGO = new GameObject("MusicSource");
                musicGO.transform.SetParent(transform);
                musicSource = musicGO.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.priority = 128;
            }

            // Assign mixer group to music source
            if (musicMixerGroup != null)
            {
                musicSource.outputAudioMixerGroup = musicMixerGroup;
            }

            // Create SFX source pool
            availableSfxSources = new List<AudioSource>();

            if (sfxSources == null || sfxSources.Length == 0)
            {
                sfxSources = new AudioSource[sfxSourceCount];
                for (int i = 0; i < sfxSourceCount; i++)
                {
                    GameObject sfxGO = new GameObject($"SFXSource_{i}");
                    sfxGO.transform.SetParent(transform);
                    AudioSource source = sfxGO.AddComponent<AudioSource>();
                    source.playOnAwake = false;
                    source.priority = 256;

                    // Assign mixer group to each SFX source
                    if (sfxMixerGroup != null)
                    {
                        source.outputAudioMixerGroup = sfxMixerGroup;
                    }

                    sfxSources[i] = source;
                }
            }

            availableSfxSources.AddRange(sfxSources);
        }

        // Convert linear volume (0-1) to decibels for mixer
        private float LinearToDecibel(float linear)
        {
            if (linear <= 0f)
                return MIN_MIXER_DB;
            return Mathf.Log10(linear) * 20f;
        }

        private void LoadVolumeSettings()
        {
            // Load saved volume settings
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            enableMusic = PlayerPrefs.GetInt("EnableMusic", 1) == 1;
            enableSFX = PlayerPrefs.GetInt("EnableSFX", 1) == 1;

            ApplyVolumeSettings();
        }

        public void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.SetInt("EnableMusic", enableMusic ? 1 : 0);
            PlayerPrefs.SetInt("EnableSFX", enableSFX ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void ApplyVolumeSettings()
        {
            // Set mixer volumes
            if (audioMixer != null)
            {
                // Music channel
                float musicDb = enableMusic ? LinearToDecibel(musicVolume * masterVolume) : MIN_MIXER_DB;
                audioMixer.SetFloat(musicVolumeParameter, musicDb);

                // SFX channel  
                float sfxDb = enableSFX ? LinearToDecibel(sfxVolume * masterVolume) : MIN_MIXER_DB;
                audioMixer.SetFloat(sfxVolumeParameter, sfxDb);
            }

            // Handle ducking separately on the music source
            if (musicSource != null && enableMusic)
            {
                float duckMultiplier = isMusicDucked ? musicDuckingLevel : 1f;
                musicSource.volume = duckMultiplier;
            }
        }

        private AudioSource GetAvailableSfxSource()
        {
            // Round-robin through SFX sources
            AudioSource source = sfxSources[nextSfxSourceIndex];
            nextSfxSourceIndex = (nextSfxSourceIndex + 1) % sfxSources.Length;

            // Stop any playing sound on this source
            if (source.isPlaying)
            {
                source.Stop();
            }

            // Reset to defaults
            source.pitch = 1f;
            source.volume = sfxVolume * masterVolume;
            source.loop = false;
            source.clip = null;

            return source;
        }

        private void PlaySFX(AudioClip clip, float volumeScale = 1f, float pitch = 1f)
        {
            if (!enableSFX || clip == null) return;

            AudioSource source = GetAvailableSfxSource();
            source.clip = clip;
            source.volume = sfxVolume * masterVolume * volumeScale;
            source.pitch = pitch;
            source.Play();
        }

        // Music Control
        public void PlayMusic(AudioClip musicClip, bool fadeIn = true)
        {
            if (!enableMusic || musicClip == null || musicSource == null) return;

            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }

            if (fadeIn)
            {
                musicFadeCoroutine = StartCoroutine(FadeInMusic(musicClip));
            }
            else
            {
                musicSource.clip = musicClip;
                musicSource.volume = musicVolume * masterVolume;
                musicSource.Play();
            }
        }

        private IEnumerator FadeInMusic(AudioClip newClip)
        {
            // Fade out current music if playing
            if (musicSource.isPlaying)
            {
                float startVolume = musicSource.volume;
                for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
                {
                    musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeOutDuration);
                    yield return null;
                }
                musicSource.Stop();
            }

            // Start new music and fade in
            musicSource.clip = newClip;
            musicSource.Play();
            musicSource.volume = 0;

            float targetVolume = musicVolume * masterVolume;
            for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(0, targetVolume, t / fadeInDuration);
                yield return null;
            }
            musicSource.volume = targetVolume;
        }

        public void StopMusic(bool fadeOut = true)
        {
            if (musicSource == null) return;

            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }

            if (fadeOut)
            {
                musicFadeCoroutine = StartCoroutine(FadeOutMusic());
            }
            else
            {
                musicSource.Stop();
            }
        }

        private IEnumerator FadeOutMusic()
        {
            float startVolume = musicSource.volume;
            for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeOutDuration);
                yield return null;
            }
            musicSource.Stop();
        }

        /// <summary>
        /// Reduces music volume for gameplay clarity
        /// </summary>
        public void DuckMusic()
        {
            if (isMusicDucked || musicSource == null || !enableMusic) return;

            if (duckingCoroutine != null)
            {
                StopCoroutine(duckingCoroutine);
            }

            duckingCoroutine = StartCoroutine(FadeMusicVolume(musicDuckingLevel, duckFadeTime));
            isMusicDucked = true;
            Debug.Log("[Audio] Ducking music volume");
        }

        /// <summary>
        /// Restores music to normal volume
        /// </summary>
        public void RestoreMusic()
        {
            if (!isMusicDucked || musicSource == null || !enableMusic) return;

            if (duckingCoroutine != null)
            {
                StopCoroutine(duckingCoroutine);
            }

            duckingCoroutine = StartCoroutine(FadeMusicVolume(1f, duckFadeTime));
            isMusicDucked = false;
            Debug.Log("[Audio] Restoring music volume");
        }

        /// <summary>
        /// Smoothly fades music volume to target level
        /// </summary>
        private IEnumerator FadeMusicVolume(float targetMultiplier, float duration)
        {
            float startVolume = musicSource.volume;
            float targetVolume = targetMultiplier; // Now just the duck multiplier

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                musicSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }

            musicSource.volume = targetVolume;
        }

        // SFX Methods
        public void PlayButtonClick()
        {
            PlaySFX(buttonClickSound, 0.8f);
        }

        public void PlayButtonHover()
        {
            PlaySFX(buttonHoverSound, 0.5f);
        }

        public void PlaySpinSound()
        {
            PlaySFX(reelStartSound);

            // Start spinning loop sound
            if (reelSpinLoopSound != null && currentSpinLoopSource == null)
            {
                currentSpinLoopSource = GetAvailableSfxSource();
                currentSpinLoopSource.clip = reelSpinLoopSound;
                currentSpinLoopSource.loop = true;
                currentSpinLoopSource.volume = sfxVolume * masterVolume * 0.6f;
                currentSpinLoopSource.Play();
            }

            Debug.Log("[Audio] Playing spin sound");
        }

        public void StopSpinSound()
        {
            if (currentSpinLoopSource != null)
            {
                currentSpinLoopSource.Stop();
                currentSpinLoopSource.loop = false;
                currentSpinLoopSource = null;
            }
        }

        public void PlayReelStop(int reelIndex)
        {
            // Slightly vary pitch for each reel
            float pitch = 1f + (reelIndex * 0.05f);
            PlaySFX(reelStopSound, 0.9f, pitch);

            // Stop spin loop after last reel with a small delay
            if (reelIndex >= 4 && reelStopSound != null)
            {
                // Delay stopping the spin sound to ensure the last reel stop sound plays
                StartCoroutine(StopSpinSoundDelayed(reelStopSound.length));
            }
        }

        // Add this new coroutine method:
        private IEnumerator StopSpinSoundDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            StopSpinSound();
        }

        public void PlayWinSound(WinTier tier)
        {
            AudioClip winClip = tier switch
            {
                WinTier.Small => smallWinSound,
                WinTier.Medium => mediumWinSound,
                WinTier.Big => bigWinSound,
                WinTier.Mega => megaWinSound,
                WinTier.Jackpot => jackpotWinSound,
                _ => noWinSound
            };

            // Higher tier wins play louder
            float volumeScale = tier switch
            {
                WinTier.Jackpot => 1.2f,
                WinTier.Mega => 1.1f,
                WinTier.Big => 1.0f,
                _ => 0.9f
            };

            PlaySFX(winClip, volumeScale);
            Debug.Log($"[Audio] Playing {tier} win sound");
        }

        public void PlayWinLineSound(int lineIndex)
        {
            // Vary pitch slightly based on line index for variety
            float pitch = 1f + (lineIndex * 0.1f);
            PlaySFX(payLineSound, 0.8f, pitch);
            Debug.Log($"[Audio] Playing win line sound for line {lineIndex}");
        }

        public void PlayCoinDrop()
        {
            // Random pitch for variety
            float pitch = Random.Range(0.9f, 1.1f);
            PlaySFX(coinDropSound, 0.7f, pitch);
        }

        public void StopAllSounds()
        {
            StopMusic(false);
            StopSpinSound();

            foreach (var source in sfxSources)
            {
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                }
            }

            Debug.Log("[Audio] Stopping all sounds");
        }

        // Volume Control Methods
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
            SaveVolumeSettings();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
            SaveVolumeSettings();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
            SaveVolumeSettings();
        }

        public void SetVolume(float volume)
        {
            SetMasterVolume(volume);
        }

        public void ToggleMusic()
        {
            enableMusic = !enableMusic;

            if (enableMusic && musicSource != null)
            {
                // If enabling and music isn't playing, start it
                if (!musicSource.isPlaying && mainMusic != null)
                {
                    PlayMusic(mainMusic, false); // No fade for resume
                }
            }
            else if (!enableMusic && musicSource != null)
            {
                // Just mute via mixer, don't stop the music
                // This way position is maintained
            }

            ApplyVolumeSettings();
            SaveVolumeSettings();
        }

        public void ToggleSFX()
        {
            enableSFX = !enableSFX;
            ApplyVolumeSettings();
            SaveVolumeSettings();
        }

        // Getters for UI
        public float GetMasterVolume() => masterVolume;
        public float GetMusicVolume() => musicVolume;
        public float GetSFXVolume() => sfxVolume;
        public bool IsMusicEnabled() => enableMusic;
        public bool IsSFXEnabled() => enableSFX;
    }
}