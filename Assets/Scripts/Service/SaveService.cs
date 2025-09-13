using UnityEngine;

namespace SweetSpin
{
    /// <summary>
    /// Save service using PlayerPrefs
    /// </summary>
    public class SaveService : ISaveService
    {
        private const string CREDITS_KEY = "SweetSpin_Credits";
        private const string STATS_KEY = "SweetSpin_Stats";

        public void SaveCredits(int credits)
        {
            PlayerPrefs.SetInt(CREDITS_KEY, credits);
            PlayerPrefs.Save();
        }

        public int LoadCredits()
        {
            return PlayerPrefs.GetInt(CREDITS_KEY, 1000); // Default 1000 credits
        }

        public void SaveStatistics(GameStatistics stats)
        {
            string json = JsonUtility.ToJson(stats);
            PlayerPrefs.SetString(STATS_KEY, json);
            PlayerPrefs.Save();
        }

        public GameStatistics LoadStatistics()
        {
            if (PlayerPrefs.HasKey(STATS_KEY))
            {
                string json = PlayerPrefs.GetString(STATS_KEY);
                return JsonUtility.FromJson<GameStatistics>(json);
            }
            return new GameStatistics();
        }
    }
}
