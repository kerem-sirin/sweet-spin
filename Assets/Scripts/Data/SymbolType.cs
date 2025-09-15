namespace SweetSpin
{
    /// <summary>
    /// Enumeration of all available slot machine symbols.
    /// Order matches the symbol weight array for RNG selection.
    /// </summary>
    public enum SymbolType
    {
        Cherry,      // 0 - Lowest
        Grape,       // 1
        Lemon,       // 2
        Orange,      // 3
        Candy,       // 4
        Plum,        // 5
        Bell,        // 6
        Bar,         // 7
        Seven,       // 8
        Wild         // 9 - Highest
    }
}