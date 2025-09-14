namespace SweetSpin
{
    /// <summary>
    /// Represents the various states of the slot machine game flow.
    /// Used by GameStateMachine to manage game state transitions.
    /// </summary>
    public enum GameState
    {
        Idle,               // Ready for input
        Spinning,           // Reels are spinning
        Evaluating,         // Evaluating the outcome of the spin
        ShowingWin,         // Showing win animations
        AutoPlayWaiting,    // Waiting for the next auto-play spin
        Bonus,              // Bonus game or free spins active
        GameOver            // No credits remaining
    }
}