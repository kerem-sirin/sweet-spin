using UnityEngine;
using SweetSpin.Core.Managers;

namespace SweetSpin.Core
{
    /// <summary>
    /// Manages game statistics tracking and persistence
    /// </summary>
    public class GameStatisticsManager : IGameStatisticsManager
    {
        private ISaveService saveService;
        private SlotMachineModel gameModel;

        public void Initialize(ISaveService save, SlotMachineModel model)
        {
            saveService = save;
            gameModel = model;
        }

        /// <summary>
        /// Update statistics after a spin
        /// </summary>
        public void UpdateStatistics(SpinResult result)
        {
            var stats = LoadStatistics();

            stats.totalSpins++;
            stats.totalWagered += gameModel.CurrentBet;

            if (result.IsWin)
            {
                stats.totalWins++;
                stats.totalWon += result.TotalWin;

                if (result.TotalWin > stats.biggestWin)
                {
                    stats.biggestWin = result.TotalWin;
                }
            }

            SaveStatistics(stats);
        }

        /// <summary>
        /// Track a win for statistics
        /// </summary>
        public void TrackWin(SpinResult result)
        {
            if (!result.IsWin) return;

            var stats = LoadStatistics();

            stats.totalWins++;
            stats.totalWon += result.TotalWin;

            if (result.TotalWin > stats.biggestWin)
            {
                stats.biggestWin = result.TotalWin;
            }

            SaveStatistics(stats);
        }

        /// <summary>
        /// Save statistics to persistent storage
        /// </summary>
        public void SaveStatistics(GameStatistics stats)
        {
            saveService.SaveStatistics(stats);
        }

        /// <summary>
        /// Load statistics from persistent storage
        /// </summary>
        public GameStatistics LoadStatistics()
        {
            return saveService.LoadStatistics();
        }

        /// <summary>
        /// Reset all statistics
        /// </summary>
        public void ResetStatistics()
        {
            var stats = new GameStatistics();
            SaveStatistics(stats);
            Debug.Log("Statistics reset");
        }

        /// <summary>
        /// Get current statistics summary
        /// </summary>
        public string GetStatisticsSummary()
        {
            var stats = LoadStatistics();

            float winRate = stats.totalSpins > 0 ?
                (float)stats.totalWins / stats.totalSpins * 100f : 0f;

            float rtp = stats.totalWagered > 0 ?
                (float)stats.totalWon / stats.totalWagered * 100f : 0f;

            return $"Total Spins: {stats.totalSpins}\n" +
                   $"Total Wins: {stats.totalWins}\n" +
                   $"Win Rate: {winRate:F1}%\n" +
                   $"RTP: {rtp:F1}%\n" +
                   $"Biggest Win: {stats.biggestWin}\n" +
                   $"Total Wagered: {stats.totalWagered}\n" +
                   $"Total Won: {stats.totalWon}";
        }
    }
}