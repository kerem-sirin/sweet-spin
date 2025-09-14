using DG.Tweening;
using SweetSpin.Core;
using System.Collections;
using UnityEngine;

namespace SweetSpin.Animation
{
    public class IntroSequenceController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private CanvasGroup backgroundGroup;
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private Transform header;
        [SerializeField] private Transform pigCharacter;
        [SerializeField] private Transform slotMachine;
        [SerializeField] private Transform buttonPanel;
        [SerializeField] private GameTitleFloater titleFloater;

        [Header("Intro Settings")]
        [SerializeField] private float fadeFromBlackDuration = 2f;
        [SerializeField] private float initialDelay = 0.5f;
        [SerializeField] private float titleRevealDuration = 1f;
        [SerializeField] private float titleHoldDuration = 1f;
        [SerializeField] private float titleMoveDuration = 0.5f;
        [SerializeField] private float elementsFadeInDuration = 1f;
        [SerializeField] private bool skipIntroInEditor = false; // For faster testing in editor

        [Header("Audio")]
        [SerializeField] private AudioClip revealSound;
        [SerializeField] private AudioClip whooshSound;

        private Vector3 titleOriginalPosition;
        private Vector3 titleOriginalScale;
        private bool isPlayingIntro = false;

        void Start()
        {
            // Store original transforms
            if (header != null)
            {
                titleOriginalPosition = header.localPosition;
                titleOriginalScale = header.localScale;
            }

            // Skip intro in editor if desired (for faster testing)
            if (skipIntroInEditor)
            {
                SetFinalPositions();
                return;
            }

            // Always play intro
            StartCoroutine(PlayIntroSequence());
        }

        private IEnumerator PlayIntroSequence()
        {
            isPlayingIntro = true;

            // Disable game input during intro
            DisableGameInput();

            // Hide UI for intro
            if (backgroundGroup != null)
                backgroundGroup.alpha = 1f;

            if (header != null)
            {
                header.localScale = Vector3.zero;
                var titleCanvasGroup = header.GetComponent<CanvasGroup>();
                if (titleCanvasGroup == null)
                    titleCanvasGroup = header.gameObject.AddComponent<CanvasGroup>();
                titleCanvasGroup.alpha = 0f;
            }

            SetElementsInvisible();

            // Start fade out from black
            SetFadeActive();
            yield return new WaitForSeconds(1.5f);

            fadeGroup.DOFade(0f, fadeFromBlackDuration).SetEase(Ease.OutQuad)
                .OnComplete(()=>fadeGroup.gameObject.SetActive(false));

            // Calculate center position in Canvas space
            Vector3 titleCenterPos = Vector3.zero; // Center of canvas (assuming anchored to center)

            yield return new WaitForSeconds(initialDelay);

            // 1. Title appears in center with scale and fade
            if (header != null)
            {
                header.localPosition = titleCenterPos;

                // Play reveal sound
                if (revealSound != null)
                {
                    var audioService = ServiceLocator.Instance.Get<IAudioService>();
                    audioService?.PlaySFX(revealSound);
                }

                Sequence titleReveal = DOTween.Sequence();
                titleReveal.Append(header.DOScale(1f, titleRevealDuration).SetEase(Ease.OutBack));
                titleReveal.Join(header.GetComponent<CanvasGroup>().DOFade(1f, titleRevealDuration * 0.7f));

                yield return titleReveal.WaitForCompletion();
            }

            yield return new WaitForSeconds(titleHoldDuration);

            // 2. Title moves to final position and starts floating
            if (header != null)
            {
                // Play whoosh sound
                if (whooshSound != null)
                {
                    var audioService = ServiceLocator.Instance.Get<IAudioService>();
                    audioService?.PlaySFX(whooshSound);
                }

                Sequence titleMove = DOTween.Sequence();
                titleMove.Append(header.DOLocalMove(titleOriginalPosition, titleMoveDuration).SetEase(Ease.InOutSine));
                titleMove.Join(header.DOScale(titleOriginalScale, titleMoveDuration)).
                    OnComplete(()=> titleFloater.Initialize());

                yield return titleMove.WaitForCompletion();
            }

            // 3. Fade in other elements
            yield return StartCoroutine(FadeInGameElements());

            // Enable game input
            EnableGameInput();
            isPlayingIntro = false;
        }

        private void SetElementsInvisible()
        {
            SetAlpha(pigCharacter, 0f);
            SetAlpha(slotMachine, 0f);
            SetAlpha(buttonPanel, 0f);
        }

        private void SetFadeActive()
        {
            if (fadeGroup != null)
            {
                fadeGroup.alpha = 1f;
            }
        }

        private IEnumerator FadeInGameElements()
        {
            float stagger = 0.2f;

            // Create staggered fade in
            Sequence fadeSequence = DOTween.Sequence();

            if (slotMachine != null)
                fadeSequence.Append(FadeElement(slotMachine, 1f, elementsFadeInDuration));

            if (pigCharacter != null)
                fadeSequence.Insert(stagger, FadeElement(pigCharacter, 1f, elementsFadeInDuration));

            if (buttonPanel != null)
                fadeSequence.Insert(stagger * 2, FadeElement(buttonPanel, 1f, elementsFadeInDuration));

            yield return fadeSequence.WaitForCompletion();
        }

        private Tween FadeElement(Transform element, float targetAlpha, float duration)
        {
            if (element == null) return null;

            var canvasGroup = element.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = element.gameObject.AddComponent<CanvasGroup>();

            return canvasGroup.DOFade(targetAlpha, duration).SetEase(Ease.OutQuad);
        }

        private void SetAlpha(Transform element, float alpha)
        {
            if (element == null) return;

            var canvasGroup = element.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = element.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = alpha;
        }

        private void SetFinalPositions()
        {
            // Instantly set everything to final state
            SetAlpha(header, 1f);
            SetAlpha(fadeGroup.transform, 0f);
            SetAlpha(pigCharacter, 1f);
            SetAlpha(slotMachine, 1f);
            SetAlpha(buttonPanel, 1f);

            if (header != null)
            {
                header.localPosition = titleOriginalPosition;
                header.localScale = titleOriginalScale;
            }

            EnableGameInput();
        }

        private void DisableGameInput()
        {
            // Disable interaction during intro
            backgroundGroup.blocksRaycasts = false; // Block input
            var eventBus = ServiceLocator.Instance.Get<IEventBus>();
            eventBus?.Publish(new GameStateChangedEvent(GameState.Idle, GameState.ShowingWin)); // Temporary lock
        }

        private void EnableGameInput()
        {
            // Enable interaction after intro
            backgroundGroup.blocksRaycasts = true; 
            var eventBus = ServiceLocator.Instance.Get<IEventBus>();
            eventBus?.Publish(new GameReadyEvent());
            eventBus?.Publish(new GameStateChangedEvent(GameState.ShowingWin, GameState.Idle)); // Unlock
        }

        private void OnDestroy()
        {
            // Clean up any running tweens
            DOTween.Kill(header);
            DOTween.Kill(pigCharacter);
            DOTween.Kill(slotMachine);
            DOTween.Kill(buttonPanel);
        }

        // Helper method to skip intro during development
        [ContextMenu("Skip Intro")]
        private void SkipIntro()
        {
            if (isPlayingIntro)
            {
                StopAllCoroutines();
                SetFinalPositions();
                isPlayingIntro = false;
            }
        }
    }
}