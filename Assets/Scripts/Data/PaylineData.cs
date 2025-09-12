using System;

namespace SweetSpin.Data
{
    /// <summary>
    /// Container for payline data loaded from JSON
    /// </summary>
    [Serializable]
    public class PaylineData
    {
        public PaylineDefinition[] paylines;
    }

    [Serializable]
    public class PaylineDefinition
    {
        public int index;
        public string name;
        public int[] positions; // 5 positions, one for each reel (0=top, 1=middle, 2=bottom)
        public string description; // Optional description for debugging
    }
}