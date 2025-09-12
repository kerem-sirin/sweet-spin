using System.Collections.Generic;
using UnityEngine;

namespace SweetSpin.Core
{
    /// <summary>
    /// Main game configuration as ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "SlotMachineConfig", menuName = "SweetSpin/SlotMachineConfiguration")]
    public class SlotMachineConfiguration : ScriptableObject
    {
        [Header("Reel Configuration")]
        public int reelCount = 5;
        public int rowCount = 3;
        public float reelSpacing = 160f;
        public float spinDuration = 2f;
        public float reelStopDelay = 0.3f;

        [Header("Symbols")]
        public SymbolData[] symbolDatabase;
        public int[] symbolWeights = { 30, 25, 20, 15, 10, 8, 5, 3 };

        [Header("Paylines")]
        public int paylineCount = 25;
        public PaylinePattern[] paylinePatterns;

        [Header("Betting")]
        public int startingCredits = 1000;
        public int minBetPerLine = 1;
        public int maxBetPerLine = 10;
        public int defaultBetPerLine = 1;

        [Header("Win Multipliers")]
        public float bigWinMultiplier = 10f;
        public float megaWinMultiplier = 25f;
        public float jackpotMultiplier = 50f;
    }

    /// <summary>
    /// Core game model - manages game state
    /// </summary>
    public class SlotMachineModel
    {
        private readonly SlotMachineConfiguration config;
        private readonly IRandomService randomService;

        public int Credits { get; private set; }
        public int BetPerLine { get; private set; }
        public int CurrentBet => BetPerLine * config.paylineCount;
        public bool CanSpin() => Credits >= CurrentBet;

        public SlotMachineModel(SlotMachineConfiguration configuration, IRandomService random)
        {
            config = configuration;
            randomService = random;
            Credits = config.startingCredits;
            BetPerLine = config.defaultBetPerLine;
        }

        public void SetCredits(int credits)
        {
            Credits = Mathf.Max(0, credits);
        }

        public void AddCredits(int amount)
        {
            Credits += amount;
        }

        public void ChangeBetPerLine(int direction)
        {
            BetPerLine = Mathf.Clamp(
                BetPerLine + direction,
                config.minBetPerLine,
                config.maxBetPerLine
            );
        }

        public SpinResult ExecuteSpin()
        {
            // Deduct bet
            Credits -= CurrentBet;

            // Generate random grid
            SymbolType[,] grid = GenerateRandomGrid();

            // Create result
            return new SpinResult(grid, CurrentBet);
        }

        private SymbolType[,] GenerateRandomGrid()
        {
            SymbolType[,] grid = new SymbolType[config.reelCount, config.rowCount];

            for (int reel = 0; reel < config.reelCount; reel++)
            {
                for (int row = 0; row < config.rowCount; row++)
                {
                    grid[reel, row] = randomService.GetRandomSymbol(config.symbolWeights);
                }
            }

            return grid;
        }
    }

    /// <summary>
    /// Game state machine
    /// </summary>
    public enum GameState
    {
        Idle,
        Spinning,
        Evaluating,
        ShowingWin,
        Bonus,
        GameOver
    }

    public class GameStateMachine
    {
        private readonly SlotMachineModel model;
        private readonly ReelController[] reels;
        private readonly IEventBus eventBus;

        public GameState CurrentState { get; private set; }

        public GameStateMachine(SlotMachineModel gameModel, ReelController[] reelControllers, IEventBus events)
        {
            model = gameModel;
            reels = reelControllers;
            eventBus = events;
            CurrentState = GameState.Idle;
        }

        public void TransitionTo(GameState newState)
        {
            Debug.Log($"State transition: {CurrentState} -> {newState}");

            // Exit current state
            OnStateExit(CurrentState);

            // Enter new state
            CurrentState = newState;
            OnStateEnter(newState);
        }

        private void OnStateEnter(GameState state)
        {
            switch (state)
            {
                case GameState.Idle:
                    // Enable user input
                    break;
                case GameState.Spinning:
                    // Disable user input
                    break;
                case GameState.Evaluating:
                    // Calculate wins
                    break;
                case GameState.ShowingWin:
                    // Show win animations
                    break;
            }
        }

        private void OnStateExit(GameState state)
        {
            // Cleanup for state exit
        }
    }

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