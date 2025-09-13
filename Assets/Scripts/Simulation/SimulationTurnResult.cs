using System;

namespace SweetSpin.Simulation
{
    /// <summary>
    /// Represents a single turn in the simulation
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