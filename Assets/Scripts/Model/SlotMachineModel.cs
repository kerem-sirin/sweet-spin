using SweetSpin.Core;
using UnityEngine;

namespace SweetSpin
{
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
                    grid[reel, row] = randomService.GetRandomSymbol(config.SymbolWeights);
                }
            }

            return grid;
        }
    }
}