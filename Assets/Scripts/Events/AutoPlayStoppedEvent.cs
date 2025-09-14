namespace SweetSpin
{
    public class AutoPlayStoppedEvent : IGameEvent
    {
        public enum StopReason
        {
            Completed,      // All spins finished
            UserStopped,    // User pressed stop
            InsufficientCredits  // Ran out of credits
        }

        public StopReason Reason { get; }

        public AutoPlayStoppedEvent(StopReason reason)
        {
            Reason = reason;
        }
    }
}