using UnityEngine;
using DG.Tweening;
using SweetSpin.Core;

namespace SweetSpin.Animation
{
    public class PigCharacterAnimator : MonoBehaviour
    {
        [Header("Pig Parts")]
        [SerializeField] private Transform pigHead;
        [SerializeField] private Transform pigBody;
        [SerializeField] private Transform pigTail;

        [Header("Idle Animation Settings")]
        [SerializeField] private float headRotationAngle = 5f;
        [SerializeField] private float headRotationDuration = 3f;
        [SerializeField] private float bodyRotationAngle = 3f;
        [SerializeField] private float bodyRotationDuration = 4f;
        [SerializeField] private float tailWagAngle = 15f;
        [SerializeField] private float tailWagDuration = 1f;
        [SerializeField] private float tailScaleVariation = 0.1f;

        [Header("Win Animation Settings")]
        [SerializeField] private bool enableWinAnimation = true;
        [SerializeField] private float winJumpHeight = 30f;
        [SerializeField] private float winDuration = 0.5f;

        private Sequence headSequence;
        private Sequence bodySequence;
        private Sequence tailSequence;
        private bool isPlayingWinAnimation = false;
        private IEventBus eventBus;

        void Start()
        {
            StartIdleAnimations();
            SubscribeToEvents();
        }

        private void StartIdleAnimations()
        {
            // Kill any existing sequences first
            headSequence?.Kill();
            bodySequence?.Kill();
            tailSequence?.Kill();

            HeadAnimation();
            BodyAnimation();
            TailAnimation();
        }

        private void TailAnimation()
        {
            // Kill existing before creating new
            tailSequence?.Kill();

            // Tail wagging with scale
            tailSequence = DOTween.Sequence();
            tailSequence.Append(
                pigTail.DORotate(new Vector3(0, 0, tailWagAngle), tailWagDuration)
                    .SetEase(Ease.InOutQuad)
            );
            tailSequence.Append(
                pigTail.DORotate(new Vector3(0, 0, -tailWagAngle), tailWagDuration)
                    .SetEase(Ease.InOutQuad)
            );
            tailSequence.Join(
                pigTail.DOScale(1f + tailScaleVariation, tailWagDuration)
                    .SetEase(Ease.InOutQuad)
                    .SetLoops(2, LoopType.Yoyo)
            );
            tailSequence.SetLoops(-1); // Infinite loop on the sequence itself
        }

        private void BodyAnimation()
        {
            // Kill existing before creating new
            bodySequence?.Kill();

            // Body breathing - yoyo loop
            bodySequence = DOTween.Sequence();
            bodySequence.Append(
                pigBody.DORotate(new Vector3(0, 0, bodyRotationAngle), bodyRotationDuration)
                    .SetEase(Ease.InOutQuad)
            );
            bodySequence.Append(
                pigBody.DORotate(new Vector3(0, 0, -bodyRotationAngle), bodyRotationDuration)
                    .SetEase(Ease.InOutQuad)
            );
            bodySequence.SetLoops(-1); // Infinite loop on the sequence itself
        }

        private void HeadAnimation()
        {
            // Kill existing before creating new
            headSequence?.Kill();

            // Head looking around
            headSequence = DOTween.Sequence();
            headSequence.Append(
                pigHead.DORotate(new Vector3(0, 0, headRotationAngle), headRotationDuration)
                    .SetEase(Ease.InOutSine)
            );
            headSequence.Append(
                pigHead.DORotate(new Vector3(0, 0, -headRotationAngle), headRotationDuration * 2)
                    .SetEase(Ease.InOutSine)
            );
            headSequence.Append(
                pigHead.DORotate(Vector3.zero, headRotationDuration)
                    .SetEase(Ease.InOutSine)
            );
            headSequence.SetLoops(-1); // Infinite loop on the sequence itself
        }

        public void PlayWinAnimation(WinTier tier)
        {
            if (!enableWinAnimation || isPlayingWinAnimation) return;

            isPlayingWinAnimation = true;

            // Kill all idle animations (not just pause)
            headSequence?.Kill();
            bodySequence?.Kill();
            tailSequence?.Kill();

            // Reset rotations and scale before win animation
            pigHead.DORotate(Vector3.zero, 0.2f);
            pigBody.DORotate(Vector3.zero, 0.2f);
            pigTail.DORotate(Vector3.zero, 0.2f);
            pigTail.DOScale(Vector3.one, 0.2f);

            // Create win animation based on tier
            float intensity = GetWinIntensity(tier);

            Sequence winSeq = DOTween.Sequence();

            // Add a small delay for rotation reset
            winSeq.AppendInterval(0.2f);

            // Happy jump
            winSeq.Append(pigBody.DOLocalMoveY(
                pigBody.localPosition.y + winJumpHeight * intensity,
                winDuration * 0.5f
            ).SetEase(Ease.OutQuad));

            winSeq.Append(pigBody.DOLocalMoveY(
                pigBody.localPosition.y,
                winDuration * 0.5f
            ).SetEase(Ease.InBounce));

            // Head excitement
            winSeq.Join(pigHead.DORotate(new Vector3(0, 0, 360), winDuration, RotateMode.FastBeyond360));

            // Tail super wag
            winSeq.Join(pigTail.DORotate(new Vector3(0, 0, 30), 0.1f)
                .SetEase(Ease.InOutQuad)
                .SetLoops(5, LoopType.Yoyo));

            winSeq.OnComplete(() => {
                isPlayingWinAnimation = false;
                // Restart idle animations
                StartIdleAnimations();
            });
        }

        private float GetWinIntensity(WinTier tier) => tier switch
        {
            WinTier.Jackpot => 2f,
            WinTier.Mega => 1.5f,
            WinTier.Big => 1.2f,
            _ => 1f
        };

        private WinTier DetermineWinTier(SpinResult result)
        {
            if (!result.IsWin) return WinTier.None;

            float multiplier = result.GetWinMultiplier();

            if (multiplier >= 50f) return WinTier.Jackpot;
            if (multiplier >= 25f) return WinTier.Mega;
            if (multiplier >= 10f) return WinTier.Big;
            if (multiplier >= 5f) return WinTier.Medium;
            if (multiplier > 0) return WinTier.Small;

            return WinTier.None;
        }

        private void SubscribeToEvents()
        {
            eventBus = ServiceLocator.Instance.Get<IEventBus>();
            eventBus?.Subscribe<SpinCompletedEvent>(OnSpinCompleted);
        }

        private void OnSpinCompleted(SpinCompletedEvent e)
        {
            if (e.Result.IsWin)
            {
                var tier = DetermineWinTier(e.Result);
                if (tier >= WinTier.Medium) // Only animate for medium wins or higher
                {
                    PlayWinAnimation(tier);
                }
            }
        }

        void OnDestroy()
        {
            headSequence?.Kill();
            bodySequence?.Kill();
            tailSequence?.Kill();
            eventBus?.Unsubscribe<SpinCompletedEvent>(OnSpinCompleted);
        }
    }
}