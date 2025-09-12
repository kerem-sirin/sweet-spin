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
        public int[] payouts = new int[3]; // 3-of-kind, 4-of-kind, 5-of-kind

        public SymbolData(SymbolType symbolType)
        {
            type = symbolType;
            // Default payouts - you can adjust these
            switch (symbolType)
            {
                case SymbolType.Cherry:
                    payouts = new int[] { 5, 10, 20 };
                    break;
                case SymbolType.Lemon:
                    payouts = new int[] { 10, 20, 40 };
                    break;
                case SymbolType.Orange:
                    payouts = new int[] { 15, 30, 60 };
                    break;
                case SymbolType.Plum:
                    payouts = new int[] { 20, 40, 80 };
                    break;
                case SymbolType.Bell:
                    payouts = new int[] { 25, 50, 100 };
                    break;
                case SymbolType.Bar:
                    payouts = new int[] { 30, 60, 150 };
                    break;
                case SymbolType.Seven:
                    payouts = new int[] { 50, 100, 250 };
                    break;
                case SymbolType.Wild:
                    payouts = new int[] { 100, 200, 500 };
                    break;
            }
        }
    }
}