namespace SweetSpin
{
    /// <summary>
    /// Categorizes wins by their magnitude relative to bet amount.
    /// Used for triggering appropriate visual/audio feedback.
    /// </summary>
    public enum WinTier
    {
        /// <summary>No win</summary>
        None,
        /// <summary>Win less than 5x bet</summary>
        Small,
        /// <summary>Win 5x-10x bet</summary>
        Medium,
        /// <summary>Win 10x-25x bet</summary>
        Big,
        /// <summary>Win 25x-50x bet</summary>
        Mega,
        /// <summary>Win 50x+ bet</summary>
        Jackpot
    }
}