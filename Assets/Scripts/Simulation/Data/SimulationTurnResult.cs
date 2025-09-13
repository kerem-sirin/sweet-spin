using System;

namespace SweetSpin.Simulation.Data
{
    /// <summary>
    /// Immutable record of a single spin execution containing credit state,
    /// bet amount, win outcome, and prize details. Forms the atomic unit
    /// of simulation history for detailed analysis.
    /// </summary>
    [Serializable]
    public class SimulationTurnResult
    {
        public int turn;
        public int startingCredit;
        public int betAmount;
        public bool isWin;
        public int prizeAmount;
        public int[] winningLines; // Empty array for no wins, or array of line indices
        public string winningSymbol; // The primary winning symbol type (if any)
        public int matchCount; // Number of symbols matched (3, 4, or 5)
    }
}