using SweetSpin.Core;
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
        public Sprite sprite;
        public int[] payouts = new int[3];
    }
}