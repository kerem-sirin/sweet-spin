using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SweetSpin.Core;
using DG.Tweening;

namespace SweetSpin
{
    /// <summary>
    /// Button that switches between "Auto" and "Stop" states
    /// </summary>
    public class AutoPlayButton : UILockableComponent
    {
        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private Image buttonImage;
        [SerializeField] private GameObject remainingGameObject;
        [SerializeField] private TextMeshProUGUI remainingText;
        [SerializeField] private AutoPlayPanel autoPlayPanel;

        [Header("Display Settings")]
        [SerializeField] private Sprite autoSprite;
        [SerializeField] private Sprite stopSprite;

        private IAutoPlayService autoPlayService;
        private IAudioService audioService;
        private bool isInAutoPlayMode = false;

        protected override void Start()
        {
            // Set to lock only during non-idle states (except during auto-play when it becomes stop)
            lockDuringStates = LockDuringStates.AllExceptIdle;

            base.Start();

            // Get services from service locator
            autoPlayService = ServiceLocator.Instance.Get<IAutoPlayService>();
            audioService = ServiceLocator.Instance.Get<IAudioService>();

            if (button == null)
                button = GetComponent<Button>();

            if (button != null)
                button.onClick.AddListener(OnButtonClick);

            if (remainingGameObject == null)
            {
                 Debug.LogError("AutoPlayButton: Remaining GameObject is not assigned.");
            }
            if (remainingText == null)
            {
                 Debug.LogError("AutoPlayButton: Remaining Text is not assigned.");
            }

            // Subscribe to auto-play events
            eventBus?.Subscribe<AutoPlayStartedEvent>(OnAutoPlayStarted);
            eventBus?.Subscribe<AutoPlayStoppedEvent>(OnAutoPlayStopped);
            eventBus?.Subscribe<AutoPlayRemainingChangedEvent>(OnRemainingChanged);

            // Initialize display
            UpdateButtonDisplay();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            button?.onClick.RemoveListener(OnButtonClick);
            eventBus?.Unsubscribe<AutoPlayStartedEvent>(OnAutoPlayStarted);
            eventBus?.Unsubscribe<AutoPlayStoppedEvent>(OnAutoPlayStopped);
            eventBus?.Unsubscribe<AutoPlayRemainingChangedEvent>(OnRemainingChanged);
        }

        private void OnButtonClick()
        {
            audioService?.PlayButtonClick();

            if (isInAutoPlayMode)
            {
                // In auto-play mode - request stop
                autoPlayService?.RequestStop();
            }
            else
            {
                // Not in auto-play - show selection panel
                autoPlayPanel?.Show();
            }
        }

        private void OnAutoPlayStarted(AutoPlayStartedEvent e)
        {
            isInAutoPlayMode = true;
            UpdateButtonDisplay();
        }

        private void OnAutoPlayStopped(AutoPlayStoppedEvent e)
        {
            isInAutoPlayMode = false;
            UpdateButtonDisplay();
        }


        private void OnRemainingChanged(AutoPlayRemainingChangedEvent e)
        {
            if (isInAutoPlayMode && e.RemainingSpins != 0)
            {
                UpdateRemainingDisplay(false);
            }
        }

        private void UpdateButtonDisplay()
        {
            if (buttonImage != null)
            {
                buttonImage.sprite = isInAutoPlayMode ? stopSprite : autoSprite;
            }

            // Show/hide remaining text
            if (remainingGameObject != null)
            {
                // show/hide with tween
                if(isInAutoPlayMode)
                {
                    // Animate in
                    remainingGameObject.transform
                        .DOScale(Vector3.one, 0.5f)
                        .OnStart(() =>
                        {
                            remainingGameObject.transform.localScale = Vector3.zero;
                            UpdateRemainingDisplay(true);
                            remainingGameObject.SetActive(true);
                        })
                        .SetEase(Ease.OutBack);
                }
                else
                {
                    // Animate out
                    remainingGameObject.transform
                    .DOScale(Vector3.zero, 0.5f)
                    .OnComplete(() =>
                    {
                        remainingGameObject.transform.localScale = Vector3.zero;
                        remainingGameObject.SetActive(false);
                    })
                    .SetEase(Ease.OutBack);
                }
            }
        }

        private void UpdateRemainingDisplay(bool isInstant)
        {
            if (autoPlayService != null && isInstant)
            {
                remainingText.text = autoPlayService.RemainingSpins.ToString();
            }
            else
            {
                // Animate the remainingGameObject (increase scale a bit and go back to normal)
                remainingGameObject.transform
                    .DOScale(1.2f, 0.2f)
                    .OnStart(() =>
                    {
                        remainingText.text = autoPlayService.RemainingSpins.ToString();
                    })
                    .OnComplete(() =>
                    {
                        remainingGameObject.transform
                            .DOScale(Vector3.one, 0.2f);
                    })
                    .SetEase(Ease.OutBack);
            }
        }

        protected override void SetLocked(bool locked)
        {
            // Special handling: during auto-play, button stays interactive as stop button
            if (button != null)
            {
                if (isInAutoPlayMode)
                {
                    // Always interactive during auto-play (acts as stop button)
                    button.interactable = true;
                }
                else
                {
                    // Normal locking behavior when not in auto-play
                    button.interactable = !locked;
                }
            }
        }
    }
}