using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.STP;

namespace SweetSpin.Core
{
    /// <summary>
    /// Manages the visual representation of the slot machine
    /// Responsible for creating and managing reels, UI updates, and visual effects
    /// </summary>
    public class SlotMachineView : MonoBehaviour
    {
        [Header("Reel Setup")]
        [SerializeField] private GameObject reelPrefab;
        [SerializeField] private Transform reelContainer;
        [SerializeField] private float reelSpacing = 160f;

        [Header("UI References")]
        [SerializeField] private Button spinButton;
        [SerializeField] private Button increaseBetButton;
        [SerializeField] private Button decreaseBetButton;
        [SerializeField] private TextMeshProUGUI creditsText;
        [SerializeField] private TextMeshProUGUI betText;
        [SerializeField] private TextMeshProUGUI winText;

        [Header("Additional UI")]
        [SerializeField] private PaytableDisplay paytableDisplay;
        [SerializeField] private CreditPurchasePanel creditPurchasePanel;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem winParticles;

        private Reel[] reel;
        private SlotMachineConfiguration configuration;
        private ISymbolService symbolService;
        private IEventBus eventBus;

        // Public properties for GameController to access
        public Button SpinButton => spinButton;
        public Button IncreaseBetButton => increaseBetButton;
        public Button DecreaseBetButton => decreaseBetButton;
        public Reel[] Reels => reel;

        /// <summary>
        /// Initialize the view with required dependencies
        /// </summary>
        public void Initialize(SlotMachineConfiguration config, ISymbolService symbols, IEventBus events)
        {
            configuration = config;
            symbolService = symbols;
            eventBus = events;

            CreateReels();
            SetupEventListeners();

            if (paytableDisplay != null)
            {
                paytableDisplay.Initialize(configuration);
            }

            winText.text = "";
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (eventBus != null)
            {
                eventBus.Unsubscribe<SpinStartedEvent>(OnSpinStarted);
                eventBus.Unsubscribe<CreditsChangedEvent>(OnCreditsChanged);
            }
        }

        private void CreateReels()
        {
            if (reelContainer == null)
            {
                Debug.LogError("ReelContainer is not assigned!");
                return;
            }

            reel = new Reel[configuration.reelCount];

            for (int i = 0; i < configuration.reelCount; i++)
            {
                GameObject reelGO = Instantiate(reelPrefab, reelContainer);
                RectTransform rt = reelGO.GetComponent<RectTransform>();

                // Calculate position to center the reels
                float xPos = i * reelSpacing - ((configuration.reelCount - 1) * reelSpacing / 2f);
                rt.anchoredPosition = new Vector2(xPos, 0);

                // Get or add ReelController
                Reel controller = reelGO.GetComponent<Reel>();
                if (controller == null)
                    controller = reelGO.AddComponent<Reel>();

                // Initialize the controller
                controller.Initialize(
                    i,
                    configuration.symbolDatabase,
                    configuration.spinSpeed,
                    configuration.spinDuration,
                    configuration.reelStopDelay,
                    configuration.snapDuration,
                    configuration.turboSnapDuration
                );
                reel[i] = controller;
            }

            Debug.Log($"Created {reel.Length} reels");
        }

        private void SetupEventListeners()
        {
            // Subscribe to game events for visual updates
            if (eventBus != null)
            {
                eventBus.Subscribe<SpinStartedEvent>(OnSpinStarted);
                eventBus.Subscribe<CreditsChangedEvent>(OnCreditsChanged);
                eventBus.Subscribe<InsufficientCreditsEvent>(OnInsufficientCredits);
            }
        }

        private void OnInsufficientCredits(InsufficientCreditsEvent e)
        {
            if (creditPurchasePanel != null)
            {
                creditPurchasePanel.Show();
            }
        }

        /// <summary>
        /// Start spinning reels with the given results
        /// </summary>
        public void SpinReels(SymbolType[,] results, float spinSpeed, float spinDuration, float reelStopDelay)
        {
            for (int i = 0; i < reel.Length; i++)
            {
                reel[i].ClearWinAnimationsInstant();

                SymbolType[] reelSymbols = new SymbolType[configuration.rowCount];
                for (int j = 0; j < configuration.rowCount; j++)
                {
                    reelSymbols[j] = results[i, j];
                }

                float delay = i * reelStopDelay;
                reel[i].Spin(reelSymbols, spinSpeed, spinDuration, delay);
            }
        }

        /// <summary>
        /// Update UI text displays
        /// </summary>
        public void UpdateUI(int credits, int bet)
        {
            if (creditsText != null)
                creditsText.text = credits.ToString();

            if (betText != null)
                betText.text = bet.ToString();
        }

        /// <summary>
        /// Display win message with appropriate effects
        /// </summary>
        public void ShowWinMessage(string message, WinTier tier)
        {
            if (winText == null) return;

            winText.text = message;

            // Color based on tier
            switch (tier)
            {
                case WinTier.Jackpot:
                    winText.color = new Color(1f, 0.84f, 0f); // Gold
                    break;
                case WinTier.Mega:
                    winText.color = new Color(1f, 0f, 1f); // Magenta
                    break;
                case WinTier.Big:
                    winText.color = new Color(0f, 1f, 1f); // Cyan
                    break;
                case WinTier.Medium:
                    winText.color = new Color(0f, 1f, 0f); // Green
                    break;
                case WinTier.Small:
                    winText.color = Color.white;
                    break;
                default:
                    winText.color = Color.white;
                    break;
            }
        }

        /// <summary>
        /// Animate multiple winning lines sequentially with different colors
        /// </summary>
        public void AnimateMultipleWinningLines(List<PaylineWin> wins, bool isTurboMode = false)
        {
            if (wins == null || wins.Count == 0) return;

            StartCoroutine(PlaySequentialWinAnimations(wins, isTurboMode));
        }

        /// <summary>
        /// Coroutine version for proper timing control from GameController
        /// </summary>
        public IEnumerator AnimateMultipleWinningLinesCoroutine(List<PaylineWin> wins, bool isTurboMode = false)
        {
            if (wins == null || wins.Count == 0) yield break;

            yield return StartCoroutine(PlaySequentialWinAnimations(wins, isTurboMode));
        }

        /// <summary>
        /// Coroutine to play win animations sequentially with different colors
        /// </summary>
        private IEnumerator PlaySequentialWinAnimations(List<PaylineWin> wins, bool isTurboMode)
        {
            // Get the appropriate delay/duration based on mode
            float animationDuration = isTurboMode ? configuration.turboSequentialDelay : configuration.sequentialAnimationDelay;

            for (int lineIndex = 0; lineIndex < wins.Count; lineIndex++)
            {
                var win = wins[lineIndex];

                // Get color from configuration
                Color frameColor = configuration.GetWinFrameColor(lineIndex);

                // Animate all symbols in this winning line with the same color
                for (int symbolIndex = 0; symbolIndex < win.matchCount; symbolIndex++)
                {
                    if (symbolIndex < reel.Length)
                    {
                        // Pass the color and animation duration from config
                        reel[symbolIndex].AnimateSymbolAt(
                            win.positions[symbolIndex],
                            frameColor,
                            0f,
                            animationDuration
                        );
                    }
                }

                // Wait for the animation to complete before starting the next line
                // (unless it's the last one)
                if (lineIndex < wins.Count - 1)
                {
                    yield return new WaitForSeconds(animationDuration);
                }
            }
        }

        /// <summary>
        /// Clear all win animations from all symbols
        /// </summary>
        public void ClearAllWinAnimations()
        {
            if (reel == null) return;

            foreach (var reelController in reel)
            {
                if (reelController != null)
                {
                    reelController.ClearWinAnimations();
                }
            }
        }

        // Event handlers
        private void OnSpinStarted(SpinStartedEvent e)
        {
            if (winText != null)
            {
                winText.text = "Spinning...";
                winText.color = Color.white;
            }

            // Clear any existing win animations
            ClearAllWinAnimations();
        }

        private void OnCreditsChanged(CreditsChangedEvent e)
        {
            if (creditsText != null)
            {
                creditsText.text = e.NewCredits.ToString();
            }
        }
    }
}