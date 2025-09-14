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
            if (isActive) return;

            totalSpins = spinCount;
            remainingSpins = spinCount;
            isActive = true;
            stopRequested = false;

            eventBus?.Publish(new AutoPlayStartedEvent(spinCount));
            Debug.Log($"[AutoPlayService] Started with {spinCount} spins");
        }

        public void RequestStop()
        {
            if (!isActive) return;

            stopRequested = true;
            Debug.Log("[AutoPlayService] Stop requested - will stop after current spin");
        }

        public void OnSpinCompleted()
        {
            if (!isActive) return;

            remainingSpins--;

            // Publish an event when remaining count changes
            eventBus?.Publish(new AutoPlayRemainingChangedEvent(remainingSpins));

            // Check if we should stop
            if (remainingSpins <= 0 || stopRequested)
            {
                StopAutoPlay(stopRequested ?
                    AutoPlayStoppedEvent.StopReason.UserStopped :
                    AutoPlayStoppedEvent.StopReason.Completed);
            }
        }

        public void StopDueToInsufficientCredits()
        {
            StopAutoPlay(AutoPlayStoppedEvent.StopReason.InsufficientCredits);
        }

        private void StopAutoPlay(AutoPlayStoppedEvent.StopReason reason)
        {
            isActive = false;
            stopRequested = false;
            remainingSpins = 0;

            eventBus?.Publish(new AutoPlayStoppedEvent(reason));
            Debug.Log($"[AutoPlayService] Stopped: {reason}");
        }

        public bool ShouldContinue()
        {
            return isActive && remainingSpins > 0 && !stopRequested;
        }
    }
}