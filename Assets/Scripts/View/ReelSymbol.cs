using DG.Tweening;
using SweetSpin.VFX;
using UnityEngine;
using UnityEngine.UI;

namespace SweetSpin
{
    /// <summary>
    /// Enhanced ReelSymbol with win frame effects and sequential animations.
    /// Handles symbol display, win animations, and colored frames for multiple winning lines.
    /// </summary>
    public class ReelSymbol : MonoBehaviour
    {
        [Header("Symbol Components")]
        [SerializeField] private Image symbolImage;
        [SerializeField] private Image winFrame; // Frame that appears during wins

        [Header("Win Animation Settings")]
        [SerializeField] private float winScaleAmount = 1.3f;
        [SerializeField] private Transform particleSystemTransform;

        private SymbolType symbolType;
        private RectTransform rectTransform;
        private Sequence currentWinSequence;

        public SymbolType Type => symbolType;
        public RectTransform RectTransform => rectTransform;

        private void Awake()
        {
            if (symbolImage == null)
                symbolImage = GetComponent<Image>();

            rectTransform = GetComponent<RectTransform>();

            // Initialize win frame
            if (winFrame != null)
            {
                // Start with frame invisible
                SetFrameAlpha(0f);
                winFrame.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"ReelSymbol on {gameObject.name} is missing winFrame reference!");
            }
        }

        public void SetSymbol(SymbolType type, Sprite sprite)
        {
            symbolType = type;
            if (symbolImage != null && sprite != null)
            {
                symbolImage.sprite = sprite;
            }
        }

        public void SetSymbol(SymbolData symbolData)
        {
            SetSymbol(symbolData.type, symbolData.sprite);
        }

        /// <summary>
        /// Animate this symbol as part of a winning line with specified color
        /// </summary>
        /// <param name="frameColor">Color for the win frame</param>
        /// <param name="animationDelay">Delay before starting animation</param>
        /// <param name="animationDuration">Total duration for the win animation (from config)</param>
        public void AnimateWin(Color frameColor, float animationDelay = 0f, float animationDuration = 0.6f)
        {
            // Kill any existing animation
            currentWinSequence?.Kill();

            // Create animation sequence
            currentWinSequence = DOTween.Sequence();

            // Add delay if specified
            if (animationDelay > 0f)
            {
                currentWinSequence.AppendInterval(animationDelay);
            }

            // Show and animate the frame
            currentWinSequence.AppendCallback(() => {
                if (winFrame != null)
                {
                    winFrame.color = frameColor;
                    winFrame.gameObject.SetActive(true);
                }
            });

            // Extremely hacky way to ensure particles render above UI not to tackle horizontal/vertical layout group sorting issues.
            // Will revisit later with a more robust solution.
            Transform vfxTransform = Instantiate(particleSystemTransform, transform.parent.parent.parent.parent.parent);
            vfxTransform.position = transform.position;
            WinParticleController vfxController = vfxTransform.GetComponent<WinParticleController>();
            if (vfxController != null)
            {
                vfxController.Initialize(frameColor);
            }
            else
            {
                Debug.LogWarning("ReelSymbol: WinParticleController component missing on particleSystemTransform prefab.");
            }

            // Animate scale up and frame fade in simultaneously (30% of duration)
            currentWinSequence.Append(
                DOTween.To(() => transform.localScale, x => transform.localScale = x,
                          Vector3.one * winScaleAmount, animationDuration * 0.3f)
                .SetEase(Ease.OutBack)
            );

            currentWinSequence.Join(
                DOTween.To(() => GetFrameAlpha(), x => SetFrameAlpha(x),
                          1f, animationDuration * 0.3f)
                .SetEase(Ease.OutQuad)
            );

            // Hold the scale and frame (40% of duration)
            currentWinSequence.AppendInterval(animationDuration * 0.4f);

            // Scale back down and fade frame out (30% of duration)
            currentWinSequence.Append(
                DOTween.To(() => transform.localScale, x => transform.localScale = x,
                          Vector3.one, animationDuration * 0.3f)
                .SetEase(Ease.InBack)
            );

            currentWinSequence.Join(
                DOTween.To(() => GetFrameAlpha(), x => SetFrameAlpha(x),
                          0f, animationDuration * 0.3f)
                .SetEase(Ease.InQuad)
            );

            // Hide frame when animation completes
            currentWinSequence.AppendCallback(() => {
                if (winFrame != null)
                {
                    winFrame.gameObject.SetActive(false);
                }
            });
        }

        /// <summary>
        /// Stop any current win animation and reset to normal state
        /// </summary>
        public void StopWinAnimation()
        {
            currentWinSequence?.Kill();
            ResetToNormal();
        }

        /// <summary>
        /// Reset symbol to normal appearance
        /// </summary>
        public void ResetToNormal()
        {
            transform.localScale = Vector3.one;

            if (winFrame != null)
            {
                SetFrameAlpha(0f);
                winFrame.gameObject.SetActive(false);
            }
        }

        private float GetFrameAlpha()
        {
            return winFrame != null ? winFrame.color.a : 0f;
        }

        private void SetFrameAlpha(float alpha)
        {
            if (winFrame != null)
            {
                Color color = winFrame.color;
                color.a = alpha;
                winFrame.color = color;
            }
        }

        private void OnDestroy()
        {
            currentWinSequence?.Kill();
        }
    }
}