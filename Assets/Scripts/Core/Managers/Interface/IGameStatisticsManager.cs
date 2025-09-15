namespace SweetSpin.Core.Managers
{
    /// <summary>
    /// Interface for game statistics tracking and persistence
    /// </summary>
    public interface IGameStatisticsManager
    {
        /// <summary>
        /// Initializes the statistics manager with required dependencies
        /// </summary>
        void Initialize(ISaveService saveService, SlotMachineModel gameModel);

        /// <summary>
        /// Updates statistics after a spin
        /// </summary>
        void UpdateStatistics(SpinResult result);

        /// <summary>
        /// Tracks a win for statistics
        /// </summary>
        void TrackWin(SpinResult result);

        /// <summary>
        /// Saves statistics to persistent storage
        /// </summary>
        void SaveStatistics(GameStatistics stats);

        /// <summary>
        /// Loads statistics from persistent storage
        /// </summary>
        GameStatistics LoadStatistics();

        /// <summary>
        /// Resets all statistics
        /// </summary>
        void ResetStatistics();

        /// <summary>
        /// Gets current statistics summary as a formatted string
        /// </summary>
        string GetStatisticsSummary();
    }
}