using UnityEngine;

namespace SweetSpin
{
    /// <summary>
    /// Data structure defining a symbol's visual and payout properties.
    /// Used by the game to determine win amounts and display symbols.
    /// </summary>
    [System.Serializable]
    public class SymbolData
    {
        public SymbolType type;
        public string name;  // Add this
        public Sprite sprite;
        public int[] payouts = new int[3];
        public int weight = 10;
    }
}