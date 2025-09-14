using UnityEngine;
using DG.Tweening;

namespace SweetSpin.Animation
{
    public class GameTitleFloater : MonoBehaviour
    {
        [Header("Float Settings")]
        [SerializeField] private float floatAmplitude = 10f;
        [SerializeField] private float floatDuration = 3f;
        [SerializeField] private AnimationCurve floatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Pulse Settings")]
        [SerializeField] private float pulseScale = 1.05f;
        [SerializeField] private float pulseDuration = 2f;

        private Vector3 originalPosition;
        private Vector3 originalScale;
        private Sequence floatSequence;
        private Sequence pulseSequence;

        public void Initialize()
        {
            originalPosition = transform.localPosition;
            originalScale = transform.localScale;
            StartFloatingAnimation();
        }

        private void StartFloatingAnimation()
        {
            // Kill any existing sequences
            floatSequence?.Kill();
            pulseSequence?.Kill();

            // Create separate sequences for floating and pulsing

            // Floating motion - using local position for UI elements
            floatSequence = DOTween.Sequence();
            floatSequence.Append(
                transform.DOLocalMoveY(originalPosition.y + floatAmplitude, floatDuration)
                    .SetEase(Ease.InOutSine)
            );
            floatSequence.Append(
                transform.DOLocalMoveY(originalPosition.y - floatAmplitude, floatDuration * 2)
                    .SetEase(Ease.InOutSine)
            );
            floatSequence.Append(
                transform.DOLocalMoveY(originalPosition.y, floatDuration)
                    .SetEase(Ease.InOutSine)
            );
            floatSequence.SetLoops(-1); // Infinite loop for floating

            // Pulsing/breathing effect - separate sequence
            pulseSequence = DOTween.Sequence();
            pulseSequence.Append(
                transform.DOScale(originalScale * pulseScale, pulseDuration)
                    .SetEase(Ease.InOutSine)
            );
            pulseSequence.Append(
                transform.DOScale(originalScale, pulseDuration)
                    .SetEase(Ease.InOutSine)
            );
            pulseSequence.SetLoops(-1); // Infinite loop for pulsing
        }


        public void StopAnimations()
        {
            floatSequence?.Pause();
            pulseSequence?.Pause();
        }

        public void ResumeAnimations()
        {
            floatSequence?.Play();
            pulseSequence?.Play();
        }

        void OnDestroy()
        {
            floatSequence?.Kill();
            pulseSequence?.Kill();
        }
    }
}