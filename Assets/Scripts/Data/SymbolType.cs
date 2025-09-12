namespace SweetSpin
{
    /// <summary>
    /// Enumeration of all available slot machine symbols.
    /// Order matches the symbol weight array for RNG selection.
    /// </summary>
    public enum SymbolType
    {
        /// <summary>Lowest value fruit symbol</summary>
        Cherry,
        /// <summary>Low value fruit symbol</summary>
        Lemon,
        /// <summary>Low-medium value fruit symbol</summary>
        Orange,
        /// <summary>Medium value fruit symbol</summary>
        Plum,
        /// <summary>Medium-high value symbol</summary>
        Bell,
        /// <summary>High value BAR symbol</summary>
        Bar,
        /// <summary>High value lucky seven</summary>
        Seven,
        /// <summary>Special symbol that substitutes for any other</summary>
        Wild
    }
}