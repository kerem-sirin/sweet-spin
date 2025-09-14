using SweetSpin.Core;
using UnityEngine;

namespace SweetSpin
{
    /// <summary>
    /// Base component for UI elements that need state-based locking
    /// </summary>
    public abstract class UILockableComponent : MonoBehaviour
    {
        [Header("Lock Configuration")]
        [SerializeField] protected LockDuringStates lockDuringStates = LockDuringStates.AllExceptIdle;

        protected IEventBus eventBus;
        protected GameState currentGameState = GameState.Idle;

        protected virtual void Start()
        {
            eventBus = ServiceLocator.Instance.Get<IEventBus>();
            eventBus?.Subscribe<GameStateChangedEvent>(OnGameStateChanged);

            // Initialize based on current state
            UpdateLockState(GameState.Idle);
        }

        protected virtual void OnDestroy()
        {
            eventBus?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            currentGameState = e.NewState;
            UpdateLockState(e.NewState);
        }

        private void UpdateLockState(GameState state)
        {
            bool shouldLock = ShouldBeLocked(state);
            SetLocked(shouldLock);
        }

        private bool ShouldBeLocked(GameState state)
        {
            switch (state)
            {
                case GameState.Idle:
                    return false;

                case GameState.Spinning:
                    return (lockDuringStates & LockDuringStates.Spinning) != 0;

                case GameState.Evaluating:
                    return (lockDuringStates & LockDuringStates.Evaluating) != 0;

                case GameState.ShowingWin:
                    return (lockDuringStates & LockDuringStates.ShowingWin) != 0;

                case GameState.AutoPlayWaiting:
                    return (lockDuringStates & LockDuringStates.AutoPlayWaiting) != 0;

                case GameState.Bonus:
                    return (lockDuringStates & LockDuringStates.Bonus) != 0;

                case GameState.GameOver:
                    return (lockDuringStates & LockDuringStates.GameOver) != 0;

                default:
                    return true;
            }
        }

        protected abstract void SetLocked(bool locked);
    }
}