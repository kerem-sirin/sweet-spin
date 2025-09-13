using System;

namespace SweetSpin
{
    /// <summary>
    /// Tracks how often each symbol produces wins
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