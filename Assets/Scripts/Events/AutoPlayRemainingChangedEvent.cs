namespace SweetSpin
{
    public class AutoPlayRemainingChangedEvent : IGameEvent
    {
        public int RemainingSpins { get; }

        public AutoPlayRemainingChangedEvent(int remaining)
        {
            RemainingSpins = remaining;
        }
    }
}