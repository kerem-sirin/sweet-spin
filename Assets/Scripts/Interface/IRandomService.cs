namespace SweetSpin
{
    /// <summary>
    /// Random number generation service (for testability)
    /// </summary>
    public interface IRandomService
    {
        void SetSeed(int seed);
        int Range(int min, int max);
        float Range(float min, float max);
        SymbolType GetRandomSymbol(int[] weights);
    }
}