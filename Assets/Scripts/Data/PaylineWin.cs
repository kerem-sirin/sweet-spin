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
        public int[] positions; // Which rows won

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