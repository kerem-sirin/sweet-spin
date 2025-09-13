namespace SweetSpin
{
    public class AddCreditsRequestEvent : IGameEvent
    {
        public int Amount { get; }
        public AddCreditsRequestEvent(int amount) => Amount = amount;
    }
}