using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SweetSpin
{
    /// <summary>
    /// Represents a single symbol on a reel.
    /// Handles symbol display and win animations.
    /// Attached to each symbol prefab instance.
    /// </summary>
    public class ReelSymbol : MonoBehaviour
    {
        [SerializeField] private Image symbolImage;
        private SymbolType symbolType;
        private RectTransform rectTransform;

        public SymbolType Type => symbolType;
        public RectTransform RectTransform => rectTransform;

        private void Awake()
        {
            if (symbolImage == null)
                symbolImage = GetComponent<Image>();

            rectTransform = GetComponent<RectTransform>();
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

        // Animation helpers
        public void AnimateWin()
        {
            transform.DOScale(1.2f, 0.2f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutQuad);
        }

        public void ResetScale()
        {
            transform.localScale = Vector3.one;
        }
    }
}