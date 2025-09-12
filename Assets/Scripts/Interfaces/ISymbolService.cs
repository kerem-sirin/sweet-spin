using UnityEngine;

namespace SweetSpin
{
    /// <summary>
    /// Symbol management service
    /// </summary>
    public interface ISymbolService
    {
        void Initialize(SymbolData[] symbols);
        SymbolData GetSymbolData(SymbolType type);
        int GetPayout(SymbolType type, int matchCount);
        Sprite GetSymbolSprite(SymbolType type);
    }
}