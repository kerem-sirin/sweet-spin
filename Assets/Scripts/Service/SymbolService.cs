using UnityEngine;

namespace SweetSpin
{
    /// <summary>
    /// Symbol service implementation
    /// </summary>
    public class SymbolService : ISymbolService
    {
        private SymbolData[] symbolDatabase;

        public void Initialize(SymbolData[] symbols)
        {
            symbolDatabase = symbols;
        }

        public SymbolData GetSymbolData(SymbolType type)
        {
            return System.Array.Find(symbolDatabase, s => s.type == type);
        }

        public int GetPayout(SymbolType type, int matchCount)
        {
            var data = GetSymbolData(type);
            if (data != null && matchCount >= 3 && matchCount <= 5)
            {
                return data.payouts[matchCount - 3];
            }
            return 0;
        }

        public Sprite GetSymbolSprite(SymbolType type)
        {
            var data = GetSymbolData(type);
            return data?.sprite;
        }
    }
}