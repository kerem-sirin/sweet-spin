using System;

namespace SweetSpin
{
    /// <summary>
    /// Defines a single payline pattern across the 5x3 reel grid.
    /// Each pattern represents a path through the symbols that can result in wins.
    /// </summary>
    [Serializable]
    public class PaylinePattern
    {
        /// <summary>Unique identifier for this payline (0-24)</summary>
        public int index;
        /// <summary>Row positions for each reel (0=top, 1=middle, 2=bottom)</summary>
        public int[] positions; // 5 positions, one for each reel
        /// <summary>Display name for UI (e.g., "V-Shape", "Middle Line")</summary>
        public string name;
    }
}