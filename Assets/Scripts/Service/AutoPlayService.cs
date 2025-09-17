using UnityEngine;

namespace SweetSpin
{
    /// <summary>
    /// Service that manages auto-play functionality without MonoBehaviour dependency
    /// </summary>
    public class AutoPlayService : IAutoPlayService
    {
        private IEventBus eventBus;
        private int totalSpins;
        private int remainingSpins;
        private bool isActive;
        private bool stopRequested;

        public bool IsActive => isActive;
        public int RemainingSpins => remainingSpins;
        public bool StopRequested => stopRequested;

        // Constructor for dependency injection
        public AutoPlayService()
        {
            // Default constructor for ServiceLocator registration
        }

        public void Initialize(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public void StartAutoPlay(int spinCount)
        {
            if (isActive)
            {
                Debug.LogWarning("[AutoPlayService] Auto-play already active, ignoring start request");
                return;
            }

            totalSpins = spinCount;
            remainingSpins = spinCount;
            isActive = true;
            stopRequested = false;

            eventBus?.Publish(new AutoPlayStartedEvent(spinCount));
            Debug.Log($"[AutoPlayService] Started with {spinCount} spins");
        }

        public void RequestStop()
        {
            if (!isActive)
            {
                Debug.Log("[AutoPlayService] Stop requested but auto-play is not active");
                return;
            }

            stopRequested = true;
            Debug.Log("[AutoPlayService] Stop requested - will stop after current spin");
        }

        public void OnSpinCompleted()
        {
            if (!isActive)
            {
                Debug.LogWarning("[AutoPlayService] Spin completed called but auto-play is not active");
                return;
            }

            remainingSpins--;
            Debug.Log($"[AutoPlayService] Spin completed, {remainingSpins} remaining");

            // Publish an event when remaining count changes
            eventBus?.Publish(new AutoPlayRemainingChangedEvent(remainingSpins));

            // Check if we should stop
            if (remainingSpins <= 0 || stopRequested)
            {
                var reason = stopRequested ?
                    AutoPlayStoppedEvent.StopReason.UserStopped :
                    AutoPlayStoppedEvent.StopReason.Completed;

                StopAutoPlay(reason);
            }
        }

        public void StopDueToInsufficientCredits()
        {
            Debug.Log("[AutoPlayService] Stopping due to insufficient credits");
            StopAutoPlay(AutoPlayStoppedEvent.StopReason.InsufficientCredits);
        }

        /// <summary>
        /// Force stop auto-play immediately (used for cleanup)
        /// </summary>
        public void ForceStop()
        {
            if (!isActive)
            {
                Debug.Log("[AutoPlayService] Force stop called but already inactive");
                return;
            }

            Debug.Log("[AutoPlayService] Force stopping auto-play");
            var reason = stopRequested ?
                AutoPlayStoppedEvent.StopReason.UserStopped :
                AutoPlayStoppedEvent.StopReason.Completed;
            StopAutoPlay(reason);
        }

        private void StopAutoPlay(AutoPlayStoppedEvent.StopReason reason)
        {
            if (!isActive)
            {
                Debug.LogWarning($"[AutoPlayService] Already stopped, ignoring stop with reason: {reason}");
                return;
            }

            Debug.Log($"[AutoPlayService] Stopping with reason: {reason}");

            isActive = false;
            stopRequested = false;
            remainingSpins = 0;

            // Always publish the stopped event
            eventBus?.Publish(new AutoPlayStoppedEvent(reason));
            Debug.Log($"[AutoPlayService] Published AutoPlayStoppedEvent with reason: {reason}");
        }

        public bool ShouldContinue()
        {
            bool should = isActive && remainingSpins > 0 && !stopRequested;
            Debug.Log($"[AutoPlayService] ShouldContinue: {should} (active:{isActive}, remaining:{remainingSpins}, stopRequested:{stopRequested})");
            return should;
        }
    }
}