namespace SweetSpin
{
    /// <summary>
    /// Represents the various states of the slot machine game flow.
    /// Used by GameStateMachine to manage game state transitions.
    /// </summary>
    public enum GameState
    {
        /// <summary>Player can interact with the game</summary>
        Idle,
        /// <summary>Reels are currently spinning</summary>
        Spinning,
        /// <summary>System is calculating win results</summary>
        Evaluating,
        /// <summary>Displaying win animations and results</summary>
        ShowingWin,
        /// <summary>Bonus game or free spins active</summary>
        Bonus,
        /// <summary>No credits remaining</summary>
        GameOver
    }
}