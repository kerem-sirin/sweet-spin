using System;

namespace SweetSpin
{
    [Serializable]
    public class PaylinePattern
    {
        public int index;
        public int[] positions; // 5 positions, one for each reel
        public string name;
    }
}