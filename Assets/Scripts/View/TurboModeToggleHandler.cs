using SweetSpin.Core;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SweetSpin
{
    /// <summary>
    /// Handles the turbo mode toggle UI element and publishes state changes
    /// through the event bus for complete decoupling from game logic.
    /// </summary>
    public class TurboModeToggleHandler : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Toggle turboToggle;
        [SerializeField] private Image turboIcon;
        [SerializeField] private Image checkmarkBackgroundIcon;
        [SerializeField] private RectTransform checkmarkTransform;

        [Header("Visual Settings")]
        [SerializeField] private Color turboInactiveIconColor;
        [SerializeField] private Color turboActiveIconColor;
        [SerializeField] private Color turboInactiveBackgroundColor;
        [SerializeField] private Color turboActiveBackgroundColor;
        [SerializeField] private float toggleAnimationDuration = 0.2f;

        private IEventBus eventBus;
        private bool isInitialized = false;

        private void Start()
        {
            // Get event bus from service locator
            eventBus = ServiceLocator.Instance.Get<IEventBus>();

            if (eventBus == null)
            {
                Debug.LogError("TurboModeToggleHandler: EventBus not found!");
                return;
            }

            // Setup toggle listener
            if (turboToggle == null)
            {
                Debug.LogError("TurboModeToggleHandler:Toggle component not found!", this);
            }

            if (turboToggle != null)
            {
                turboToggle.onValueChanged.AddListener(OnToggleValueChanged);

                // Load saved preference
                bool savedTurboMode = PlayerPrefs.GetInt("TurboMode", 0) == 1;
                turboToggle.isOn = savedTurboMode;

                // Update visuals to match initial state
                UpdateVisuals(savedTurboMode);

                // Publish initial state
                eventBus.Publish(new TurboModeChangedEvent(savedTurboMode));
            }

            // Subscribe to turbo mode changes from other sources (like keyboard shortcuts)
            eventBus.Subscribe<TurboModeChangedEvent>(OnTurboModeChanged);

            isInitialized = true;
        }

        private void OnDestroy()
        {
            if (turboToggle != null)
            {
                turboToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }

            if (eventBus != null)
            {
                eventBus.Unsubscribe<TurboModeChangedEvent>(OnTurboModeChanged);
            }
        }

        private void Update()
        {
            // Optional: Keyboard shortcut for turbo toggle
            if (Input.GetKeyDown(KeyCode.T) && isInitialized)
            {
                if (turboToggle != null)
                {
                    turboToggle.isOn = !turboToggle.isOn;
                    // OnToggleValueChanged will be called automatically
                }
            }
        }

        private void OnToggleValueChanged(bool isOn)
        {
            // Save preference
            PlayerPrefs.SetInt("TurboMode", isOn ? 1 : 0);
            PlayerPrefs.Save();

            // Update visuals
            UpdateVisuals(isOn);

            // Publish event through event bus
            eventBus?.Publish(new TurboModeChangedEvent(isOn));

            // Log for debugging
            Debug.Log($"Turbo Mode: {(isOn ? "ENABLED" : "DISABLED")}");
        }

        private void OnTurboModeChanged(TurboModeChangedEvent e)
        {
            // Update toggle without triggering the event again
            if (turboToggle != null && turboToggle.isOn != e.IsEnabled)
            {
                turboToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
                turboToggle.isOn = e.IsEnabled;
                turboToggle.onValueChanged.AddListener(OnToggleValueChanged);

                UpdateVisuals(e.IsEnabled);
            }
        }

        private void UpdateVisuals(bool isTurboActive)
        {
            // Update icon color
            if (turboIcon != null)
            {
                turboToggle.interactable = false;
                // tween color change for smooth transition
                turboIcon.DOColor(isTurboActive ? turboActiveIconColor : turboInactiveIconColor, toggleAnimationDuration);
                checkmarkBackgroundIcon.DOColor(isTurboActive ? turboActiveBackgroundColor : turboInactiveBackgroundColor, toggleAnimationDuration);
                // Tween the checkmark position for a simple animation effect
                if (checkmarkTransform != null)
                {
                    float targetX = isTurboActive ? 20f : -20f;
                    checkmarkTransform.DOAnchorPosX(targetX, toggleAnimationDuration)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(()=>turboToggle.interactable = true);
                }
            }
        }
    }
}
