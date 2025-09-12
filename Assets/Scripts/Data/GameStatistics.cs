using System;

namespace SweetSpin
{
    /// <summary>
    /// Tracks cumulative game statistics for analytics and player history.
    /// Persisted between sessions via SaveService.
    /// </summary>
    [Serializable]
    public class GameStatistics
    {
        /// <summary>Total number of spins executed</summary>
        public int totalSpins;
        /// <summary>Number of spins that resulted in wins</summary>
        public int totalWins;
        /// <summary>Highest single win amount</summary>
        public int biggestWin;
        /// <summary>Total credits wagered across all spins</summary>
        public long totalWagered;
        /// <summary>Total credits won from all spins</summary>
        public long totalWon;
    }
}