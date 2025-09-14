namespace SweetSpin
{
    public class GameStateChangedEvent : IGameEvent
    {
        public GameState PreviousState { get; }
        public GameState NewState { get; }
        public bool IsIdle => NewState == GameState.Idle;

        public GameStateChangedEvent(GameState previousState, GameState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }
}