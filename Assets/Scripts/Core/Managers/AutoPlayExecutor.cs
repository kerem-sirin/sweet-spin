using System.Collections;
using UnityEngine;
using SweetSpin.Core.Managers;

namespace SweetSpin.Core
{
    /// <summary>
    /// Executes auto-play sequences and manages auto-play state
    /// </summary>
    public class AutoPlayExecutor : IAutoPlayExecutor
    {
        private SlotMachineConfiguration configuration;
        private SlotMachineModel gameModel;
        private GameStateMachine stateMachine;
        private ISpinManager spinManager;
        private IWinPresentationManager winPresentationManager;
        private IGameStatisticsManager statisticsManager;
        private IAutoPlayService autoPlayService;
        private IAudioService audioService;
        private IEventBus eventBus;
        private bool isTurboMode = false;
        private Coroutine activeAutoPlayCoroutine;
        private MonoBehaviour coroutineRunner;

        public void Initialize(
            SlotMachineConfiguration config,
            SlotMachineModel model,
            GameStateMachine gameStateMachine,
            ISpinManager spin,
            IWinPresentationManager winPresentation,
            IGameStatisticsManager statistics,
            IAutoPlayService autoPlay,
            IAudioService audio,
            IEventBus events,
            MonoBehaviour coroutineRunner)
        {
            configuration = config;
            gameModel = model;
            stateMachine = gameStateMachine;
            spinManager = spin;
            winPresentationManager = winPresentation;
            statisticsManager = statistics;
            autoPlayService = autoPlay;
            audioService = audio;
            eventBus = events;
            this.coroutineRunner = coroutineRunner;

            // Subscribe to events
            events.Subscribe<AutoPlayStartedEvent>(OnAutoPlayStarted);
            events.Subscribe<TurboModeChangedEvent>(OnTurboModeChanged);
        }

        public void Cleanup()
        {
            eventBus?.Unsubscribe<AutoPlayStartedEvent>(OnAutoPlayStarted);
            eventBus?.Unsubscribe<TurboModeChangedEvent>(OnTurboModeChanged);
            StopAutoPlay();
        }

        private void OnAutoPlayStarted(AutoPlayStartedEvent e)
        {
            // Start the auto-play coroutine using the provided MonoBehaviour
            if (coroutineRunner != null)
            {
                // Stop any existing auto-play coroutine
                if (activeAutoPlayCoroutine != null)
                {
                    coroutineRunner.StopCoroutine(activeAutoPlayCoroutine);
                    activeAutoPlayCoroutine = null;
                }

                activeAutoPlayCoroutine = coroutineRunner.StartCoroutine(ExecuteAutoPlaySequence());
            }
            else
            {
                Debug.LogError("AutoPlayExecutor: No coroutine runner provided!");
            }
        }

        private void OnTurboModeChanged(TurboModeChangedEvent e)
        {
            isTurboMode = e.IsEnabled;
        }

        /// <summary>
        /// Execute the auto-play sequence with improved cleanup
        /// </summary>
        public IEnumerator ExecuteAutoPlaySequence()
        {
            Debug.Log("[AutoPlayExecutor] Starting auto-play sequence");

            // Duck music at the start of auto-play sequence
            audioService.DuckMusic();

            bool completedNormally = false;

            try
            {
                while (autoPlayService.ShouldContinue())
                {
                    // Double-check stop wasn't requested
                    if (autoPlayService.StopRequested)
                    {
                        Debug.Log("[AutoPlayExecutor] Stop requested before spin, breaking loop");
                        break;
                    }

                    // Check if we can afford to spin
                    if (!gameModel.CanSpin())
                    {
                        Debug.Log("[AutoPlayExecutor] Insufficient credits, stopping auto-play");
                        autoPlayService.StopDueToInsufficientCredits();
                        break;
                    }

                    // Execute single auto-play spin
                    yield return ExecuteAutoPlaySpin();

                    // Check if stop was requested during the spin
                    if (autoPlayService.StopRequested)
                    {
                        Debug.Log("[AutoPlayExecutor] Stop requested during spin, notifying service");
                        autoPlayService.OnSpinCompleted();
                        break;
                    }

                    // Notify auto-play service that spin completed
                    autoPlayService.OnSpinCompleted();

                    // Check again after spin completed
                    if (!autoPlayService.ShouldContinue())
                    {
                        Debug.Log("[AutoPlayExecutor] Auto-play should not continue, breaking loop");
                        break;
                    }

                    // If more spins remain, add delay with proper state
                    if (autoPlayService.ShouldContinue())
                    {
                        // Transition to waiting state to keep UI locked
                        stateMachine.TransitionTo(GameState.AutoPlayWaiting);

                        // Wait between spins
                        float delay = isTurboMode ?
                            configuration.autoPlayDelayTurbo :
                            configuration.autoPlayDelay;
                        yield return new WaitForSeconds(delay);

                        // Return to idle before next spin
                        stateMachine.TransitionTo(GameState.Idle);
                    }
                }

                completedNormally = true;
                Debug.Log("[AutoPlayExecutor] Auto-play sequence completed normally");
            }
            finally
            {
                // Ensure cleanup happens regardless of how we exit
                Debug.Log($"[AutoPlayExecutor] Cleaning up (completed normally: {completedNormally})");

                // Clear the coroutine reference
                activeAutoPlayCoroutine = null;

                // Ensure we're in idle state
                if (stateMachine.CurrentState != GameState.Idle)
                {
                    Debug.Log($"[AutoPlayExecutor] Forcing return to Idle state from {stateMachine.CurrentState}");
                    stateMachine.TransitionTo(GameState.Idle);
                }

                // Restore music
                audioService.RestoreMusic();

                // Force the auto-play service to stop if it hasn't already
                if (autoPlayService.IsActive)
                {
                    Debug.Log("[AutoPlayExecutor] Force stopping auto-play service");
                    autoPlayService.ForceStop();
                }
            }
        }

        /// <summary>
        /// Execute a single spin in auto-play mode
        /// </summary>
        private IEnumerator ExecuteAutoPlaySpin()
        {
            // Execute the spin
            yield return spinManager.ExecuteSpin(isTurboMode);

            // Get the result from the SpinManager
            SpinResult lastResult = spinManager.LastSpinResult;

            if (lastResult != null)
            {
                // Update statistics
                statisticsManager.UpdateStatistics(lastResult);

                // Show win presentation (this will handle ShowingWin -> Idle transition)
                yield return winPresentationManager.ShowWinPresentation(lastResult, isTurboMode);
            }
            else
            {
                // Fallback: ensure we return to idle
                Debug.LogWarning("AutoPlay: No spin result received, returning to idle");
                stateMachine.TransitionTo(GameState.Idle);
            }
        }

        /// <summary>
        /// Stop auto-play immediately with improved cleanup
        /// </summary>
        public void StopAutoPlay()
        {
            Debug.Log("[AutoPlayExecutor] StopAutoPlay called");

            if (activeAutoPlayCoroutine != null && coroutineRunner != null)
            {
                Debug.Log("[AutoPlayExecutor] Stopping active coroutine");
                coroutineRunner.StopCoroutine(activeAutoPlayCoroutine);
                activeAutoPlayCoroutine = null;
            }

            // Request stop from service
            autoPlayService?.RequestStop();

            // Restore music
            audioService?.RestoreMusic();

            // Ensure we return to idle state
            if (stateMachine != null && stateMachine.CurrentState != GameState.Idle)
            {
                Debug.Log($"[AutoPlayExecutor] Transitioning from {stateMachine.CurrentState} to Idle");
                stateMachine.TransitionTo(GameState.Idle);
            }

            // Force stop if service is still active
            if (autoPlayService?.IsActive == true)
            {
                Debug.Log("[AutoPlayExecutor] Force stopping service");
                autoPlayService.ForceStop();
            }
        }
    }
}