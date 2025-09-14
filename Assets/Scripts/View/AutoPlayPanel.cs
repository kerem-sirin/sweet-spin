using DG.Tweening;
using SweetSpin.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SweetSpin
{
    /// <summary>
    /// UI panel for selecting auto-play spin count
    /// </summary>
    public class AutoPlayPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button spin3Button;
        [SerializeField] private Button spin7Button;
        [SerializeField] private Button spin15Button;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backgroundAsButton;

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.3f;

        private IAutoPlayService autoPlayService;
        private IEventBus eventBus;

        private void Start()
        {
            autoPlayService = ServiceLocator.Instance.Get<IAutoPlayService>();
            eventBus = ServiceLocator.Instance.Get<IEventBus>();

            // Setup button listeners
            if (spin3Button != null)
            {
                spin3Button.onClick.AddListener(() => SelectSpinCount(3));
            }

            if (spin7Button != null)
            {
                spin7Button.onClick.AddListener(() => SelectSpinCount(7));
            }

            if (spin15Button != null)
            {
                spin15Button.onClick.AddListener(() => SelectSpinCount(15));
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            if (backgroundAsButton != null)
            {
                backgroundAsButton.onClick.AddListener(Hide);
            }

            // Hide panel initially
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            spin3Button?.onClick.RemoveAllListeners();
            spin7Button?.onClick.RemoveAllListeners();
            spin15Button?.onClick.RemoveAllListeners();
            closeButton?.onClick.RemoveAllListeners();
        }

        public void Show()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);

                // Animate in
                panelRoot.transform.localScale = Vector3.zero;
                panelRoot.transform.DOScale(Vector3.one, animationDuration)
                    .SetEase(Ease.OutBack);
            }
        }

        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.transform.DOScale(Vector3.zero, animationDuration * 0.7f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => panelRoot.SetActive(false));
            }
        }

        private void SelectSpinCount(int count)
        {
            // Start auto-play through service
            autoPlayService?.StartAutoPlay(count);

            // Close panel
            Hide();
        }
    }
}