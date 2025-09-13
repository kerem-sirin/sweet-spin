using System;

namespace SweetSpin.Simulation
{
    /// <summary>
    /// Tracks how often each line wins
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