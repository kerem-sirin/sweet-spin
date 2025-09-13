using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem winParticles;
        [SerializeField] private GameObject winLineRenderer;

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
        /// Called by GameController after instantiation
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
                controller.Initialize(i, configuration.symbolDatabase, configuration.spinSpeed, configuration.spinDuration, configuration.reelStopDelay);
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
                eventBus.Subscribe<SpinCompletedEvent>(OnSpinCompleted);
                eventBus.Subscribe<CreditsChangedEvent>(OnCreditsChanged);
            }
        }

        /// <summary>
        /// Start spinning reels with the given results
        /// </summary>
        public void SpinReels(SymbolType[,] results)
        {
            for (int i = 0; i < reel.Length; i++)
            {
                SymbolType[] reelSymbols = new SymbolType[configuration.rowCount];
                for (int j = 0; j < configuration.rowCount; j++)
                {
                    reelSymbols[j] = results[i, j];
                }

                float delay = i * configuration.reelStopDelay;
                reel[i].Spin(reelSymbols, delay);
            }
        }

        /// <summary>
        /// Update UI text displays
        /// </summary>
        public void UpdateUI(int credits, int bet)
        {
            if (creditsText != null)
            {
                creditsText.text = credits.ToString();
            }

            if (betText != null)
            {
                betText.text = bet.ToString();
            }
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
                    AnimateJackpot();
                    break;
                case WinTier.Mega:
                    winText.color = new Color(1f, 0f, 1f); // Magenta
                    AnimateMegaWin();
                    break;
                case WinTier.Big:
                    winText.color = new Color(0f, 1f, 1f); // Cyan
                    AnimateBigWin();
                    break;
                case WinTier.Small:
                case WinTier.Medium:
                    winText.color = new Color(0f, 1f, 0f); // Green
                    AnimateWin();
                    break;
                default:
                    winText.color = Color.white;
                    break;
            }
        }

        /// <summary>
        /// Animate winning symbols
        /// </summary>
        public void AnimateWinningLine(PaylineWin win)
        {
            for (int i = 0; i < win.matchCount; i++)
            {
                if (i < reel.Length)
                {
                    reel[i].AnimateSymbolAt(win.positions[i]);
                }
            }

            // TODO: Could also draw line here if we have LineRenderer setup
            // DrawWinLine(win.positions);
        }

        // Visual effect methods
        private void AnimateWin()
        {
            winText.transform.DOScale(1.1f, 0.3f)
                .SetLoops(2, LoopType.Yoyo);

            if (winParticles != null)
                winParticles.Play();
        }

        private void AnimateBigWin()
        {
            winText.transform.DOScale(1.2f, 0.3f)
                .SetLoops(2, LoopType.Yoyo);

            if (winParticles != null)
            {
                winParticles.Play();
                // Could modify particle settings for bigger effect
            }
        }

        private void AnimateMegaWin()
        {
            winText.transform.DOScale(1.3f, 0.4f)
                .SetLoops(3, LoopType.Yoyo)
                .SetEase(Ease.OutBounce);

            if (winParticles != null)
            {
                winParticles.Play();
            }
        }

        private void AnimateJackpot()
        {
            winText.transform.DOScale(1.5f, 0.5f)
                .SetLoops(4, LoopType.Yoyo)
                .SetEase(Ease.OutElastic);

            if (winParticles != null)
            {
                winParticles.Play();
            }

            // Could add screen shake or other dramatic effects
        }

        // Event handlers
        private void OnSpinStarted(SpinStartedEvent e)
        {
            if (spinButton != null)
            {
                spinButton.interactable = false;
            }

            if (winText != null)
            {
                winText.text = "Spinning...";
            }
        }

        private void OnSpinCompleted(SpinCompletedEvent e)
        {
            if (spinButton != null)
            {
                spinButton.interactable = true;
            }
        }

        private void OnCreditsChanged(CreditsChangedEvent e)
        {
            // Could add credit count animation here
            if (creditsText != null)
            {
                // Simple update for now
                creditsText.text = e.NewCredits.ToString();
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (eventBus != null)
            {
                eventBus.Unsubscribe<SpinStartedEvent>(OnSpinStarted);
                eventBus.Unsubscribe<SpinCompletedEvent>(OnSpinCompleted);
                eventBus.Unsubscribe<CreditsChangedEvent>(OnCreditsChanged);
            }
        }
    }
}