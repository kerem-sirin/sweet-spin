namespace SweetSpin
{
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