using SweetSpin.Core;

namespace SweetSpin
{
    public class SpinCompletedEvent : IGameEvent
    {
        public SpinResult Result { get; }
        public SpinCompletedEvent(SpinResult result) => Result = result;
    }
}