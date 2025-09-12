using DG.Tweening;
using System.Collections.Generic;
using TMPro;
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

        [Header("Settings")]
        [SerializeField] private float animationDuration = 0.3f;

        private SymbolData[] symbolDatabase;
        private List<GameObject> payoutEntries = new List<GameObject>();
        private bool isShowing = false;

        // Standard payout structure for the slot machine
        private readonly PayoutInfo[] payoutStructure = new PayoutInfo[]
        {
        new PayoutInfo(SymbolType.Cherry, new int[] { 5, 10, 20 }, "Cherry", new Color(1f, 0.2f, 0.2f)),
        new PayoutInfo(SymbolType.Lemon, new int[] { 10, 20, 40 }, "Lemon", new Color(1f, 1f, 0.2f)),
        new PayoutInfo(SymbolType.Orange, new int[] { 15, 30, 60 }, "Orange", new Color(1f, 0.6f, 0.1f)),
        new PayoutInfo(SymbolType.Plum, new int[] { 20, 40, 80 }, "Plum", new Color(0.6f, 0.2f, 0.8f)),
        new PayoutInfo(SymbolType.Bell, new int[] { 25, 50, 100 }, "Bell", new Color(1f, 0.9f, 0.1f)),
        new PayoutInfo(SymbolType.Bar, new int[] { 30, 60, 150 }, "BAR", new Color(0.2f, 0.2f, 0.8f)),
        new PayoutInfo(SymbolType.Seven, new int[] { 50, 100, 250 }, "777", new Color(1f, 0.1f, 0.1f)),
        new PayoutInfo(SymbolType.Wild, new int[] { 100, 200, 500 }, "WILD", new Color(0.9f, 0.1f, 0.9f))
        };

        [System.Serializable]
        private class PayoutInfo
        {
            public SymbolType type;
            public int[] payouts;
            public string displayName;
            public Color symbolColor;

            public PayoutInfo(SymbolType symbolType, int[] payoutValues, string name, Color color)
            {
                type = symbolType;
                payouts = payoutValues;
                displayName = name;
                symbolColor = color;
            }
        }

        private void Start()
        {
            if (togglePaytableButton != null)
                togglePaytableButton.onClick.AddListener(TogglePaytable);

            if (closePaytableButton != null)
                closePaytableButton.onClick.AddListener(HidePaytable);

            if (paytablePanel != null)
                paytablePanel.SetActive(false);

            CreatePaytableEntries();
        }

        public void Initialize(SymbolData[] symbols)
        {
            symbolDatabase = symbols;
            UpdatePaytableDisplay();
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

            // Create header
            GameObject header = CreateHeaderEntry();
            payoutEntries.Add(header);

            // Create entry for each symbol
            foreach (var payoutInfo in payoutStructure)
            {
                GameObject entry = CreatePayoutEntry(payoutInfo);
                payoutEntries.Add(entry);
            }

            // Add special rules text
            GameObject rulesEntry = CreateRulesEntry();
            payoutEntries.Add(rulesEntry);
        }

        private GameObject CreateHeaderEntry()
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(payoutContainer);

            RectTransform rt = header.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 60);
            rt.localScale = Vector3.one;

            // Add layout component
            HorizontalLayoutGroup layout = header.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 0;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.padding = new RectOffset(100, 50, 10, 10);

            // Add column headers
            CreateText(header, "SYMBOL", 150, TextAlignmentOptions.Center, true);
            CreateText(header, "3x", 100, TextAlignmentOptions.Center, true);
            CreateText(header, "4x", 100, TextAlignmentOptions.Center, true);
            CreateText(header, "5x", 100, TextAlignmentOptions.Center, true);

            return header;
        }

        private GameObject CreatePayoutEntry(PayoutInfo payoutInfo)
        {
            GameObject entry = new GameObject($"Payout_{payoutInfo.displayName}");
            entry.transform.SetParent(payoutContainer);

            RectTransform rt = entry.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 50);
            rt.localScale = Vector3.one;

            // Add background
            Image bg = entry.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.3f);

            // Add layout
            HorizontalLayoutGroup layout = entry.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 50;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.padding = new RectOffset(100, 50, 5, 5);

            // Symbol name with color
            var symbolText = CreateText(entry, payoutInfo.displayName, 150, TextAlignmentOptions.Center, false);
            symbolText.color = payoutInfo.symbolColor;
            symbolText.fontStyle = FontStyles.Bold;

            // Payout values
            foreach (int payout in payoutInfo.payouts)
            {
                var payoutText = CreateText(entry, payout.ToString(), 100, TextAlignmentOptions.Center, false);
                payoutText.color = Color.yellow;
            }

            return entry;
        }

        private GameObject CreateRulesEntry()
        {
            GameObject rules = new GameObject("Rules");
            rules.transform.SetParent(payoutContainer);

            RectTransform rt = rules.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(800, 100);
            rt.localScale = Vector3.one;

            TextMeshProUGUI rulesText = rules.AddComponent<TextMeshProUGUI>();
            rulesText.text = "\n<b>GAME RULES:</b>\n" +
                            "• WILD substitutes for all symbols\n" +
                            "• Wins pay left to right on active paylines\n" +
                            "• All wins multiplied by bet per line";
            rulesText.fontSize = 16;
            rulesText.alignment = TextAlignmentOptions.Center;
            rulesText.color = new Color(0.8f, 0.8f, 0.8f);

            return rules;
        }

        private TextMeshProUGUI CreateText(GameObject parent, string text, float width,
                                           TextAlignmentOptions alignment, bool isHeader)
        {
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(parent.transform);

            RectTransform rt = textGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, 40);
            rt.localScale = Vector3.one;

            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = isHeader ? 20 : 18;
            tmp.alignment = alignment;
            tmp.color = isHeader ? Color.cyan : Color.white;
            if (isHeader)
                tmp.fontStyle = FontStyles.Bold | FontStyles.Underline;

            return tmp;
        }

        private void UpdatePaytableDisplay()
        {
            // Update the display if symbol database changes
            // This could be expanded to show actual sprites instead of text
        }

        public void TogglePaytable()
        {
            if (isShowing)
                HidePaytable();
            else
                ShowPaytable();
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

        // Method to highlight a specific symbol's payouts (useful when showing wins)
        public void HighlightSymbol(SymbolType symbolType)
        {
            for (int i = 0; i < payoutStructure.Length; i++)
            {
                if (payoutStructure[i].type == symbolType && i + 1 < payoutEntries.Count)
                {
                    GameObject entry = payoutEntries[i + 1]; // +1 to skip header
                    entry.transform.DOScale(1.1f, 0.2f).SetLoops(2, LoopType.Yoyo);

                    Image bg = entry.GetComponent<Image>();
                    if (bg != null)
                    {
                        bg.DOColor(new Color(1f, 1f, 0f, 0.5f), 0.2f).SetLoops(2, LoopType.Yoyo);
                    }
                }
            }
        }
    }
}
