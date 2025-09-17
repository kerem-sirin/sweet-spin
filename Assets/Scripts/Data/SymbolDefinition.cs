using System;

namespace SweetSpin
{
    [Serializable]
    public class SymbolDefinition
    {
        public int type;           // Maps to SymbolType enum
        public string name;        // Display name
        public int weight;         // Probability weight
        public int[] payouts;      // Payouts for 3, 4, 5 matches
    }
}