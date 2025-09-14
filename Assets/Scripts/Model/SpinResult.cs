using System.Collections.Generic;
using UnityEngine;

namespace SweetSpin.Core
{
    /// <summary>
    /// Encapsulates all data related to a single spin outcome
    /// </summary>
    public class SpinResult
    {
        // Core spin data
        public SymbolType[,] Grid { get; private set; }
        public List<PaylineWin> Wins { get; private set; }
        public int TotalWin { get; private set; }
        public int BetAmount { get; private set; }

        // Special features
        public bool HasWilds { get; private set; }
        public int WildCount { get; private set; }
        public bool IsBigWin { get; private set; } // Win >= 10x bet
        public bool IsMegaWin { get; private set; } // Win >= 25x bet
        public bool IsJackpot { get; private set; } // Win >= 50x bet

        // Statistics
        public int TotalWinningLines { get; private set; }
        public SymbolType? HighestPayingSymbol { get; private set; }
        public int LongestLineMatch { get; private set; }

        // Constructor that takes grid and bet amount
        public SpinResult(SymbolType[,] grid, int betAmount)
        {
            Grid = grid;
            BetAmount = betAmount;
            Wins = new List<PaylineWin>();

            // Analyze grid for special symbols
            AnalyzeGrid();
        }

        // Method to set wins after evaluation
        public void SetWins(List<PaylineWin> wins)
        {
            Wins = wins ?? new List<PaylineWin>();
            AnalyzeResults();
        }

        private void AnalyzeResults()
        {
            // Calculate total win
            TotalWin = 0;
            LongestLineMatch = 0;
            int highestSingleWin = 0;

            foreach (var win in Wins)
            {
                TotalWin += win.winAmount;

                if (win.matchCount > LongestLineMatch)
                {
                    LongestLineMatch = win.matchCount;
                }

                if (win.winAmount > highestSingleWin)
                {
                    highestSingleWin = win.winAmount;
                    HighestPayingSymbol = win.symbol;
                }
            }

            // Count winning lines
            TotalWinningLines = Wins.Count;

            // Check for special wins
            if (BetAmount > 0)
            {
                float winMultiplier = (float)TotalWin / BetAmount;
                IsBigWin = winMultiplier >= 10f;
                IsMegaWin = winMultiplier >= 25f;
                IsJackpot = winMultiplier >= 50f;
            }
        }

        private void AnalyzeGrid()
        {
            WildCount = 0;

            int cols = Grid.GetLength(0);
            int rows = Grid.GetLength(1);

            for (int col = 0; col < cols; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    if (Grid[col, row] == SymbolType.Wild)
                    {
                        WildCount++;
                        HasWilds = true;
                    }
                }
            }
        }

        public bool IsWin => TotalWin > 0;

        /// <summary>
        /// Get win multiplier relative to bet
        /// </summary>
        public float GetWinMultiplier()
        {
            if (BetAmount <= 0) return 0;
            return (float)TotalWin / BetAmount;
        }

        /// <summary>
        /// Get a descriptive string for the win tier
        /// </summary>
        public string GetWinTierDescription()
        {
            if (IsJackpot) return "JACKPOT!";
            if (IsMegaWin) return "MEGA WIN!";
            if (IsBigWin) return "BIG WIN!";
            if (IsWin) return "WIN!";
            return "";
        }

        /// <summary>
        /// Get formatted win message for UI
        /// </summary>
        public string GetWinMessage()
        {
            if (!IsWin) return "Try Again!";

            string debugMessage = "";

            // Add tier description
            string tier = GetWinTierDescription();
            if (!string.IsNullOrEmpty(tier))
            {
                debugMessage = tier + "\n";
            }

            // Add win details
            if (TotalWinningLines == 1)
            {
                var win = Wins[0];
                debugMessage += $"{win.matchCount}x {win.symbol}\n";
            }
            else if (TotalWinningLines > 1)
            {
                debugMessage += $"{TotalWinningLines} WINNING LINES!\n";
            }

            // Add total win
            debugMessage += $"WIN: {TotalWin} CREDITS";

            // Add multiplier for big wins
            if (IsBigWin)
            {
                debugMessage += $"\n({GetWinMultiplier():F1}x BET)";
            }

            Debug.Log(debugMessage);

            return TotalWin.ToString();
        }

        /// <summary>
        /// Debug information for console
        /// </summary>
        public void LogDebugInfo()
        {
            Debug.Log("=== SPIN RESULT ===");
            Debug.Log($"Total Win: {TotalWin} ({GetWinMultiplier():F1}x bet)");
            Debug.Log($"Winning Lines: {TotalWinningLines}");

            if (IsWin)
            {
                Debug.Log($"Win Tier: {GetWinTierDescription()}");
                Debug.Log($"Highest Symbol: {HighestPayingSymbol}");
                Debug.Log($"Longest Match: {LongestLineMatch} symbols");

                foreach (var win in Wins)
                {
                    Debug.Log($"  Line {win.paylineIndex + 1}: {win.matchCount}x {win.symbol} = {win.winAmount}");
                }
            }

            if (HasWilds)
            {
                Debug.Log($"Wild Symbols: {WildCount}");
            }

            Debug.Log("==================");
        }
    }
}