using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SweetSpin.Core
{
    /// <summary>
    /// Main game controller - Orchestrates game logic
    /// No longer responsible for view instantiation
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private SlotMachineConfiguration configuration;

        [Header("View")]
        [SerializeField] private GameObject slotMachineViewPrefab; // The complete UI prefab

        // Services
        private IEventBus eventBus;
        private IAudioService audioService;
        private ISaveService saveService;
        private IRandomService randomService;
        private IPaylineService paylineService;
        private ISymbolService symbolService;
        private IAutoPlayService autoPlayService;

        // Game components
        private SlotMachineModel gameModel;
        private SlotMachineView slotMachineView;
        private GameStateMachine stateMachine;
        private AutoPlayService autoPlayManager;

        private bool isTurboMode = false;

        private void Start()
        {
            InitializeServices();
            InitializeGame();
            CreateView();
            SetupUI();
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
                eventBus.Unsubscribe<AddCreditsRequestEvent>(OnAddCreditsRequest);
                eventBus.Unsubscribe<TurboModeChangedEvent>(OnTurboModeChanged);
                eventBus.Unsubscribe<AutoPlayStartedEvent>(OnAutoPlayStarted);
            }

            // Save game state
            if (saveService != null && gameModel != null)
            {
                saveService.SaveCredits(gameModel.Credits);
            }
        }

        private void OnTurboModeChanged(TurboModeChangedEvent e)
        {
            isTurboMode = e.IsEnabled;
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
            autoPlayService = ServiceLocator.Instance.Get<IAutoPlayService>();

            // Initialize services with configuration
            symbolService.Initialize(configuration.symbolDatabase);
            paylineService.Initialize(configuration.paylinePatterns);
            autoPlayService.Initialize(eventBus);

            // Initialize PaylineService properly
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
        }

        private void CreateView()
        {
            // Instantiate the complete slot machine view
            if (slotMachineViewPrefab != null)
            {
                GameObject viewGO = Instantiate(slotMachineViewPrefab);
                slotMachineView = viewGO.GetComponent<SlotMachineView>();

                if (slotMachineView == null)
                {
                    Debug.LogError("SlotMachineView component not found on prefab!");
                    return;
                }

                // Initialize the view with dependencies
                slotMachineView.Initialize(configuration, symbolService, eventBus);

                // Initialize state machine with the view's reel controllers
                stateMachine = new GameStateMachine(gameModel, slotMachineView.Reels, eventBus);

            }
            else
            {
                Debug.LogError("SlotMachineView prefab not assigned!");
            }
        }

        private void SetupUI()
        {
            if (slotMachineView == null) return;

            // Wire up button events
            if (slotMachineView.SpinButton != null)
                slotMachineView.SpinButton.onClick.AddListener(OnSpinButtonClick);

            if (slotMachineView.IncreaseBetButton != null)
                slotMachineView.IncreaseBetButton.onClick.AddListener(() => ChangeBet(1));

            if (slotMachineView.DecreaseBetButton != null)
                slotMachineView.DecreaseBetButton.onClick.AddListener(() => ChangeBet(-1));

            // Subscribe to events
            SubscribeToEvents();

            // Initial UI update
            UpdateUI();
        }

        private void SubscribeToEvents()
        {
            eventBus.Subscribe<SpinStartedEvent>(OnSpinStarted);
            eventBus.Subscribe<SpinCompletedEvent>(OnSpinCompleted);
            eventBus.Subscribe<CreditsChangedEvent>(OnCreditsChanged);
            eventBus.Subscribe<ReelStoppedEvent>(OnReelStopped);
            eventBus.Subscribe<AddCreditsRequestEvent>(OnAddCreditsRequest);
            eventBus.Subscribe<TurboModeChangedEvent>(OnTurboModeChanged);
            eventBus.Subscribe<AutoPlayStartedEvent>(OnAutoPlayStarted);
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
                // Publish event
                eventBus.Publish(new InsufficientCreditsEvent(
                    gameModel.CurrentBet,
                    gameModel.Credits
                ));
                return;
            }

            StartCoroutine(ExecuteSpin(isTurboMode));
        }

        private IEnumerator ExecuteSpin(bool isTurboMode = false)
        {
            // Change state
            stateMachine.TransitionTo(GameState.Spinning);

            // Deduct bet and generate results
            var spinResult = gameModel.ExecuteSpin();

            // Publish spin started event
            eventBus.Publish(new SpinStartedEvent(gameModel.CurrentBet));

            // Calculate timing based on turbo mode
            float spinDuration = isTurboMode ? configuration.turboSpinDuration : configuration.spinDuration;
            float reelStopDelay = isTurboMode ? configuration.turboReelStopDelay : configuration.reelStopDelay;
            float spinSpeed = isTurboMode ? configuration.turboSpinSpeed : configuration.spinSpeed;

            // Tell the view to spin the reels with appropriate speed
            slotMachineView.SpinReels(spinResult.Grid, spinSpeed, spinDuration, reelStopDelay);

            // Wait for all reels to stop (adjusted for turbo mode)
            yield return new WaitForSeconds(spinDuration + (configuration.reelCount * reelStopDelay));

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

            // Pass turbo mode to win presentation for faster animations
            yield return ShowWinPresentation(spinResult, isTurboMode);

            // Return to idle
            stateMachine.TransitionTo(GameState.Idle);
        }

        private IEnumerator ShowWinPresentation(SpinResult result, bool isTurboMode = false)
        {
            if (result.IsWin)
            {
                // Determine win tier
                WinTier tier = DetermineWinTier(result);

                // Play appropriate sound
                audioService.PlayWinSound(tier);

                // Show win message with effects
                slotMachineView.ShowWinMessage(result.GetWinMessage(), tier);

                // Start the animation coroutine and wait for it to complete
                yield return StartCoroutine(slotMachineView.AnimateMultipleWinningLinesCoroutine(result.Wins, isTurboMode));

                // Additional hold time after all animations complete
                float additionalHoldTime = isTurboMode ? configuration.turboSequentialDelay : configuration.sequentialAnimationDelay;
                yield return new WaitForSeconds(additionalHoldTime);

                // Clear win animations after presentation
                slotMachineView.ClearAllWinAnimations();
            }
            else
            {
                slotMachineView.ShowWinMessage("Try Again!", WinTier.None);

                // only wait 1 frame to let message show briefly
                yield return null;
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
            audioService.PlaySpinSound();
        }

        private void OnSpinCompleted(SpinCompletedEvent e)
        {
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

        private void ModifyCredits(int amount)
        {
            if (gameModel == null) return;

            gameModel.AddCredits(amount);
            saveService?.SaveCredits(gameModel.Credits);
            UpdateUI();
        }

        private void OnAddCreditsRequest(AddCreditsRequestEvent e)
        {
            int previousCredits = gameModel.Credits;
            ModifyCredits(e.Amount);

            eventBus.Publish(new CreditsChangedEvent(previousCredits, gameModel.Credits));
            slotMachineView.ShowWinMessage($"+{e.Amount} Credits!", WinTier.Small);
        }

        private void UpdateUI()
        {
            if (slotMachineView != null)
            {
                slotMachineView.UpdateUI(
                    gameModel.Credits,
                    gameModel.CurrentBet
                );
            }
        }

        private void OnAutoPlayStarted(AutoPlayStartedEvent e)
        {
            // Start the auto-play coroutine
            StartCoroutine(ExecuteAutoPlaySequence());
        }

        private IEnumerator ExecuteAutoPlaySequence()
        {
            while (autoPlayService.ShouldContinue())
            {
                // Check if we can afford to spin
                if (!gameModel.CanSpin())
                {
                    autoPlayService.StopDueToInsufficientCredits();
                    break;
                }

                // Execute single spin (this handles state transitions internally)
                yield return ExecuteSpin(isTurboMode);

                // Notify auto-play service that spin completed
                autoPlayService.OnSpinCompleted();

                // If more spins remain, add delay with proper state
                if (autoPlayService.ShouldContinue())
                {
                    // Transition to waiting state to keep UI locked
                    stateMachine.TransitionTo(GameState.AutoPlayWaiting);

                    // Wait between spins
                    float delay = isTurboMode ? configuration.autoPlayDelayTurbo : configuration.autoPlayDelay;
                    yield return new WaitForSeconds(delay);

                    // Return to idle before next spin
                    stateMachine.TransitionTo(GameState.Idle);
                }
            }
        }

        [ContextMenu("Debug/Add 100 Credits")]
        private void Add100Credits() => AddDebugCredits(100);

        [ContextMenu("Debug/Add 1000 Credits")]
        private void Add1000Credits() => AddDebugCredits(1000);

        [ContextMenu("Debug/Remove 100 Credits")]
        private void Remove100Credits() => AddDebugCredits(-100);

        [ContextMenu("Debug/Remove 1000 Credits")]
        private void Remove1000Credits() => AddDebugCredits(-1000);

        [ContextMenu("Debug/Reset to Starting Credits")]
        private void ResetCredits()
        {
            if (gameModel != null && configuration != null)
            {
                gameModel.SetCredits(configuration.startingCredits);
                saveService?.SaveCredits(gameModel.Credits);
                UpdateUI();
                Debug.Log("Reset to starting credits: " + configuration.startingCredits);
            }
        }

        private void AddDebugCredits(int amount)
        {
            ModifyCredits(amount);
            Debug.Log($"Added {amount} credits. New balance: {gameModel.Credits}");
        }

        [ContextMenu("Debug/Test Single Win")]
        private void TestSingleWin()
        {
            CreateTestWin(1);
        }

        [ContextMenu("Debug/Test Double Win")]
        private void TestDoubleWin()
        {
            CreateTestWin(2);
        }

        [ContextMenu("Debug/Test Triple Win")]
        private void TestTripleWin()
        {
            CreateTestWin(3);
        }

        [ContextMenu("Debug/Test 5 Line Win")]
        private void TestFiveLineWin()
        {
            CreateTestWin(5);
        }

        [ContextMenu("Debug/Test Max Win (10 lines)")]
        private void TestMaxWin()
        {
            CreateTestWin(10);
        }

        private void CreateTestWin(int lineCount)
        {
            if (gameModel == null || slotMachineView == null)
            {
                Debug.LogError("Game not properly initialized for testing");
                return;
            }

            // Create fake win data for testing
            var testWins = new List<PaylineWin>();

            for (int i = 0; i < lineCount; i++)
            {
                // Create a test winning line
                var positions = new int[] { 1, 1, 1, 1, 1 }; // All middle row for simplicity

                var win = new PaylineWin(
                    i,                          // payline index
                    SymbolType.Cherry,          // symbol type
                    3,                          // match count
                    50 * gameModel.BetPerLine,  // win amount
                    positions                   // positions
                );

                testWins.Add(win);
            }

            // Create a test spin result
            var testGrid = new SymbolType[5, 3];
            for (int reel = 0; reel < 5; reel++)
            {
                for (int row = 0; row < 3; row++)
                {
                    testGrid[reel, row] = row == 1 ? SymbolType.Cherry : SymbolType.Lemon;
                }
            }

            var testResult = new SpinResult(testGrid, gameModel.CurrentBet);
            testResult.SetWins(testWins);

            // Show the win presentation
            Debug.Log($"Testing {lineCount} winning lines");
            StartCoroutine(ShowWinPresentation(testResult, isTurboMode));
        }

        [ContextMenu("Debug/Toggle Turbo for Testing")]
        private void ToggleTurboForTesting()
        {
            isTurboMode = !isTurboMode;
            Debug.Log($"Turbo mode for testing: {(isTurboMode ? "ON" : "OFF")}");
        }
    }
}