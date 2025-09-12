namespace SweetSpin
{
    /// <summary>
    /// Game state machine
    /// </summary>
    public enum GameState
    {
        Idle,
        Spinning,
        Evaluating,
        ShowingWin,
        Bonus,
        GameOver
    }
}