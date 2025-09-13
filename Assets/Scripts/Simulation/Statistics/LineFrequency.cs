using System;

namespace SweetSpin.Simulation.Statistics
{
    /// <summary>
    /// Statistical tracker for individual payline performance measuring hit count
    /// and relative frequency. Used to identify payline balance and pattern biases.
    /// </summary>
    [Serializable]
    public class LineFrequency
    {
        public int lineIndex;
        public string lineName;
        public int hitCount;
        public float hitPercentage; // Percentage of total wins
    }
}