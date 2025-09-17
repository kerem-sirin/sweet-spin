using SweetSpin.Data;
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

        [Header("Default Mode Settings")]
        public float reelSpacing = 160f;
        public float spinSpeed = 1600;
        public float spinDuration = 2f;
        public float reelStopDelay = 0.3f;
        public float snapDuration = 0.3f;
        public float autoPlayDelay = 1f;

        [Header("Turbo Mode Settings")]
        public bool turboModeEnabled = false;
        public float turboSpinSpeed = 3200f;      // 2x normal speed
        public float turboSpinDuration = 0.5f;    // Much shorter than normal 2f
        public float turboReelStopDelay = 0.05f;  // Almost instant
        public float turboWinDisplayTime = 0.5f;  // Shorter win display
        public float turboSnapDuration = 0.1f;
        public float autoPlayDelayTurbo = 0.5f;

        [Header("Win Animation Settings")]
        public float sequentialAnimationDelay = 1f; // Delay between different winning lines
        public float turboSequentialDelay = 0.3f; // Faster delay for turbo mode

        [Header("Symbols")]
        [SerializeField] private TextAsset symbolJSON; // JSON file containing symbol configuration
        public SymbolData[] symbolDatabase
        {
            get
            {
                if (!symbolsLoaded || _symbolDatabase == null)
                {
                    LoadSymbolsFromJSON();
                }
                return _symbolDatabase;
            }
        }

        private SymbolData[] _symbolDatabase;
        private bool symbolsLoaded = false;


        [Header("Win Frame Colors")]
        [Tooltip("Colors for win frames on different paylines")]
        public Color[] winFrameColors;

        [Header("Paylines")]
        [SerializeField] private TextAsset paylineJSONFile; // JSON file containing payline configuration

        private PaylinePattern[] cachedPaylinePatterns; // Cached parsed patterns
        private int cachedPaylineCount; // Cached count from parsed JSON

        // Property to get payline count
        public int paylineCount
        {
            get
            {
                if (cachedPaylinePatterns == null || cachedPaylinePatterns.Length == 0)
                {
                    LoadPaylinesFromJSON();
                }
                return cachedPaylineCount;
            }
        }

        // Property to get payline patterns
        public PaylinePattern[] paylinePatterns
        {
            get
            {
                if (cachedPaylinePatterns == null || cachedPaylinePatterns.Length == 0)
                {
                    LoadPaylinesFromJSON();
                }
                return cachedPaylinePatterns;
            }
        }

        [Header("Betting")]
        public int startingCredits = 1000;
        public int minBetPerLine = 1;
        public int maxBetPerLine = 10;
        public int defaultBetPerLine = 1;

        [Header("Win Multipliers")]
        public float bigWinMultiplier = 10f;
        public float megaWinMultiplier = 25f;
        public float jackpotMultiplier = 50f;

        // You can also add a method to safely get colors:
        public Color GetWinFrameColor(int index)
        {
            if (winFrameColors != null && index >= 0 && index < winFrameColors.Length)
            {
                return winFrameColors[index];
            }
            return Color.yellow; // Default fallback
        }

        /// <summary>
        /// Loads and parses payline patterns from JSON file
        /// </summary>
        private void LoadPaylinesFromJSON()
        {
            if (paylineJSONFile == null)
            {
                Debug.LogError("SlotMachineConfiguration: Payline JSON file is not assigned!");
                cachedPaylinePatterns = new PaylinePattern[0];
                cachedPaylineCount = 0;
                return;
            }

            string jsonContent = paylineJSONFile.text;

            if (string.IsNullOrEmpty(jsonContent))
            {
                Debug.LogError("SlotMachineConfiguration: Payline JSON file is empty!");
                cachedPaylinePatterns = new PaylinePattern[0];
                cachedPaylineCount = 0;
                return;
            }

            try
            {
                var paylineData = JsonUtility.FromJson<PaylineData>(jsonContent);

                if (paylineData == null || paylineData.paylines == null || paylineData.paylines.Length == 0)
                {
                    Debug.LogError("SlotMachineConfiguration: Failed to parse paylines from JSON - invalid structure!");
                    cachedPaylinePatterns = new PaylinePattern[0];
                    cachedPaylineCount = 0;
                    return;
                }

                cachedPaylinePatterns = new PaylinePattern[paylineData.paylines.Length];

                for (int i = 0; i < paylineData.paylines.Length; i++)
                {
                    var def = paylineData.paylines[i];

                    // Validate each payline
                    if (def.positions == null || def.positions.Length != 5)
                    {
                        Debug.LogError($"SlotMachineConfiguration: Payline {i} has invalid positions array! Expected 5 positions.");
                        cachedPaylinePatterns = new PaylinePattern[0];
                        cachedPaylineCount = 0;
                        return;
                    }

                    // Validate position values (should be 0, 1, or 2 for 3 rows)
                    for (int j = 0; j < def.positions.Length; j++)
                    {
                        if (def.positions[j] < 0 || def.positions[j] >= rowCount)
                        {
                            Debug.LogError($"SlotMachineConfiguration: Payline {i} has invalid position value {def.positions[j]} at reel {j}! Must be between 0 and {rowCount - 1}.");
                            cachedPaylinePatterns = new PaylinePattern[0];
                            cachedPaylineCount = 0;
                            return;
                        }
                    }

                    cachedPaylinePatterns[i] = new PaylinePattern
                    {
                        index = def.index,
                        name = def.name,
                        positions = def.positions
                    };
                }

                cachedPaylineCount = cachedPaylinePatterns.Length;
                Debug.Log($"SlotMachineConfiguration: Successfully loaded {cachedPaylineCount} paylines from JSON");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SlotMachineConfiguration: Exception while parsing payline JSON: {e.Message}");
                cachedPaylinePatterns = new PaylinePattern[0];
                cachedPaylineCount = 0;
            }
        }

        private void LoadSymbolsFromJSON()
        {
            if (symbolJSON == null)
            {
                Debug.LogError("SlotMachineConfiguration: Symbol JSON file is not assigned!");
                _symbolDatabase = new SymbolData[0];
                return;
            }

            string jsonContent = symbolJSON.text;

            if (string.IsNullOrEmpty(jsonContent))
            {
                Debug.LogError("SlotMachineConfiguration: Symbol JSON file is empty!");
                _symbolDatabase = new SymbolData[0];
                return;
            }

            try
            {
                // Parse JSON
                var symbolsData = JsonUtility.FromJson<SymbolDatabase>(jsonContent);

                if (symbolsData == null || symbolsData.symbols == null || symbolsData.symbols.Length == 0)
                {
                    Debug.LogError("SlotMachineConfiguration: Failed to parse symbols from JSON - invalid structure!");
                    _symbolDatabase = new SymbolData[0];
                    return;
                }

                // Create SymbolData array
                _symbolDatabase = new SymbolData[symbolsData.symbols.Length];

                // Load sprites and populate
                for (int i = 0; i < symbolsData.symbols.Length; i++)
                {
                    var jsonSymbol = symbolsData.symbols[i];

                    // Load sprite from Resources
                    Sprite sprite = Resources.Load<Sprite>($"Symbols/Symbol{i}");

                    if (sprite == null)
                    {
                        Debug.LogWarning($"SlotMachineConfiguration: Could not load sprite at 'Resources/Symbols/Symbol{i}'");
                    }

                    // Create SymbolData
                    symbolDatabase[i] = new SymbolData
                    {
                        type = (SymbolType)jsonSymbol.type,
                        name = jsonSymbol.name,
                        sprite = sprite,
                        weight = jsonSymbol.weight,
                        payouts = jsonSymbol.payouts
                    };
                }

                symbolsLoaded = true;
                Debug.Log($"SlotMachineConfiguration: Successfully loaded {symbolDatabase.Length} symbols from JSON");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SlotMachineConfiguration: Exception while parsing symbol JSON: {e.Message}");
                _symbolDatabase = new SymbolData[0];
            }
        }

        // Optimized loading without sprites (for simulation)
        public void LoadSymbolsForSimulation()
        {
            if (symbolJSON == null) return;

            var symbolsData = JsonUtility.FromJson<SymbolDatabase>(symbolJSON.text);
            _symbolDatabase = new SymbolData[symbolsData.symbols.Length];

            for (int i = 0; i < symbolsData.symbols.Length; i++)
            {
                var jsonSymbol = symbolsData.symbols[i];

                // Skip sprite loading for simulation!
                _symbolDatabase[i] = new SymbolData
                {
                    type = (SymbolType)jsonSymbol.type,
                    name = jsonSymbol.name,
                    sprite = null,  // No sprite needed for simulation
                    weight = jsonSymbol.weight,
                    payouts = jsonSymbol.payouts
                };
            }
            symbolsLoaded = true;
            Debug.Log($"Loaded {_symbolDatabase.Length} symbols for simulation (no sprites)");
        }

        // Reload paylines when JSON changes in inspector
        private void OnValidate()
        {
            // Force reload when JSON files are changed in inspector
            cachedPaylinePatterns = null;
            cachedPaylineCount = 0;
            symbolsLoaded = false;
        }
    }
}