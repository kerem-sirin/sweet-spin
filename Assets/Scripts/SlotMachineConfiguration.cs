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
}