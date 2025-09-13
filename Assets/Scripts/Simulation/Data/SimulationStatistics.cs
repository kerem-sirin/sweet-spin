using SweetSpin.Simulation.Statistics;
using System;

namespace SweetSpin.Simulation.Data
{
    /// <summary>
    /// Aggregated statistical analysis of simulation results including win frequencies,
    /// payout distributions, symbol performance metrics, and line hit patterns.
    /// Provides key metrics for game balance evaluation.
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