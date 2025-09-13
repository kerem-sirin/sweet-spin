using UnityEngine;

namespace SweetSpin
{
    /// <summary>
    /// Save service for persistence
    /// </summary>
    public interface ISaveService
    {
        void SaveCredits(int credits);
        int LoadCredits();
        void SaveStatistics(GameStatistics stats);
        GameStatistics LoadStatistics();
    }
}
