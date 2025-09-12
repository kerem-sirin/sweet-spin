using System.Collections.Generic;
using UnityEngine;

namespace SweetSpin.Core
{
    public class PaylineManager
    {
        // 25 standard payline patterns for 5x3 slot
        // Each payline is represented as [reel0_row, reel1_row, reel2_row, reel3_row, reel4_row]
        // Rows are: 0 = top, 1 = middle, 2 = bottom
        private static readonly int[,] paylinePatterns = new int[,]
        {
            // Straight lines (3)
            {0, 0, 0, 0, 0}, // Line 2: Top row
            {1, 1, 1, 1, 1}, // Line 1: Middle row
            {2, 2, 2, 2, 2}, // Line 3: Bottom row
            
            // V-shapes (2)
            {0, 1, 2, 1, 0}, // Line 4: V-shape
            {2, 1, 0, 1, 2}, // Line 5: Inverted V
            
            // Diagonal lines (2)
            {0, 1, 1, 1, 2}, // Line 6: Slight diagonal
            {2, 1, 1, 1, 0}, // Line 7: Opposite diagonal
            
            // Zigzag patterns (6)
            {1, 0, 1, 2, 1}, // Line 8: Zigzag
            {1, 2, 1, 0, 1}, // Line 9: Reverse zigzag
            {0, 1, 0, 1, 0}, // Line 10: Top zigzag
            {2, 1, 2, 1, 2}, // Line 11: Bottom zigzag
            {1, 0, 0, 0, 1}, // Line 12: Dip pattern
            {1, 2, 2, 2, 1}, // Line 13: Rise pattern
            
            // Complex patterns (12)
            {0, 0, 1, 2, 2}, // Line 14: Step down
            {2, 2, 1, 0, 0}, // Line 15: Step up
            {0, 2, 0, 2, 0}, // Line 16: Jump pattern
            {2, 0, 2, 0, 2}, // Line 17: Reverse jump
            {0, 1, 2, 2, 2}, // Line 18: L-shape
            {2, 1, 0, 0, 0}, // Line 19: Inverted L
            {0, 0, 1, 0, 0}, // Line 20: Peak pattern
            {2, 2, 1, 2, 2}, // Line 21: Valley pattern
            {0, 2, 2, 2, 0}, // Line 22: Wide V bottom
            {2, 0, 0, 0, 2}, // Line 23: Wide V top
            {0, 2, 1, 0, 2}, // Line 24: W-shape
            {2, 0, 1, 2, 0}  // Line 25: M-shape
        };

        private List<PaylineWin> currentWins = new List<PaylineWin>();
        private SymbolData[] symbolDatabase;

        public void Initialize(SymbolData[] symbols)
        {
            symbolDatabase = symbols;
        }

        public List<PaylineWin> EvaluatePaylines(SymbolType[,] gridSymbols, int betPerLine)
        {
            currentWins.Clear();

            // Check each payline
            for (int lineIndex = 0; lineIndex < 25; lineIndex++)
            {
                PaylineWin win = CheckPayline(lineIndex, gridSymbols, betPerLine);
                if (win != null)
                {
                    currentWins.Add(win);
                }
            }

            return currentWins;
        }

        private PaylineWin CheckPayline(int lineIndex, SymbolType[,] grid, int betPerLine)
        {
            // Get the pattern for this payline
            int[] pattern = GetPaylinePattern(lineIndex);

            // Get symbols along this payline
            SymbolType[] lineSymbols = new SymbolType[5];
            for (int reel = 0; reel < 5; reel++)
            {
                lineSymbols[reel] = grid[reel, pattern[reel]];
            }

            // Check for winning combinations (left to right)
            SymbolType firstSymbol = lineSymbols[0];

            // Skip if first symbol is Wild (Wilds don't start combinations)
            if (firstSymbol == SymbolType.Wild)
            {
                // Find first non-wild symbol
                for (int i = 1; i < 5; i++)
                {
                    if (lineSymbols[i] != SymbolType.Wild)
                    {
                        firstSymbol = lineSymbols[i];
                        break;
                    }
                }
            }

            // Count consecutive matches
            int matchCount = 1;
            for (int reel = 1; reel < 5; reel++)
            {
                if (lineSymbols[reel] == firstSymbol || lineSymbols[reel] == SymbolType.Wild)
                {
                    matchCount++;
                }
                else
                {
                    break; // No more matches
                }
            }

            // Check if we have enough matches for a win (3+)
            if (matchCount >= 3)
            {
                // Calculate win amount
                SymbolData symbolData = System.Array.Find(symbolDatabase, s => s.type == firstSymbol);
                if (symbolData != null)
                {
                    int payoutIndex = matchCount - 3; // 0 for 3-match, 1 for 4-match, 2 for 5-match
                    if (payoutIndex < symbolData.payouts.Length)
                    {
                        int winAmount = symbolData.payouts[payoutIndex] * betPerLine;

                        // Create positions array for win animation
                        int[] winPositions = new int[matchCount];
                        for (int i = 0; i < matchCount; i++)
                        {
                            winPositions[i] = pattern[i];
                        }

                        return new PaylineWin(lineIndex, firstSymbol, matchCount, winAmount, winPositions);
                    }
                }
            }

            return null;
        }

        private int[] GetPaylinePattern(int lineIndex)
        {
            int[] pattern = new int[5];
            for (int i = 0; i < 5; i++)
            {
                pattern[i] = paylinePatterns[lineIndex, i];
            }
            return pattern;
        }

        public int GetTotalWin()
        {
            int total = 0;
            foreach (var win in currentWins)
            {
                total += win.winAmount;
            }
            return total;
        }

        // Visual helper to show payline patterns (for debugging or display)
        public void DrawPayline(int lineIndex, LineRenderer lineRenderer)
        {
            if (lineRenderer == null) return;

            int[] pattern = GetPaylinePattern(lineIndex);
            Vector3[] positions = new Vector3[5];

            for (int i = 0; i < 5; i++)
            {
                float x = i * 160f - 320f; // Adjust based on your reel spacing
                float y = (1 - pattern[i]) * 150f; // Adjust based on symbol height
                positions[i] = new Vector3(x, y, 0);
            }

            lineRenderer.positionCount = 5;
            lineRenderer.SetPositions(positions);
        }

        // Get all winning payline indices
        public List<int> GetWinningPaylineIndices()
        {
            List<int> indices = new List<int>();
            foreach (var win in currentWins)
            {
                indices.Add(win.paylineIndex);
            }
            return indices;
        }

        // Get description of a payline pattern (for UI display)
        public static string GetPaylineDescription(int lineIndex)
        {
            switch (lineIndex)
            {
                case 0: return "Middle Line";
                case 1: return "Top Line";
                case 2: return "Bottom Line";
                case 3: return "V-Shape";
                case 4: return "Inverted V";
                case 5: return "Diagonal Down";
                case 6: return "Diagonal Up";
                default: return $"Pattern {lineIndex + 1}";
            }
        }
    }
}