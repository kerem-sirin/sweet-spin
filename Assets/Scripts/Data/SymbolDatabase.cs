using System;
using UnityEngine;

namespace SweetSpin.Data
{
    /// <summary>
    /// Container for symbol data loaded from JSON
    /// </summary>
    [Serializable]
    public class SymbolDatabase
    {
        public string version;
        public SymbolData[] symbols;
    }
}
