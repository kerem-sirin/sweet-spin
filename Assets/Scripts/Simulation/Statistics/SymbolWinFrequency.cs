using System;

namespace SweetSpin.Simulation.Statistics
{
    /// <summary>
    /// Performance metrics for individual symbol types tracking win contribution,
    /// average payouts, and relative frequency. Critical for symbol balance analysis
    /// and RTP contribution assessment.
    /// </summary>
    [Serializable]
    public class SymbolWinFrequency
    {
        public string symbolType;
        public int winCount;
        public float winPercentage; // Percentage of total wins
        public int totalPrizeAmount; // Total prizes from this symbol
        public float averagePrize;
    }
}