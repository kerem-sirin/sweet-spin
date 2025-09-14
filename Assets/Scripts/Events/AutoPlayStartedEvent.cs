namespace SweetSpin
{
    public class AutoPlayStartedEvent : IGameEvent
    {
        public int SpinCount { get; }

        public AutoPlayStartedEvent(int spinCount)
        {
            SpinCount = spinCount;
        }
    }
}