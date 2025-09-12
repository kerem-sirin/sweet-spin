using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SweetSpin.Core
{
    /// <summary>
    /// Main game controller - Entry point for the slot machine game
    /// Uses Service Locator pattern for dependency management
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private SlotMachineConfiguration configuration;

        [Header("View Components")]
        [SerializeField] private Transform reelContainer;
        [SerializeField] private GameObject reelPrefab;

        [Header("UI Components")]
        [SerializeField] private Button spinButton;
        [SerializeField] private Button increaseBetButton;
        [SerializeField] private Button decreaseBetButton;
        [SerializeField] private TextMeshProUGUI creditsText;
        [SerializeField] private TextMeshProUGUI betText;
        [SerializeField] private TextMeshProUGUI winText;
        [SerializeField] private TextMeshProUGUI linesText;

        // Services
        private IEventBus eventBus;
        private IAudioService audioService;
        private ISaveService saveService;
        private IRandomService randomService;
        private IPaylineService paylineService;
        private ISymbolService symbolService;

        // Game components
        private SlotMachineModel gameModel;
        private ReelController[] reelControllers;
        private GameStateMachine stateMachine;

        private void Start()
        {
            InitializeServices();
            InitializeGame();
            SubscribeToEvents();
            SetupUI();
        }

        private void InitializeServices()
        {
            // Get services from Service Locator
            eventBus = ServiceLocator.Instance.Get<IEventBus>();
            audioService = ServiceLocator.Instance.Get<IAudioService>();
            saveService = ServiceLocator.Instance.Get<ISaveService>();
            randomService = ServiceLocator.Instance.Get<IRandomService>();
            paylineService = ServiceLocator.Instance.Get<IPaylineService>();
            symbolService = ServiceLocator.Instance.Get<ISymbolService>();

            // Initialize services with configuration
            symbolService.Initialize(configuration.symbolDatabase);
            paylineService.Initialize(configuration.paylinePatterns);

            // Register additional runtime services
            ServiceLocator.Instance.Register<IPaylineService>(new PaylineService());
            var paylineServiceImpl = ServiceLocator.Instance.Get<IPaylineService>() as PaylineService;
            paylineServiceImpl?.Initialize(configuration.symbolDatabase);
        }

        private void InitializeGame()
        {
            // Create game model
            gameModel = new SlotMachineModel(configuration, randomService);

            // Load saved credits
            int savedCredits = saveService.LoadCredits();
            gameModel.SetCredits(savedCredits > 0 ? savedCredits : configuration.startingCredits);

            // Create reels
            CreateReels();

            // Initialize state machine
            stateMachine = new GameStateMachine(gameModel, reelControllers, eventBus);
        }

        private void CreateReels()
        {
            reelControllers = new ReelController[configuration.reelCount];

            for (int i = 0; i < configuration.reelCount; i++)
            {
                GameObject reelGO = Instantiate(reelPrefab, reelContainer);
                RectTransform rt = reelGO.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(i * configuration.reelSpacing - (2 * configuration.reelSpacing), 0);

                ReelController controller = reelGO.GetComponent<ReelController>();
                if (controller == null)
                    controller = reelGO.AddComponent<ReelController>();

                controller.Initialize(i, configuration.symbolDatabase, symbolService);
                reelControllers[i] = controller;
            }
        }

        private void SubscribeToEvents()
        {
            eventBus.Subscribe<SpinStartedEvent>(OnSpinStarted);
            eventBus.Subscribe<SpinCompletedEvent>(OnSpinCompleted);
            eventBus.Subscribe<CreditsChangedEvent>(OnCreditsChanged);
            eventBus.Subscribe<ReelStoppedEvent>(OnReelStopped);
        }

        private void SetupUI()
        {
            spinButton.onClick.AddListener(OnSpinButtonClick);

            if (increaseBetButton != null)
                increaseBetButton.onClick.AddListener(() => ChangeBet(1));

            if (decreaseBetButton != null)
                decreaseBetButton.onClick.AddListener(() => ChangeBet(-1));

            UpdateUI();
        }

        private void OnSpinButtonClick()
        {
            audioService.PlayButtonClick();

            if (stateMachine.CurrentState != GameState.Idle)
            {
                Debug.Log("Cannot spin - game not idle");
                return;
            }

            if (!gameModel.CanSpin())
            {
                winText.text = "Not enough credits!";
                return;
            }

            StartCoroutine(ExecuteSpin());
        }

        private IEnumerator ExecuteSpin()
        {
            // Change state
            stateMachine.TransitionTo(GameState.Spinning);

            // Deduct bet and generate results
            var spinResult = gameModel.ExecuteSpin();

            // Publish spin started event
            eventBus.Publish(new SpinStartedEvent(gameModel.CurrentBet));

            // Start reels spinning
            for (int i = 0; i < reelControllers.Length; i++)
            {
                SymbolType[] reelSymbols = new SymbolType[configuration.rowCount];
                for (int j = 0; j < configuration.rowCount; j++)
                {
                    reelSymbols[j] = spinResult.Grid[i, j];
                }

                float delay = i * configuration.reelStopDelay;
                reelControllers[i].Spin(reelSymbols, delay);
            }

            // Wait for all reels to stop
            yield return new WaitForSeconds(configuration.spinDuration +
                (configuration.reelCount * configuration.reelStopDelay));

            // Transition to evaluation
            stateMachine.TransitionTo(GameState.Evaluating);

            // Evaluate wins
            var wins = paylineService.EvaluatePaylines(spinResult.Grid, gameModel.BetPerLine);
            spinResult.SetWins(wins);

            // Update credits with winnings
            if (spinResult.IsWin)
            {
                gameModel.AddCredits(spinResult.TotalWin);
            }

            // Save game state
            saveService.SaveCredits(gameModel.Credits);

            // Update statistics
            UpdateStatistics(spinResult);

            // Publish completion event
            eventBus.Publish(new SpinCompletedEvent(spinResult));

            // Show results
            stateMachine.TransitionTo(GameState.ShowingWin);
            yield return ShowWinPresentation(spinResult);

            // Return to idle
            stateMachine.TransitionTo(GameState.Idle);
        }

        private IEnumerator ShowWinPresentation(SpinResult result)
        {
            if (result.IsWin)
            {
                // Play win sound based on tier
                WinTier tier = DetermineWinTier(result);
                audioService.PlayWinSound(tier);

                // Update win text
                winText.text = result.GetWinMessage();

                // Animate winning symbols
                foreach (var win in result.Wins)
                {
                    AnimateWinningLine(win);
                }

                yield return new WaitForSeconds(2f);
            }
            else
            {
                winText.text = "Try Again!";
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void AnimateWinningLine(PaylineWin win)
        {
            for (int i = 0; i < win.matchCount; i++)
            {
                if (i < reelControllers.Length)
                {
                    reelControllers[i].AnimateSymbolAt(win.positions[i]);
                }
            }
        }

        private WinTier DetermineWinTier(SpinResult result)
        {
            float multiplier = result.GetWinMultiplier();

            if (multiplier >= 50f) return WinTier.Jackpot;
            if (multiplier >= 25f) return WinTier.Mega;
            if (multiplier >= 10f) return WinTier.Big;
            if (multiplier >= 5f) return WinTier.Medium;
            if (multiplier > 0) return WinTier.Small;
            return WinTier.None;
        }

        private void UpdateStatistics(SpinResult result)
        {
            var stats = saveService.LoadStatistics();
            stats.totalSpins++;
            stats.totalWagered += gameModel.CurrentBet;

            if (result.IsWin)
            {
                stats.totalWins++;
                stats.totalWon += result.TotalWin;

                if (result.TotalWin > stats.biggestWin)
                {
                    stats.biggestWin = result.TotalWin;
                }
            }

            saveService.SaveStatistics(stats);
        }

        // Event handlers
        private void OnSpinStarted(SpinStartedEvent e)
        {
            spinButton.interactable = false;
            winText.text = "Spinning...";
            audioService.PlaySpinSound();
        }

        private void OnSpinCompleted(SpinCompletedEvent e)
        {
            spinButton.interactable = true;
            UpdateUI();
        }

        private void OnCreditsChanged(CreditsChangedEvent e)
        {
            UpdateUI();
        }

        private void OnReelStopped(ReelStoppedEvent e)
        {
            Debug.Log($"Reel {e.ReelIndex} stopped");
        }

        private void ChangeBet(int direction)
        {
            audioService.PlayButtonClick();
            gameModel.ChangeBetPerLine(direction);
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (creditsText != null)
                creditsText.text = $"Credits: {gameModel.Credits}";

            if (betText != null)
                betText.text = $"Bet: {gameModel.CurrentBet}";

            if (linesText != null)
                linesText.text = $"Lines: {configuration.paylineCount}";
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (eventBus != null)
            {
                eventBus.Unsubscribe<SpinStartedEvent>(OnSpinStarted);
                eventBus.Unsubscribe<SpinCompletedEvent>(OnSpinCompleted);
                eventBus.Unsubscribe<CreditsChangedEvent>(OnCreditsChanged);
                eventBus.Unsubscribe<ReelStoppedEvent>(OnReelStopped);
            }

            // Save game state
            if (saveService != null && gameModel != null)
            {
                saveService.SaveCredits(gameModel.Credits);
            }
        }
    }
}