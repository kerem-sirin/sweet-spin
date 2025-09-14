namespace SweetSpin
{
    public class TurboModeChangedEvent : IGameEvent
    {
        public bool IsEnabled { get; }

        public TurboModeChangedEvent(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
    }
}