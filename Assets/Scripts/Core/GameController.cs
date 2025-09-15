using System.Collections;
using UnityEngine;
using SweetSpin.Core.Managers;

namespace SweetSpin.Core
{
    /// <summary>
    /// Main game controller - Orchestrates game managers and coordinates game flow
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private SlotMachineConfiguration configuration;

        [Header("View")]
        [SerializeField] private GameObject slotMachineViewPrefab;

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

        // Managers (using interfaces)
        private ISpinManager spinManager;
        private IWinPresentationManager winPresentationManager;
        private IBettingManager bettingManager;
        private IGameStatisticsManager statisticsManager;
        private IAutoPlayExecutor autoPlayExecutor;
        private DebugCommandsManager debugCommandsManager;

        private bool isTurboMode = false;

        private void Start()
        {
            InitializeServices();
            InitializeGame();
            CreateView();
            InitializeManagers();
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
            }

            // Cleanup AutoPlayExecutor
            autoPlayExecutor?.Cleanup();

            // Save game state
            if (saveService != null && gameModel != null)
            {
                saveService.SaveCredits(gameModel.Credits);
            }
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
                slotMachineView.Initialize(configuration, eventBus, audioService);

                // Initialize state machine with the view's reel controllers
                stateMachine = new GameStateMachine(gameModel, slotMachineView.Reels, eventBus);
            }
            else
            {
                Debug.LogError("SlotMachineView prefab not assigned!");
            }
        }

        private void InitializeManagers()
        {
            // Create and initialize SpinManager (as a regular class, not component)
            spinManager = new SpinManager();
            spinManager.Initialize(
                configuration,
                gameModel,
                slotMachineView,
                stateMachine,
                eventBus,
                audioService,
                paylineService,
                saveService
            );

            // Create and initialize WinPresentationManager
            winPresentationManager = gameObject.AddComponent<WinPresentationManager>();
            winPresentationManager.Initialize(
                configuration,
                slotMachineView,
                stateMachine,
                audioService
            );

            // Create and initialize BettingManager (as a regular class, not component)
            bettingManager = new BettingManager();
            bettingManager.Initialize(
                gameModel,
                slotMachineView,
                eventBus,
                audioService,
                saveService
            );

            // Create and initialize GameStatisticsManager (as a regular class, not component)
            statisticsManager = new GameStatisticsManager();
            statisticsManager.Initialize(saveService, gameModel);

            // Create and initialize AutoPlayExecutor (as a regular class, not component)
            autoPlayExecutor = new AutoPlayExecutor();
            autoPlayExecutor.Initialize(
                configuration,
                gameModel,
                stateMachine,
                spinManager,
                winPresentationManager,
                statisticsManager,
                autoPlayService,
                audioService,
                eventBus,
                this  // Pass GameController as the coroutine runner
            );

            // Create and initialize DebugCommandsManager
            debugCommandsManager = gameObject.AddComponent<DebugCommandsManager>();
            debugCommandsManager.Initialize(
                configuration,
                gameModel,
                slotMachineView,
                bettingManager,
                winPresentationManager,
                statisticsManager
            );
        }

        private void SetupUI()
        {
            if (slotMachineView == null) return;

            // Wire up button events
            if (slotMachineView.SpinButton != null)
                slotMachineView.SpinButton.onClick.AddListener(OnSpinButtonClick);

            if (slotMachineView.IncreaseBetButton != null)
                slotMachineView.IncreaseBetButton.onClick.AddListener(() => bettingManager.ChangeBet(1));

            if (slotMachineView.DecreaseBetButton != null)
                slotMachineView.DecreaseBetButton.onClick.AddListener(() => bettingManager.ChangeBet(-1));

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

            if (!spinManager.ValidateSpin())
            {
                return;
            }

            // Duck music when spin starts
            audioService.DuckMusic();

            StartCoroutine(ExecuteCompleteSpinSequence());
        }

        private IEnumerator ExecuteCompleteSpinSequence()
        {
            // Execute the spin
            yield return spinManager.ExecuteSpin(isTurboMode);

            // Get the spin result from the SpinManager
            SpinResult spinResult = spinManager.LastSpinResult;

            if (spinResult != null)
            {
                // Update statistics
                statisticsManager.UpdateStatistics(spinResult);

                // Show win presentation (this will transition to ShowingWin and back to Idle)
                yield return winPresentationManager.ShowWinPresentation(spinResult, isTurboMode);
            }
            else
            {
                // Fallback: if no result, just go back to idle
                Debug.LogWarning("No spin result received, returning to idle");
                stateMachine.TransitionTo(GameState.Idle);
            }

            // Restore music volume after everything is done
            audioService.RestoreMusic();
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
            spinManager.HandleReelStop(e.ReelIndex);
        }

        private void OnAddCreditsRequest(AddCreditsRequestEvent e)
        {
            bettingManager.OnAddCreditsRequest(e);
        }

        private void OnTurboModeChanged(TurboModeChangedEvent e)
        {
            isTurboMode = e.IsEnabled;
            debugCommandsManager.SetTurboMode(e.IsEnabled);
        }

        private void UpdateUI()
        {
            if (slotMachineView != null && gameModel != null)
            {
                slotMachineView.UpdateUI(
                    gameModel.Credits,
                    gameModel.CurrentBet
                );
            }
        }
    }
}