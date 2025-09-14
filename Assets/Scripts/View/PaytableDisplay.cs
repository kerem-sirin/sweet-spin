using DG.Tweening;
using SweetSpin.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SweetSpin
{
    public class PaytableDisplay : MonoBehaviour
    {
        [Header("Paytable UI")]
        [SerializeField] private GameObject paytablePanel;
        [SerializeField] private Transform payoutContainer;
        [SerializeField] private Button togglePaytableButton;
        [SerializeField] private Button closePaytableButton;
        [SerializeField] private Button backgroundAsButton;

        [Header("Prefabs")]
        [SerializeField] private GameObject paytableEntryPrefab;

        [Header("Settings")]
        [SerializeField] private float animationDuration = 0.3f;

        private IAudioService audioService;
        private SlotMachineConfiguration configuration;
        private List<GameObject> payoutEntries = new List<GameObject>();
        private bool isShowing = false;

        private void Start()
        {
            // Get audio service
            audioService = ServiceLocator.Instance.Get<IAudioService>();

            if (togglePaytableButton != null)
            {
                togglePaytableButton.onClick.AddListener(() => {
                    audioService?.PlayButtonClick();
                    TogglePaytable();
                });
            }

            if (closePaytableButton != null)
            {
                closePaytableButton.onClick.AddListener(() => {
                    audioService?.PlayButtonClick();
                    HidePaytable();
                });
            }

            if (backgroundAsButton != null)
            {
                backgroundAsButton.onClick.AddListener(() => {
                    audioService?.PlayButtonClick();
                    HidePaytable();
                });
            }

            if (paytablePanel != null)
            {
                paytablePanel.SetActive(false);
            }
        }

        public void Initialize(SlotMachineConfiguration config)
        {
            configuration = config;
            CreatePaytableEntries();
        }

        private void OnDestroy()
        {
            if (togglePaytableButton != null)
            {
                togglePaytableButton.onClick.RemoveListener(TogglePaytable);
            }


            if (closePaytableButton != null)
            {
                closePaytableButton.onClick.RemoveListener(HidePaytable);
            }

            if (backgroundAsButton != null)
            {
                backgroundAsButton.onClick.RemoveListener(HidePaytable);
            }
        }

        private void CreatePaytableEntries()
        {
            // Clear existing entries
            foreach (var entry in payoutEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            payoutEntries.Clear();

            if (configuration == null || configuration.symbolDatabase == null)
            {
                Debug.LogWarning("PaytableDisplay: No configuration or symbol database found");
                return;
            }

            if (paytableEntryPrefab == null)
            {
                Debug.LogError("PaytableDisplay: Entry prefab not assigned!");
                return;
            }

            // Create entry for each symbol from the database
            foreach (var symbolData in configuration.symbolDatabase)
            {
                GameObject entryGO = Instantiate(paytableEntryPrefab, payoutContainer);
                PaytableEntry entry = entryGO.GetComponent<PaytableEntry>();

                if (entry != null)
                {
                    entry.SetData(symbolData);
                    payoutEntries.Add(entryGO);
                }
            }
        }

        public void TogglePaytable()
        {
            if (isShowing)
            {
                HidePaytable();
            }

            else
            {
                ShowPaytable();
            }
        }

        public void ShowPaytable()
        {
            if (paytablePanel == null || isShowing) return;

            isShowing = true;
            paytablePanel.SetActive(true);

            // Animate panel appearance
            paytablePanel.transform.localScale = Vector3.zero;
            paytablePanel.transform.DOScale(Vector3.one, animationDuration)
                .SetEase(Ease.OutBack);

            // Fade in background
            CanvasGroup cg = paytablePanel.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = paytablePanel.AddComponent<CanvasGroup>();

            cg.alpha = 0;
            cg.DOFade(1, animationDuration);
        }

        public void HidePaytable()
        {
            if (paytablePanel == null || !isShowing) return;

            isShowing = false;

            // Animate panel disappearance
            paytablePanel.transform.DOScale(Vector3.zero, animationDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => paytablePanel.SetActive(false));

            CanvasGroup cg = paytablePanel.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.DOFade(0, animationDuration);
        }
    }
}