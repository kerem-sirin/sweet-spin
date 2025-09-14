using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SweetSpin.Core;

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

        private IAutoPlayService autoPlayService; // Changed to interface
        private bool isInAutoPlayMode = false;

        protected override void Start()
        {
            // Set to lock only during non-idle states (except during auto-play when it becomes stop)
            lockDuringStates = LockDuringStates.AllExceptIdle;

            base.Start();

            // Get service from ServiceLocator instead of FindObjectOfType
            autoPlayService = ServiceLocator.Instance.Get<IAutoPlayService>();

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
            eventBus?.Subscribe<SpinCompletedEvent>(OnSpinCompleted);
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
            eventBus?.Unsubscribe<SpinCompletedEvent>(OnSpinCompleted);
            eventBus?.Unsubscribe<AutoPlayRemainingChangedEvent>(OnRemainingChanged);
        }

        private void OnButtonClick()
        {
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

        private void OnSpinCompleted(SpinCompletedEvent e)
        {
            // Update remaining count display
            if (isInAutoPlayMode && autoPlayService != null)
            {
                UpdateRemainingDisplay();
            }
        }


        private void OnRemainingChanged(AutoPlayRemainingChangedEvent e)
        {
            if (isInAutoPlayMode)
            {
                UpdateRemainingDisplay();
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
                remainingGameObject.gameObject.SetActive(isInAutoPlayMode);
                if (isInAutoPlayMode)
                {
                    UpdateRemainingDisplay();
                }
            }
        }

        private void UpdateRemainingDisplay()
        {
            if (autoPlayService != null)
            {
                remainingText.text = autoPlayService.RemainingSpins.ToString();
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