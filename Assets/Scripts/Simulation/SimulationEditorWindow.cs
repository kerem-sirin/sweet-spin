using UnityEngine;
using UnityEditor;
using SweetSpin.Core;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace SweetSpin.Simulation
{
    /// <summary>
    /// Unity Editor window for running slot machine simulations
    /// </summary>
    public class SimulationEditorWindow : EditorWindow
    {
        // Configuration
        private SlotMachineConfiguration configuration;
        private int startingCredits = 1000;
        private int betPerLine = 1;
        private int maxTurns = 1000;

        // Results display
        private string lastResultPath = "";
        private SimulationReport lastReport;
        private Vector2 scrollPosition;
        private bool showDetailedStats = false;

        [MenuItem("Tools/Sweet Spin/Simulation Runner")]
        public static void ShowWindow()
        {
            var window = GetWindow<SimulationEditorWindow>("Slot Simulation");
            window.minSize = new Vector2(400, 500);
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawConfiguration();
            DrawSimulationButton();
            DrawResults();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);

            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.LabelField("Slot Machine Simulator", headerStyle);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Run simulations to test game balance and RTP", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.Space(10);
            DrawHorizontalLine();
        }

        private void DrawConfiguration()
        {
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            configuration = (SlotMachineConfiguration)EditorGUILayout.ObjectField(
                "Slot Configuration",
                configuration,
                typeof(SlotMachineConfiguration),
                false
            );

            if (configuration == null)
            {
                EditorGUILayout.HelpBox("Please select a Slot Machine Configuration asset", MessageType.Warning);
            }
            else
            {
                // Show configuration info
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField("Paylines", configuration.paylineCount);
                EditorGUILayout.IntField("Reels", configuration.reelCount);
                EditorGUILayout.IntField("Rows", configuration.rowCount);
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Simulation Parameters", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            startingCredits = EditorGUILayout.IntField(
                new GUIContent("Starting Credits", "Initial credits for the simulation"),
                startingCredits
            );
            startingCredits = Mathf.Max(100, startingCredits);

            betPerLine = EditorGUILayout.IntSlider(
                new GUIContent("Bet Per Line", "Fixed bet amount per payline"),
                betPerLine,
                1,
                configuration != null ? configuration.maxBetPerLine : 10
            );

            maxTurns = EditorGUILayout.IntField(
                new GUIContent("Max Turns", "Maximum number of spins before stopping"),
                maxTurns
            );
            maxTurns = Mathf.Max(10, maxTurns);

            if (configuration != null)
            {
                int totalBetPerSpin = betPerLine * configuration.paylineCount;
                EditorGUILayout.HelpBox($"Total bet per spin: {totalBetPerSpin} credits", MessageType.Info);
            }

            EditorGUILayout.Space(10);
            DrawHorizontalLine();
        }

        private void DrawSimulationButton()
        {
            EditorGUILayout.Space(10);

            GUI.enabled = configuration != null;

            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                fixedHeight = 35
            };

            if (GUILayout.Button("Run Simulation", buttonStyle))
            {
                RunSimulation();
            }

            GUI.enabled = true;

            EditorGUILayout.Space(5);

            // Additional buttons
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Open Results Folder"))
            {
                OpenResultsFolder();
            }

            if (GUILayout.Button("Clean Old Results"))
            {
                if (EditorUtility.DisplayDialog(
                    "Clean Old Results",
                    "This will delete old simulation files, keeping only the 50 most recent.\n\nContinue?",
                    "Yes",
                    "Cancel"))
                {
                    SimulationFileManager.CleanupOldSimulations(50);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            DrawHorizontalLine();
        }

        private void DrawResults()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Last Simulation Results", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (lastReport == null)
            {
                EditorGUILayout.LabelField("No simulation run yet", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Basic results
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"File: {System.IO.Path.GetFileName(lastResultPath)}");
            EditorGUILayout.LabelField($"Total Turns: {lastReport.totalTurns}");
            EditorGUILayout.LabelField($"Final Credits: {lastReport.finalCredits}");
            EditorGUILayout.LabelField($"Total Bet: {lastReport.totalBet}");
            EditorGUILayout.LabelField($"Total Won: {lastReport.totalWon}");

            // RTP with color coding
            var rtpColor = lastReport.rtp >= 95f ? Color.green :
                          lastReport.rtp >= 85f ? Color.yellow :
                          Color.red;
            var originalColor = GUI.color;
            GUI.color = rtpColor;
            EditorGUILayout.LabelField($"RTP: {lastReport.rtp:F2}%", EditorStyles.boldLabel);
            GUI.color = originalColor;

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Detailed statistics
            showDetailedStats = EditorGUILayout.Foldout(showDetailedStats, "Detailed Statistics");

            if (showDetailedStats && lastReport.statistics != null)
            {
                var stats = lastReport.statistics;

                // Win Statistics
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Win Statistics", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Hit Frequency: {stats.hitFrequency:F2}%");
                EditorGUILayout.LabelField($"Total Wins: {stats.totalWins}");
                EditorGUILayout.LabelField($"Total Losses: {stats.totalLosses}");
                EditorGUILayout.LabelField($"Average Win: {stats.averageWinAmount:F2}");
                EditorGUILayout.LabelField($"Biggest Win: {stats.biggestWin} (Turn {stats.biggestWinTurn})");
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(5);

                // Win Distribution
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Win Distribution", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Small Wins (<5x): {stats.smallWins}");
                EditorGUILayout.LabelField($"Medium Wins (5-10x): {stats.mediumWins}");
                EditorGUILayout.LabelField($"Big Wins (10-25x): {stats.bigWins}");
                EditorGUILayout.LabelField($"Mega Wins (25-50x): {stats.megaWins}");
                EditorGUILayout.LabelField($"Jackpot Wins (50x+): {stats.jackpotWins}");
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(5);

                // Match Distribution
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Match Distribution", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"3 of a Kind: {stats.threeOfKindWins}");
                EditorGUILayout.LabelField($"4 of a Kind: {stats.fourOfKindWins}");
                EditorGUILayout.LabelField($"5 of a Kind: {stats.fiveOfKindWins}");
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(5);

                // Most Common Lines
                if (stats.mostCommonWinningLines != null && stats.mostCommonWinningLines.Length > 0)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Most Common Winning Lines", EditorStyles.boldLabel);
                    foreach (var line in stats.mostCommonWinningLines)
                    {
                        EditorGUILayout.LabelField($"{line.lineName}: {line.hitCount} hits ({line.hitPercentage:F1}%)");
                    }
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space(5);

                // Symbol Frequency
                if (stats.symbolWinFrequency != null && stats.symbolWinFrequency.Length > 0)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Symbol Win Frequency", EditorStyles.boldLabel);
                    foreach (var symbol in stats.symbolWinFrequency)
                    {
                        EditorGUILayout.LabelField(
                            $"{symbol.symbolType}: {symbol.winCount} wins ({symbol.winPercentage:F1}%) - Avg: {symbol.averagePrize:F0}"
                        );
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void RunSimulation()
        {
            if (configuration == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a configuration asset", "OK");
                return;
            }

            // Start timer
            var stopwatch = Stopwatch.StartNew();

            // Run simulation
            var controller = new SimulationController(configuration);
            lastReport = controller.RunSimulation(startingCredits, betPerLine, maxTurns);

            // Save results
            lastResultPath = SimulationFileManager.SaveSimulationReport(lastReport);

            stopwatch.Stop();

            // Show completion dialog
            string message = $"Simulation completed in {stopwatch.ElapsedMilliseconds}ms\n\n" +
                           $"Turns: {lastReport.totalTurns}\n" +
                           $"RTP: {lastReport.rtp:F2}%\n" +
                           $"Hit Frequency: {lastReport.statistics.hitFrequency:F2}%\n\n" +
                           $"Results saved to:\n{lastResultPath}";

            EditorUtility.DisplayDialog("Simulation Complete", message, "OK");

            // Refresh the asset database to show the new file
            AssetDatabase.Refresh();
        }

        private void OpenResultsFolder()
        {
            string folderPath = System.IO.Path.GetFullPath("Assets/SimulationResults");

            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            EditorUtility.RevealInFinder(folderPath);
        }

        private void DrawHorizontalLine()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }
    }
}