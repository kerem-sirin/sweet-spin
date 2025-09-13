using System;

namespace SweetSpin.Simulation
{
    /// <summary>
    /// Detailed statistics from the simulation
    /// </summary>
    [Serializable]
    public class SimulationStatistics
    {
        // Win Statistics
        public float hitFrequency; // Percentage of winning spins
        public int totalWins;
        public int totalLosses;
        public float averageWinAmount;
        public int biggestWin;
        public int biggestWinTurn;

        // Line Statistics
        public LineFrequency[] mostCommonWinningLines; // Top 5 most hit lines
        public int totalLineHits;

        // Symbol Statistics
        public SymbolWinFrequency[] symbolWinFrequency; // How often each symbol wins

        // Win Distribution
        public int smallWins; // < 5x bet
        public int mediumWins; // 5x - 10x bet
        public int bigWins; // 10x - 25x bet
        public int megaWins; // 25x - 50x bet
        public int jackpotWins; // 50x+ bet

        // Match Count Distribution
        public int threeOfKindWins;
        public int fourOfKindWins;
        public int fiveOfKindWins;
    }
}
