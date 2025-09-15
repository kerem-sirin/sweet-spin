using SweetSpin.Core.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace SweetSpin.Core
{
    /// <summary>
    /// Manages debug commands and testing utilities for development
    /// </summary>
    public class DebugCommandsManager : MonoBehaviour
    {
        private SlotMachineConfiguration configuration;
        private SlotMachineModel gameModel;
        private SlotMachineView slotMachineView;
        private IBettingManager bettingManager;
        private IWinPresentationManager winPresentationManager;
        private IGameStatisticsManager statisticsManager;
        private bool isTurboMode = false;

        public void Initialize(
            SlotMachineConfiguration config,
            SlotMachineModel model,
            SlotMachineView view,
            IBettingManager betting,
            IWinPresentationManager winPresentation,
            IGameStatisticsManager statistics)
        {
            configuration = config;
            gameModel = model;
            slotMachineView = view;
            bettingManager = betting;
            winPresentationManager = winPresentation;
            statisticsManager = statistics;
        }

        [ContextMenu("Debug/Add 100 Credits")]
        public void Add100Credits()
        {
            AddDebugCredits(100);
        }

        [ContextMenu("Debug/Add 1000 Credits")]
        public void Add1000Credits()
        {
            AddDebugCredits(1000);
        }

        [ContextMenu("Debug/Remove 100 Credits")]
        public void Remove100Credits()
        {
            AddDebugCredits(-100);
        }

        [ContextMenu("Debug/Remove 1000 Credits")]
        public void Remove1000Credits()
        {
            AddDebugCredits(-1000);
        }

        [ContextMenu("Debug/Reset to Starting Credits")]
        public void ResetCredits()
        {
            if (gameModel != null && configuration != null)
            {
                bettingManager.SetCredits(configuration.startingCredits);
                Debug.Log("Reset to starting credits: " + configuration.startingCredits);
            }
        }

        private void AddDebugCredits(int amount)
        {
            bettingManager.AddCredits(amount);
            Debug.Log($"Added {amount} credits. New balance: {bettingManager.GetCredits()}");
        }

        [ContextMenu("Debug/Test Single Win")]
        public void TestSingleWin()
        {
            CreateTestWin(1);
        }

        [ContextMenu("Debug/Test Double Win")]
        public void TestDoubleWin()
        {
            CreateTestWin(2);
        }

        [ContextMenu("Debug/Test Triple Win")]
        public void TestTripleWin()
        {
            CreateTestWin(3);
        }

        [ContextMenu("Debug/Test 5 Line Win")]
        public void TestFiveLineWin()
        {
            CreateTestWin(5);
        }

        [ContextMenu("Debug/Test Max Win (10 lines)")]
        public void TestMaxWin()
        {
            CreateTestWin(10);
        }

        private void CreateTestWin(int lineCount)
        {
            if (gameModel == null || slotMachineView == null)
            {
                Debug.LogError("Game not properly initialized for testing");
                return;
            }

            // Debug win animation
            var testWins = new List<PaylineWin>();

            for (int i = 0; i < lineCount; i++)
            {
                // Create a test winning line
                var positions = new int[] { 1, 1, 1, 1, 1 }; // All middle row for simplicity

                var win = new PaylineWin(
                    i,                          // payline index
                    SymbolType.Cherry,          // symbol type
                    3,                          // match count
                    50 * gameModel.BetPerLine,  // win amount
                    positions                   // positions
                );

                testWins.Add(win);
            }

            // Create a test spin result
            var testGrid = new SymbolType[5, 3];
            for (int reel = 0; reel < 5; reel++)
            {
                for (int row = 0; row < 3; row++)
                {
                    testGrid[reel, row] = row == 1 ? SymbolType.Cherry : SymbolType.Lemon;
                }
            }

            var testResult = new SpinResult(testGrid, gameModel.CurrentBet);
            testResult.SetWins(testWins);

            // Show the win presentation
            Debug.Log($"Testing {lineCount} winning lines");
            StartCoroutine(winPresentationManager.ShowWinPresentation(testResult, isTurboMode));
        }

        [ContextMenu("Debug/Toggle Turbo for Testing")]
        public void ToggleTurboForTesting()
        {
            isTurboMode = !isTurboMode;
            Debug.Log($"Turbo mode for testing: {(isTurboMode ? "ON" : "OFF")}");
        }

        [ContextMenu("Debug/Force Jackpot Win")]
        public void ForceJackpotWin()
        {
            // Create a 5-of-a-kind Wild win
            var testGrid = new SymbolType[5, 3];
            for (int reel = 0; reel < 5; reel++)
            {
                for (int row = 0; row < 3; row++)
                {
                    testGrid[reel, row] = row == 1 ? SymbolType.Wild : SymbolType.Cherry;
                }
            }

            var testWins = new List<PaylineWin>
            {
                new PaylineWin(
                    0,
                    SymbolType.Wild,
                    5,
                    1000 * gameModel.BetPerLine,
                    new int[] { 1, 1, 1, 1, 1 }
                )
            };

            var testResult = new SpinResult(testGrid, gameModel.CurrentBet);
            testResult.SetWins(testWins);

            Debug.Log("Testing Jackpot Win!");
            StartCoroutine(winPresentationManager.ShowWinPresentation(testResult, isTurboMode));
        }

        [ContextMenu("Debug/Print Statistics")]
        public void PrintStatistics()
        {
            if (statisticsManager != null)
            {
                Debug.Log("=== GAME STATISTICS ===\n" + statisticsManager.GetStatisticsSummary());
            }
            else
            {
                Debug.LogError("Statistics manager not initialized!");
            }
        }

        public void SetTurboMode(bool enabled)
        {
            isTurboMode = enabled;
        }
    }
}