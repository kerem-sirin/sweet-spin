using SweetSpin.Core;
using UnityEngine;

namespace SweetSpin
{
    public class GameStateMachine
    {
        private readonly SlotMachineModel model;
        private readonly Reel[] reels;
        private readonly IEventBus eventBus;

        public GameState CurrentState { get; private set; }

        public GameStateMachine(SlotMachineModel gameModel, Reel[] reelControllers, IEventBus events)
        {
            model = gameModel;
            reels = reelControllers;
            eventBus = events;
            CurrentState = GameState.Idle;
        }

        public void TransitionTo(GameState newState)
        {
            Debug.Log($"State transition: {CurrentState} -> {newState}");

            // Exit current state
            OnStateExit(CurrentState);

            // Enter new state
            CurrentState = newState;
            OnStateEnter(newState);
        }

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

        private void OnStateExit(GameState state)
        {
            // Cleanup for state exit
        }
    }
}
