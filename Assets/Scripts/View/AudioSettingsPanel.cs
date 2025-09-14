using DG.Tweening;
using SweetSpin.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SweetSpin
{
    /// <summary>
    /// Simple audio settings panel with music and SFX toggles
    /// </summary>
    public class AudioSettingsPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backgroundAsButton;
        [SerializeField] private Button audioSettingsButton; // Button that opens this panel

        [Header("Music Toggle")]
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Image musicCheckmarkBackground;
        [SerializeField] private RectTransform musicCheckmarkTransform;

        [Header("SFX Toggle")]
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Image sfxCheckmarkBackground;
        [SerializeField] private RectTransform sfxCheckmarkTransform;

        [Header("Visual Settings")]
        [SerializeField] private Color activeBackgroundColor;
        [SerializeField] private Color inactiveBackgroundColor;
        [SerializeField] private float toggleAnimationDuration = 0.2f;

        [Header("Panel Animation")]
        [SerializeField] private float panelAnimationDuration = 0.3f;

        private IAudioService audioService;

        private void Start()
        {
            audioService = ServiceLocator.Instance.Get<IAudioService>();

            // Setup toggle listeners
            if (musicToggle != null)
            {
                // Set initial state from saved settings
                bool musicEnabled = audioService.IsMusicEnabled();
                musicToggle.isOn = musicEnabled;
                UpdateMusicVisuals(musicEnabled, true); // Instant update on start

                musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
            }

            if (sfxToggle != null)
                if (sfxToggle != null)
                {
                    // Set initial state from saved settings
                    bool sfxEnabled = audioService.IsSFXEnabled();
                    sfxToggle.isOn = sfxEnabled;
                    UpdateSFXVisuals(sfxEnabled, true); // Instant update on start

                    sfxToggle.onValueChanged.AddListener(OnSFXToggleChanged);
                }

            // Setup close buttons
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => {
                    audioService?.PlayButtonClick();
                    Hide();
                });
            }

            if (backgroundAsButton != null)
            {
                backgroundAsButton.onClick.AddListener(() => {
                    audioService?.PlayButtonClick();
                    Hide();
                });
            }

            // Setup the button that opens this panel
            if (audioSettingsButton != null)
            {
                audioSettingsButton.onClick.AddListener(() => {
                    audioService?.PlayButtonClick();
                    Show();
                });
            }

            // Hide panel initially
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            musicToggle?.onValueChanged.RemoveAllListeners();
            sfxToggle?.onValueChanged.RemoveAllListeners();
            closeButton?.onClick.RemoveAllListeners();
            backgroundAsButton?.onClick.RemoveAllListeners();
            audioSettingsButton?.onClick.RemoveAllListeners();
        }

        public void Show()
        {
            if (panelRoot != null)
            {
                // Update toggle states before showing
                if (musicToggle != null && audioService != null)
                {
                    bool musicEnabled = audioService.IsMusicEnabled();
                    musicToggle.isOn = musicEnabled;
                    UpdateMusicVisuals(musicEnabled, true);
                }

                if (sfxToggle != null && audioService != null)
                {
                    bool sfxEnabled = audioService.IsSFXEnabled();
                    sfxToggle.isOn = sfxEnabled;
                    UpdateSFXVisuals(sfxEnabled, true);
                }

                panelRoot.SetActive(true);

                // Animate in
                panelRoot.transform.localScale = Vector3.zero;
                panelRoot.transform.DOScale(Vector3.one, panelAnimationDuration)
                    .SetEase(Ease.OutBack);
            }
        }

        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.transform.DOScale(Vector3.zero, panelAnimationDuration * 0.7f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => panelRoot.SetActive(false));
            }
        }

        private void OnMusicToggleChanged(bool isOn)
        {
            audioService?.PlayButtonClick();
            audioService?.ToggleMusic();
            UpdateMusicVisuals(isOn, false);
        }

        private void OnSFXToggleChanged(bool isOn)
        {
            audioService?.PlayButtonClick();
            audioService?.ToggleSFX();
            UpdateSFXVisuals(isOn, false);
        }

        private void UpdateMusicVisuals(bool isActive, bool instant = false)
        {
            if (musicToggle != null)
            {
                musicToggle.interactable = false;
            }

            float duration = instant ? 0f : toggleAnimationDuration;

            // Animate background color
            if (musicCheckmarkBackground != null)
            {
                musicCheckmarkBackground.DOColor(isActive ? activeBackgroundColor : inactiveBackgroundColor, duration);
            }

            // Animate checkmark position
            if (musicCheckmarkTransform != null)
            {
                float targetX = isActive ? 20f : -20f;
                if (instant)
                {
                    musicCheckmarkTransform.anchoredPosition = new Vector2(targetX, musicCheckmarkTransform.anchoredPosition.y);
                    if (musicToggle != null) musicToggle.interactable = true;
                }
                else
                {
                    musicCheckmarkTransform.DOAnchorPosX(targetX, duration)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => {
                            if (musicToggle != null) musicToggle.interactable = true;
                        });
                }
            }
        }

        private void UpdateSFXVisuals(bool isActive, bool instant = false)
        {
            if (sfxToggle != null)
            {
                sfxToggle.interactable = false;
            }

            float duration = instant ? 0f : toggleAnimationDuration;

            // Animate background color
            if (sfxCheckmarkBackground != null)
            {
                sfxCheckmarkBackground.DOColor(isActive ? activeBackgroundColor : inactiveBackgroundColor, duration);
            }

            // Animate checkmark position
            if (sfxCheckmarkTransform != null)
            {
                float targetX = isActive ? 20f : -20f;
                if (instant)
                {
                    sfxCheckmarkTransform.anchoredPosition = new Vector2(targetX, sfxCheckmarkTransform.anchoredPosition.y);
                    if (sfxToggle != null) sfxToggle.interactable = true;
                }
                else
                {
                    sfxCheckmarkTransform.DOAnchorPosX(targetX, duration)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => {
                            if (sfxToggle != null) sfxToggle.interactable = true;
                        });
                }
            }
        }
    }
}