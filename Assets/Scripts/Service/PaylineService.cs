using System.Collections.Generic;
using UnityEngine;

namespace SweetSpin
{
    /// <summary>
    /// Payline service implementation
    /// Evaluates winning combinations across configured paylines
    /// </summary>
    public class PaylineService : IPaylineService
    {
        private PaylinePattern[] patterns;
        private SymbolData[] symbolDatabase;

        /// <summary>
        /// Initialize with payline patterns from configuration
        /// </summary>
        public void Initialize(PaylinePattern[] paylinePatterns)
        {
            if (paylinePatterns == null || paylinePatterns.Length == 0)
            {
                Debug.LogError("PaylineService: No payline patterns provided! Service will not function correctly.");
                patterns = new PaylinePattern[0];
                return;
            }

            patterns = paylinePatterns;
            Debug.Log($"PaylineService initialized with {patterns.Length} paylines");
        }

        /// <summary>
        /// Initialize with symbol database for payout calculations
        /// </summary>
        public void Initialize(SymbolData[] symbols)
        {
            if (symbols == null || symbols.Length == 0)
            {
                Debug.LogWarning("PaylineService: No symbol database provided. Will use fallback payouts.");
            }

            symbolDatabase = symbols;
        }

        /// <summary>
        /// Evaluate all paylines for winning combinations
        /// </summary>
        public List<PaylineWin> EvaluatePaylines(SymbolType[,] grid, int betPerLine)
        {
            var wins = new List<PaylineWin>();

            if (patterns == null || patterns.Length == 0)
            {
                Debug.LogError("PaylineService: Cannot evaluate paylines - no patterns loaded!");
                return wins;
            }

            foreach (var pattern in patterns)
            {
                var win = CheckPayline(pattern, grid, betPerLine);
                if (win != null)
                {
                    wins.Add(win);
                }
            }

            return wins;
        }

        /// <summary>
        /// Check a single payline for winning combinations
        /// </summary>
        private PaylineWin CheckPayline(PaylinePattern pattern, SymbolType[,] grid, int betPerLine)
        {
            // Get symbols along this payline
            SymbolType[] lineSymbols = new SymbolType[5];
            for (int reel = 0; reel < 5; reel++)
            {
                lineSymbols[reel] = grid[reel, pattern.positions[reel]];
            }

            // Check for winning combination
            SymbolType firstSymbol = lineSymbols[0];

            // Skip if first symbol is Wild (Wilds don't start combinations)
            if (firstSymbol == SymbolType.Wild)
            {
                for (int i = 1; i < 5; i++)
                {
                    if (lineSymbols[i] != SymbolType.Wild)
                    {
                        firstSymbol = lineSymbols[i];
                        break;
                    }
                }

                // If all symbols are Wild, treat as Wild win
                if (firstSymbol == SymbolType.Wild)
                {
                    // All wilds is a valid win
                }
            }

            // Count consecutive matches from left to right
            int matchCount = 1;
            for (int reel = 1; reel < 5; reel++)
            {
                if (lineSymbols[reel] == firstSymbol || lineSymbols[reel] == SymbolType.Wild)
                {
                    matchCount++;
                }
                else
                {
                    break; // Stop at first non-matching symbol
                }
            }

            // Check if we have a win (3+ matches)
            if (matchCount >= 3)
            {
                int winAmount = CalculateWinAmount(firstSymbol, matchCount, betPerLine);

                return new PaylineWin(
                    pattern.index,
                    firstSymbol,
                    matchCount,
                    winAmount,
                    pattern.positions
                );
            }

            return null;
        }

        /// <summary>
        /// Calculate win amount based on symbol and match count
        /// </summary>
        private int CalculateWinAmount(SymbolType symbol, int matchCount, int betPerLine)
        {
            // Get payout from symbol database
            if (symbolDatabase != null && symbolDatabase.Length > 0)
            {
                var symbolData = System.Array.Find(symbolDatabase, s => s.type == symbol);
                if (symbolData != null && matchCount - 3 < symbolData.payouts.Length)
                {
                    return symbolData.payouts[matchCount - 3] * betPerLine;
                }
            }

            // Fallback payouts if no symbol database
            // This should ideally never be used in production
            Debug.LogWarning($"PaylineService: Using fallback payout for {symbol} x{matchCount}");

            int[,] fallbackPayouts = {
                {5, 10, 20},    // Cherry
                {10, 20, 40},   // Lemon
                {15, 30, 60},   // Orange
                {20, 40, 80},   // Plum
                {25, 50, 100},  // Bell
                {30, 60, 150},  // Bar
                {50, 100, 250}, // Seven
                {100, 200, 500} // Wild
            };

            int symbolIndex = (int)symbol;
            if (symbolIndex < fallbackPayouts.GetLength(0) && matchCount >= 3 && matchCount <= 5)
            {
                return fallbackPayouts[symbolIndex, matchCount - 3] * betPerLine;
            }

            return 0;
        }

        /// <summary>
        /// Get the current number of paylines
        /// </summary>
        public int GetPaylineCount()
        {
            return patterns?.Length ?? 0;
        }
    }
}