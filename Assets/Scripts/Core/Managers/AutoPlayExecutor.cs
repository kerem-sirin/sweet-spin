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
        /// Execute the auto-play sequence
        /// </summary>
        public IEnumerator ExecuteAutoPlaySequence()
        {
            // Duck music at the start of auto-play sequence
            audioService.DuckMusic();

            while (autoPlayService.ShouldContinue())
            {
                // Check if we can afford to spin
                if (!gameModel.CanSpin())
                {
                    autoPlayService.StopDueToInsufficientCredits();
                    break;
                }

                // Execute single auto-play spin (this handles all state transitions internally)
                yield return ExecuteAutoPlaySpin();

                // Notify auto-play service that spin completed
                autoPlayService.OnSpinCompleted();

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
                else
                {
                    // Ensure we're in idle state when auto-play completes
                    if (stateMachine.CurrentState != GameState.Idle)
                    {
                        Debug.Log("AutoPlay complete - ensuring return to Idle state");
                        stateMachine.TransitionTo(GameState.Idle);
                    }
                }
            }

            // Final check to ensure we're in idle state
            if (stateMachine.CurrentState != GameState.Idle)
            {
                Debug.Log("AutoPlay sequence ended - forcing return to Idle state");
                stateMachine.TransitionTo(GameState.Idle);
            }

            // Restore music when auto-play ends
            audioService.RestoreMusic();
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
        /// Stop auto-play immediately
        /// </summary>
        public void StopAutoPlay()
        {
            if (activeAutoPlayCoroutine != null && coroutineRunner != null)
            {
                coroutineRunner.StopCoroutine(activeAutoPlayCoroutine);
                activeAutoPlayCoroutine = null;
            }

            autoPlayService?.RequestStop();
            audioService?.RestoreMusic();

            // Ensure we return to idle state
            if (stateMachine != null && stateMachine.CurrentState != GameState.Idle)
            {
                stateMachine.TransitionTo(GameState.Idle);
            }
        }
    }
}