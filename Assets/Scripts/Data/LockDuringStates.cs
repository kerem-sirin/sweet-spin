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
        AutoPlayWaiting = 1 << 3,
        Bonus = 1 << 4,
        GameOver = 1 << 5,
        AllExceptIdle = Spinning | Evaluating | ShowingWin | AutoPlayWaiting | Bonus | GameOver
    }
}