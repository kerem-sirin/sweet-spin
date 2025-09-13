using System;

namespace SweetSpin.Simulation.Data
{
    /// <summary>
    /// Main container for complete simulation results including configuration metadata,
    /// financial summary, detailed statistics, and individual turn history.
    /// Serializable for JSON export and analysis.
    /// </summary>
    [Serializable]
    public class SimulationReport
    {
        // Basic Info
        public string timestamp;
        public string configurationName;
        public int initialCredits;
        public int finalCredits;
        public int totalTurns;
        public int betPerLine;
        public int totalLines;

        // Financial Summary
        public int totalBet;
        public int totalWon;
        public float rtp; // Return to Player percentage

        // Statistics
        public SimulationStatistics statistics;

        // All turn results
        public SimulationTurnResult[] turns;
    }
}