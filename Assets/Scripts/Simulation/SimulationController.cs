using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SweetSpin.Core;

namespace SweetSpin.Simulation
{
    /// <summary>
    /// Handles the core simulation logic without any UI dependencies
    /// </summary>
    public class SimulationController
    {
        private SlotMachineConfiguration configuration;
        private IRandomService randomService;
        private ISymbolService symbolService;
        private IPaylineService paylineService;
        private SlotMachineModel gameModel;

        // Statistics tracking
        private Dictionary<int, int> lineHitCounts;
        private Dictionary<SymbolType, SymbolStats> symbolStats;
        private List<SimulationTurnResult> turnResults;

        private class SymbolStats
        {
            public int winCount;
            public int totalPrize;
        }

        public SimulationController(SlotMachineConfiguration config)
        {
            configuration = config;
            InitializeServices();
            InitializeTracking();
        }

        private void InitializeServices()
        {
            // Create service instances - same as game but without UI services
            randomService = new RandomService();
            symbolService = new SymbolService();
            paylineService = new PaylineService();

            // Initialize services with configuration
            symbolService.Initialize(configuration.symbolDatabase);
            paylineService.Initialize(configuration.paylinePatterns);

            // Also initialize payline service with symbols for payout calculation
            var paylineServiceImpl = paylineService as PaylineService;
            paylineServiceImpl?.Initialize(configuration.symbolDatabase);
        }

        private void InitializeTracking()
        {
            lineHitCounts = new Dictionary<int, int>();
            symbolStats = new Dictionary<SymbolType, SymbolStats>();
            turnResults = new List<SimulationTurnResult>();

            // Initialize symbol stats for all symbol types
            foreach (SymbolType symbolType in Enum.GetValues(typeof(SymbolType)))
            {
                symbolStats[symbolType] = new SymbolStats();
            }
        }

        /// <summary>
        /// Run a complete simulation
        /// </summary>
        public SimulationReport RunSimulation(int startingCredits, int betPerLine, int maxTurns)
        {
            // Reset tracking
            InitializeTracking();

            // Create game model
            gameModel = new SlotMachineModel(configuration, randomService);
            gameModel.SetCredits(startingCredits);

            // Set bet amount
            for (int i = 0; i < betPerLine - 1; i++)
            {
                gameModel.ChangeBetPerLine(1);
            }

            // Simulation variables
            int currentTurn = 0;
            int totalBet = 0;
            int totalWon = 0;
            int biggestWin = 0;
            int biggestWinTurn = 0;

            // Run simulation loop
            while (gameModel.Credits >= gameModel.CurrentBet && currentTurn < maxTurns)
            {
                int startCredit = gameModel.Credits;

                // Execute spin
                var spinResult = gameModel.ExecuteSpin();

                // Evaluate paylines
                var wins = paylineService.EvaluatePaylines(spinResult.Grid, gameModel.BetPerLine);
                spinResult.SetWins(wins);

                // Calculate total win for this spin
                int spinWinAmount = spinResult.TotalWin;

                // Update credits
                if (spinWinAmount > 0)
                {
                    gameModel.AddCredits(spinWinAmount);
                }

                // Track statistics
                totalBet += gameModel.CurrentBet;
                totalWon += spinWinAmount;

                if (spinWinAmount > biggestWin)
                {
                    biggestWin = spinWinAmount;
                    biggestWinTurn = currentTurn;
                }

                // Record turn result
                var turnResult = new SimulationTurnResult
                {
                    turn = currentTurn,
                    startingCredit = startCredit,
                    betAmount = gameModel.CurrentBet,
                    isWin = spinWinAmount > 0,
                    prizeAmount = spinWinAmount,
                    winningLines = wins.Count > 0 ? wins.Select(w => w.paylineIndex).ToArray() : new int[0],
                    winningSymbol = wins.Count > 0 ? wins[0].symbol.ToString() : "",
                    matchCount = wins.Count > 0 ? wins[0].matchCount : 0
                };

                turnResults.Add(turnResult);

                // Track line and symbol statistics
                foreach (var win in wins)
                {
                    // Track line hits
                    if (!lineHitCounts.ContainsKey(win.paylineIndex))
                        lineHitCounts[win.paylineIndex] = 0;
                    lineHitCounts[win.paylineIndex]++;

                    // Track symbol wins
                    symbolStats[win.symbol].winCount++;
                    symbolStats[win.symbol].totalPrize += win.winAmount;
                }

                currentTurn++;
            }

            // Generate report
            return GenerateReport(
                startingCredits,
                betPerLine,
                totalBet,
                totalWon,
                biggestWin,
                biggestWinTurn
            );
        }

        private SimulationReport GenerateReport(
            int startingCredits,
            int betPerLine,
            int totalBet,
            int totalWon,
            int biggestWin,
            int biggestWinTurn)
        {
            var report = new SimulationReport
            {
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                configurationName = configuration.name,
                initialCredits = startingCredits,
                finalCredits = gameModel.Credits,
                totalTurns = turnResults.Count,
                betPerLine = betPerLine,
                totalLines = configuration.paylineCount,
                totalBet = totalBet,
                totalWon = totalWon,
                rtp = totalBet > 0 ? (totalWon / (float)totalBet) * 100f : 0f,
                turns = turnResults.ToArray()
            };

            // Generate statistics
            report.statistics = GenerateStatistics(
                biggestWin,
                biggestWinTurn,
                gameModel.CurrentBet
            );

            return report;
        }

        private SimulationStatistics GenerateStatistics(int biggestWin, int biggestWinTurn, int betAmount)
        {
            var stats = new SimulationStatistics();

            // Calculate win statistics
            var winningTurns = turnResults.Where(t => t.isWin).ToList();
            stats.totalWins = winningTurns.Count;
            stats.totalLosses = turnResults.Count - stats.totalWins;
            stats.hitFrequency = turnResults.Count > 0 ? (stats.totalWins / (float)turnResults.Count) * 100f : 0f;
            stats.averageWinAmount = stats.totalWins > 0 ? (float)winningTurns.Average(t => t.prizeAmount) : 0f;
            stats.biggestWin = biggestWin;
            stats.biggestWinTurn = biggestWinTurn;

            // Calculate most common winning lines (top 5)
            var topLines = lineHitCounts
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .Select(kvp => new LineFrequency
                {
                    lineIndex = kvp.Key,
                    lineName = $"Line {kvp.Key + 1}",
                    hitCount = kvp.Value,
                    hitPercentage = stats.totalWins > 0 ? (kvp.Value / (float)stats.totalWins) * 100f : 0f
                })
                .ToArray();
            stats.mostCommonWinningLines = topLines;
            stats.totalLineHits = lineHitCounts.Sum(kvp => kvp.Value);

            // Calculate symbol win frequency
            var symbolFrequencies = new List<SymbolWinFrequency>();
            foreach (var kvp in symbolStats)
            {
                if (kvp.Value.winCount > 0)
                {
                    symbolFrequencies.Add(new SymbolWinFrequency
                    {
                        symbolType = kvp.Key.ToString(),
                        winCount = kvp.Value.winCount,
                        winPercentage = stats.totalWins > 0 ? (kvp.Value.winCount / (float)stats.totalWins) * 100f : 0f,
                        totalPrizeAmount = kvp.Value.totalPrize,
                        averagePrize = kvp.Value.winCount > 0 ? kvp.Value.totalPrize / (float)kvp.Value.winCount : 0f
                    });
                }
            }
            stats.symbolWinFrequency = symbolFrequencies.OrderByDescending(s => s.winCount).ToArray();

            // Calculate win distribution
            foreach (var turn in winningTurns)
            {
                float multiplier = turn.prizeAmount / (float)betAmount;

                if (multiplier >= 50f)
                    stats.jackpotWins++;
                else if (multiplier >= 25f)
                    stats.megaWins++;
                else if (multiplier >= 10f)
                    stats.bigWins++;
                else if (multiplier >= 5f)
                    stats.mediumWins++;
                else
                    stats.smallWins++;
            }

            // Calculate match count distribution
            foreach (var turn in winningTurns)
            {
                switch (turn.matchCount)
                {
                    case 3:
                        stats.threeOfKindWins++;
                        break;
                    case 4:
                        stats.fourOfKindWins++;
                        break;
                    case 5:
                        stats.fiveOfKindWins++;
                        break;
                }
            }

            return stats;
        }
    }
}