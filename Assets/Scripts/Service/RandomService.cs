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

        public SymbolType GetRandomSymbol(int[] weights)
        {
            int totalWeight = 0;
            foreach (int weight in weights)
                totalWeight += weight;

            int randomValue = Range(0, totalWeight);
            int currentWeight = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                currentWeight += weights[i];
                if (randomValue < currentWeight)
                {
                    return (SymbolType)i;
                }
            }

            return SymbolType.Cherry;
        }
    }
}