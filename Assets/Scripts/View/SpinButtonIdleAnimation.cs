using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using SweetSpin.Core;

namespace SweetSpin.VFX
{
    public class SpinButtonIdleAnimation : MonoBehaviour
    {
        [Header("Arrow Settings")]
        [SerializeField] private Transform arrowIcon;
        [SerializeField] private float idleDelay = 3f;
        [SerializeField] private float rotationDuration = 1f;
        [SerializeField] private Ease rotationEase = Ease.InOutQuad;

        private float idleTimer = 0f;
        private bool hasPlayedAnimation = false;
        private Tween rotationTween;
        private IEventBus eventBus;

        void Start()
        {
            eventBus = ServiceLocator.Instance.Get<IEventBus>();
            eventBus?.Subscribe<SpinStartedEvent>(OnSpinStarted);
            eventBus?.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        void Update()
        {
            // Only count idle time when game is in idle state
            if (GameIsIdle())
            {
                idleTimer += Time.deltaTime;

                if (idleTimer >= idleDelay && !hasPlayedAnimation)
                {
                    PlayIdleAnimation();
                    hasPlayedAnimation = true;
                }
            }
        }

        private void PlayIdleAnimation()
        {
            if (arrowIcon == null) return;

            // Kill any existing tween
            rotationTween?.Kill();

            // Rotate 360 degrees
            rotationTween = arrowIcon.DORotate(new Vector3(0, 0, -360), rotationDuration, RotateMode.FastBeyond360)
                .SetEase(rotationEase)
                .SetRelative(true)
                .OnComplete(() => {
                    // Reset for next idle period
                    ResetIdleTimer();
                });
        }

        private void OnSpinStarted(SpinStartedEvent e)
        {
            ResetIdleTimer();

            // Kill rotation if it's playing
            rotationTween?.Kill();
            arrowIcon.rotation = Quaternion.identity; // Reset rotation
        }

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            if (e.NewState == GameState.Idle)
            {
                ResetIdleTimer();
            }
        }

        private void ResetIdleTimer()
        {
            idleTimer = 0f;
            hasPlayedAnimation = false;
        }

        private bool GameIsIdle()
        {
            // You might want to check the actual game state here
            // For now, we'll assume it's idle if the button is interactable
            var button = GetComponent<Button>();
            return button != null && button.interactable;
        }

        void OnDestroy()
        {
            rotationTween?.Kill();
            eventBus?.Unsubscribe<SpinStartedEvent>(OnSpinStarted);
            eventBus?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }
    }
}