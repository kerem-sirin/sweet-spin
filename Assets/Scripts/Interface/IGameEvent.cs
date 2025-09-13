using SweetSpin.Core;

namespace SweetSpin
{
    public interface IGameEvent { }

    public class SpinStartedEvent : IGameEvent
    {
        public int BetAmount { get; }
        public SpinStartedEvent(int betAmount) => BetAmount = betAmount;
    }

    public class ReelStoppedEvent : IGameEvent
    {
        public int ReelIndex { get; }
        public ReelStoppedEvent(int reelIndex) => ReelIndex = reelIndex;
    }

    public class SpinCompletedEvent : IGameEvent
    {
        public SpinResult Result { get; }
        public SpinCompletedEvent(SpinResult result) => Result = result;
    }

    public class CreditsChangedEvent : IGameEvent
    {
        public int OldCredits { get; }
        public int NewCredits { get; }
        public CreditsChangedEvent(int oldCredits, int newCredits)
        {
            OldCredits = oldCredits;
            NewCredits = newCredits;
        }
    }
}