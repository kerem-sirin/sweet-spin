using System.Collections.Generic;

namespace SweetSpin
{
    /// <summary>
    /// Payline service implementation
    /// </summary>
    public class PaylineService : IPaylineService
    {
        private PaylinePattern[] patterns;
        private SymbolData[] symbolDatabase;

        // Standard 25 payline patterns
        private static readonly int[,] defaultPatterns = new int[,]
        {
            {1, 1, 1, 1, 1}, // Middle line
            {0, 0, 0, 0, 0}, // Top line
            {2, 2, 2, 2, 2}, // Bottom line
            {0, 1, 2, 1, 0}, // V-shape
            {2, 1, 0, 1, 2}, // Inverted V
            // ... add remaining 20 patterns
        };

        public void Initialize(PaylinePattern[] paylinePatterns)
        {
            patterns = paylinePatterns;

            // If no patterns provided, use defaults
            if (patterns == null || patterns.Length == 0)
            {
                CreateDefaultPatterns();
            }
        }

        public void Initialize(SymbolData[] symbols)
        {
            symbolDatabase = symbols;
            CreateDefaultPatterns();
        }

        private void CreateDefaultPatterns()
        {
            patterns = new PaylinePattern[25];
            for (int i = 0; i < 25; i++)
            {
                patterns[i] = new PaylinePattern
                {
                    index = i,
                    name = $"Line {i + 1}",
                    positions = new int[5]
                };

                // Copy from default patterns
                for (int j = 0; j < 5; j++)
                {
                    patterns[i].positions[j] = i < defaultPatterns.GetLength(0)
                        ? defaultPatterns[i, j]
                        : 1; // Default to middle row
                }
            }
        }

        public List<PaylineWin> EvaluatePaylines(SymbolType[,] grid, int betPerLine)
        {
            var wins = new System.Collections.Generic.List<PaylineWin>();

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

            // Skip if first symbol is Wild
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
            }

            // Count matches
            int matchCount = 1;
            for (int reel = 1; reel < 5; reel++)
            {
                if (lineSymbols[reel] == firstSymbol || lineSymbols[reel] == SymbolType.Wild)
                {
                    matchCount++;
                }
                else
                {
                    break;
                }
            }

            // Check if we have a win (3+ matches)
            if (matchCount >= 3)
            {
                int winAmount = CalculateWinAmount(firstSymbol, matchCount, betPerLine);

                // Replace the object initializer for PaylineWin in PaylineService.CheckPayline with a constructor call.
                // The error occurs because PaylineWin requires a constructor with parameters, not an object initializer.

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

        private int CalculateWinAmount(SymbolType symbol, int matchCount, int betPerLine)
        {
            // Get payout from symbol database
            if (symbolDatabase != null)
            {
                var symbolData = System.Array.Find(symbolDatabase, s => s.type == symbol);
                if (symbolData != null && matchCount - 3 < symbolData.payouts.Length)
                {
                    return symbolData.payouts[matchCount - 3] * betPerLine;
                }
            }

            // Fallback payouts
            int[,] defaultPayouts = {
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
            if (symbolIndex < defaultPayouts.GetLength(0))
            {
                return defaultPayouts[symbolIndex, matchCount - 3] * betPerLine;
            }

            return 0;
        }

        public int GetPaylineCount()
        {
            return patterns?.Length ?? 25;
        }
    }
}
