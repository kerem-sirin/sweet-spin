using UnityEngine;

namespace SweetSpin
{
    /// <summary>
    /// Reel controller - refactored from your existing Reel class
    /// </summary>
    public class ReelController : MonoBehaviour
    {
        private int reelIndex;
        private ISymbolService symbolService;
        private Reel reelView; // Your existing Reel component

        public void Initialize(int index, SymbolData[] symbols, ISymbolService service)
        {
            reelIndex = index;
            symbolService = service;

            // Get or add the existing Reel component
            reelView = GetComponent<Reel>();
            if (reelView == null)
                reelView = gameObject.AddComponent<Reel>();

            // Initialize the view
            reelView.Initialize(symbols, index);
        }

        public void Spin(SymbolType[] results, float delay)
        {
            reelView.Spin(results, delay);
        }

        public void AnimateSymbolAt(int row)
        {
            var symbols = reelView.GetVisibleSymbols();
            if (row < symbols.Length)
            {
                symbols[row].AnimateWin();
            }
        }

        public bool IsSpinning => reelView.IsSpinning;
    }
}
