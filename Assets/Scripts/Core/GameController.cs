using System.Collections;
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

        // Game components
        private SlotMachineModel gameModel;
        private SlotMachineView slotMachineView;
        private GameStateMachine stateMachine;

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
                eventBus?.Unsubscribe<AddCreditsRequestEvent>(OnAddCreditsRequest);
                eventBus.Unsubscribe<TurboModeChangedEvent>(OnTurboModeChanged);
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

            // Initialize services with configuration
            symbolService.Initialize(configuration.symbolDatabase);
            paylineService.Initialize(configuration.paylinePatterns);

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

                // Animate winning lines
                foreach (var win in result.Wins)
                {
                    slotMachineView.AnimateWinningLine(win);
                }

                // Shorter display time in turbo mode
                float displayTime = isTurboMode ? 0.5f : 2f;
                yield return new WaitForSeconds(displayTime);
            }
            else
            {
                slotMachineView.ShowWinMessage("Try Again!", WinTier.None);

                // Shorter delay in turbo mode
                float displayTime = isTurboMode ? 0.2f : 0.5f;
                yield return new WaitForSeconds(displayTime);
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
    }
}