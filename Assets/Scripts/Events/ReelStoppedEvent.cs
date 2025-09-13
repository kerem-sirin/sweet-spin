namespace SweetSpin
{
    public class ReelStoppedEvent : IGameEvent
    {
        public int ReelIndex { get; }
        public ReelStoppedEvent(int reelIndex) => ReelIndex = reelIndex;
    }
}