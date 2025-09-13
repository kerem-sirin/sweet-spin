namespace SweetSpin
{
    public class InsufficientCreditsEvent : IGameEvent
    {
        public int RequiredAmount { get; }
        public int CurrentAmount { get; }

        public InsufficientCreditsEvent(int required, int current)
        {
            RequiredAmount = required;
            CurrentAmount = current;
        }
    }
}