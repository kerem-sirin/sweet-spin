namespace SweetSpin
{
    public class SpinStartedEvent : IGameEvent
    {
        public int BetAmount { get; }
        public SpinStartedEvent(int betAmount) => BetAmount = betAmount;
    }
}