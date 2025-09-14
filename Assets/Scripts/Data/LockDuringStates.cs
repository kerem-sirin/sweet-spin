namespace SweetSpin
{
    /// <summary>
    /// Defines when a UI component should be locked/unlocked based on game state
    /// </summary>
    [System.Flags]
    public enum LockDuringStates
    {
        None = 0,
        Spinning = 1 << 0,
        Evaluating = 1 << 1,
        ShowingWin = 1 << 2,
        Bonus = 1 << 3,
        GameOver = 1 << 4,
        AllExceptIdle = Spinning | Evaluating | ShowingWin | Bonus | GameOver
    }
}