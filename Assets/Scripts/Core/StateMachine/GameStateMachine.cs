using UnityEngine;

namespace SweetSpin.Core
{
    /// <summary>
    /// Handles state transitions
    /// </summary>
    public class GameStateMachine
    {
        private readonly SlotMachineModel model;
        private readonly Reel[] reels;
        private readonly IEventBus eventBus;

        /// <summary>Current state of the game</summary>
        public GameState CurrentState { get; private set; }

        /// <summary>
        /// Initializes the state machine with game components
        /// </summary>
        public GameStateMachine(SlotMachineModel gameModel, Reel[] reelControllers, IEventBus events)
        {
            model = gameModel;
            reels = reelControllers;
            eventBus = events;
            CurrentState = GameState.Idle;
        }

        /// <summary>
        /// Transitions to a new state, triggering exit and enter logic
        /// </summary>
        public void TransitionTo(GameState newState)
        {
            Debug.Log($"State transition: {CurrentState} -> {newState}");
            var previousState = CurrentState;

            // Exit current state
            OnStateExit(CurrentState);

            // Enter new state
            CurrentState = newState;
            OnStateEnter(newState);

            // Publish state change event
            eventBus?.Publish(new GameStateChangedEvent(previousState, newState));
        }

        /// <summary>
        /// Executes logic when entering a state
        /// </summary>
        private void OnStateEnter(GameState state)
        {
            switch (state)
            {
                case GameState.Idle:
                    // Enable user input
                    break;
                case GameState.Spinning:
                    // Disable user input
                    break;
                case GameState.Evaluating:
                    // Calculate wins
                    break;
                case GameState.ShowingWin:
                    // Show win animations
                    break;
            }
        }

        /// <summary>
        /// Executes cleanup logic when exiting a state
        /// </summary>
        private void OnStateExit(GameState state)
        {
            // Cleanup for state exit
        }
    }
}