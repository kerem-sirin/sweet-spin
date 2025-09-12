using System;

namespace SweetSpin
{
    [Serializable]
    public class PaylineWin
    {
        public int paylineIndex;
        public SymbolType symbol;
        public int matchCount;
        public int winAmount;
        public int[] positions; // The reel positions that form this win

        public PaylineWin(int line, SymbolType sym, int matches, int amount, int[] pos)
        {
            paylineIndex = line;
            symbol = sym;
            matchCount = matches;
            winAmount = amount;
            positions = pos;
        }
    }
}