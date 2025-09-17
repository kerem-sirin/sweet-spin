namespace SweetSpin
{
    /// <summary>
    /// Wrapper for RNG (makes testing easier)
    /// </summary>
    public class RandomService : IRandomService
    {
        private System.Random random = new System.Random();

        public void SetSeed(int seed)
        {
            random = new System.Random(seed);
        }

        public int Range(int min, int max)
        {
            return random.Next(min, max);
        }

        public float Range(float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }

        public SymbolType GetRandomSymbol(SymbolData[] symbolDatabase)
        {
            if (symbolDatabase == null || symbolDatabase.Length == 0)
            {
                return SymbolType.Cherry; // Fallback
            }

            // Calculate total weight
            int totalWeight = 0;
            foreach (var symbol in symbolDatabase)
            {
                totalWeight += symbol.weight;
            }

            // Generate random value
            int randomValue = Range(0, totalWeight);
            int currentWeight = 0;

            // Find the selected symbol
            for (int i = 0; i < symbolDatabase.Length; i++)
            {
                currentWeight += symbolDatabase[i].weight;
                if (randomValue < currentWeight)
                {
                    return symbolDatabase[i].type;
                }
            }

            // Fallback (should never reach here)
            return symbolDatabase[0].type;
        }
    }
}