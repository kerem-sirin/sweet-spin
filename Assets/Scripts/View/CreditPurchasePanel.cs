using DG.Tweening;
using SweetSpin.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SweetSpin
{
    public class CreditPurchasePanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button add100Button;
        [SerializeField] private Button add1000Button;
        [SerializeField] private Button add10000Button;
        [SerializeField] private Button closeButton;

        [Header("Settings")]
        [SerializeField] private float animationDuration = 0.3f;

        private void Start()
        {
            // Setup button listeners
            if (add100Button != null)
                add100Button.onClick.AddListener(() => SelectAmount(100));

            if (add1000Button != null)
                add1000Button.onClick.AddListener(() => SelectAmount(1000));

            if (add10000Button != null)
                add10000Button.onClick.AddListener(() => SelectAmount(10000));

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            // Hide panel initially
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            // Clean up listeners
            add100Button?.onClick.RemoveAllListeners();
            add1000Button?.onClick.RemoveAllListeners();
            add10000Button?.onClick.RemoveAllListeners();
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
                panelRoot.transform.DOScale(Vector3.zero, animationDuration)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        panelRoot.SetActive(false);

                    });
            }
        }

        private void SelectAmount(int amount)
        {
            // Visual feedback on button
            Button selectedButton = amount switch
            {
                100 => add100Button,
                1000 => add1000Button,
                10000 => add10000Button,
                _ => null
            };

            if (selectedButton != null)
            {
                selectedButton.transform.DOScale(1.1f, 0.1f)
                    .SetLoops(2, LoopType.Yoyo)
                    .OnComplete(()=>
                    {
                        OnAccept(amount);
                    });
            }
        }

        private void OnAccept(int amount)
        {
            // Publish event 
            ServiceLocator.Instance.Get<IEventBus>().Publish(new AddCreditsRequestEvent(amount));
            Hide();
        }
    }
}