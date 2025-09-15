using System.Collections;
using UnityEngine;
using SweetSpin.Core.Managers;

namespace SweetSpin.Core
{
    /// <summary>
    /// Manages spin execution logic including validation, processing, and result handling
    /// </summary>
    public class SpinManager : ISpinManager
    {
        private SlotMachineConfiguration configuration;
        private SlotMachineModel gameModel;
        private SlotMachineView slotMachineView;
        private GameStateMachine stateMachine;
        private IEventBus eventBus;
        private IAudioService audioService;
        private IPaylineService paylineService;
        private ISaveService saveService;

        // Store the last spin result
        public SpinResult LastSpinResult { get; private set; }

        public void Initialize(
            SlotMachineConfiguration config,
            SlotMachineModel model,
            SlotMachineView view,
            GameStateMachine gameStateMachine,
            IEventBus events,
            IAudioService audio,
            IPaylineService payline,
            ISaveService save)
        {
            configuration = config;
            gameModel = model;
            slotMachineView = view;
            stateMachine = gameStateMachine;
            eventBus = events;
            audioService = audio;
            paylineService = payline;
            saveService = save;
        }

        /// <summary>
        /// Validates if a spin can be executed
        /// </summary>
        public bool ValidateSpin()
        {
            if (stateMachine.CurrentState != GameState.Idle)
            {
                Debug.Log("Cannot spin - game not idle");
                return false;
            }

            if (!gameModel.CanSpin())
            {
                // Publish insufficient credits event
                eventBus.Publish(new InsufficientCreditsEvent(
                    gameModel.CurrentBet,
                    gameModel.Credits
                ));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Main spin execution coroutine that returns the SpinResult
        /// </summary>
        public IEnumerator ExecuteSpin(bool isTurboMode = false)
        {
            // Change state to spinning
            stateMachine.TransitionTo(GameState.Spinning);

            // Deduct bet and generate results
            var spinResult = gameModel.ExecuteSpin();
            LastSpinResult = spinResult;

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

            // Stop the spinning sound loop
            audioService.StopSpinSound();

            // Process the spin result
            ProcessSpinResult(spinResult);
        }

        /// <summary>
        /// Process the spin result after reels have stopped
        /// </summary>
        private void ProcessSpinResult(SpinResult spinResult)
        {
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

            // Publish completion event
            eventBus.Publish(new SpinCompletedEvent(spinResult));
        }

        /// <summary>
        /// Handle reel stop event
        /// </summary>
        public void HandleReelStop(int reelIndex)
        {
            audioService.PlayReelStop(reelIndex);
        }
    }
}