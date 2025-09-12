using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using SweetSpin.Core;
using NUnit.Framework;

namespace SweetSpin
{
    public class SlotMachineManager : MonoBehaviour
    {
        [Header("Reels")]
        [SerializeField] private GameObject reelPrefab;
        [SerializeField] private Transform reelContainer;
        [SerializeField] private float reelSpacing = 160f;
        private Reel[] reels = new Reel[5];

        [Header("Symbols")]
        [SerializeField] private SymbolData[] symbolDatabase;

        [Header("UI")]
        [SerializeField] private Button spinButton;
        [SerializeField] private TextMeshProUGUI creditsText;
        [SerializeField] private TextMeshProUGUI betText;
        [SerializeField] private TextMeshProUGUI winText;

        [Header("Game Settings")]
        [SerializeField] private int startingCredits = 1000;
        [SerializeField] private int betPerLine = 1;
        [SerializeField] private int numberOfPaylines = 25;

        private int currentCredits;
        private int currentBet;
        private bool isSpinning = false;

        private PaylineManager paylineManager;
        private SpinResult lastSpinResult;

        // Symbol weights for RNG (higher weight = more common)
        private readonly int[] symbolWeights = { 30, 25, 20, 15, 10, 8, 5, 3 }; // Cherry to Wild

        private void Start()
        {
            ValidateSymbolDatabase();
            CreateReels();
            InitializeGame();

            // Initialize PaylineManager
            paylineManager = new PaylineManager();
            paylineManager.Initialize(symbolDatabase);

            if (spinButton != null)
            {
                spinButton.onClick.AddListener(OnSpinButtonClick);
            }
        }

        private void ValidateSymbolDatabase()
        {
            // Check if symbol database is set in inspector
            if (symbolDatabase == null || symbolDatabase.Length == 0)
            {
                Debug.LogError("Symbol Database is not set or empty in the inspector!");
                return;
            }
        }

        private void CreateReels()
        {
            for (int i = 0; i < 5; i++)
            {
                GameObject reelGO = Instantiate(reelPrefab, reelContainer);
                RectTransform rt = reelGO.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(i * reelSpacing - (2 * reelSpacing), 0); // Center the reels

                Reel reel = reelGO.GetComponent<Reel>();
                if (reel == null)
                    reel = reelGO.AddComponent<Reel>();

                reel.Initialize(symbolDatabase, i);
                reels[i] = reel;
            }
        }

        private void InitializeGame()
        {
            currentCredits = startingCredits;
            currentBet = betPerLine * numberOfPaylines;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (creditsText != null)
                creditsText.text = $"Credits: {currentCredits}";
            if (betText != null)
                betText.text = $"Bet: {currentBet}";
            if (winText != null && winText.text == "")
                winText.text = "Good Luck!";
        }

        private void OnSpinButtonClick()
        {
            if (isSpinning) return;
            if (currentCredits < currentBet)
            {
                if (winText != null)
                    winText.text = "Not enough credits!";
                return;
            }

            StartCoroutine(SpinReels());
        }

        private IEnumerator SpinReels()
        {
            isSpinning = true;

            // Deduct bet
            currentCredits -= currentBet;
            UpdateUI();

            // Clear win text
            if (winText != null)
                winText.text = "Spinning...";

            // Disable spin button
            if (spinButton != null)
                spinButton.interactable = false;

            // Generate random results for each reel
            SymbolType[,] results = GenerateResults();

            // Start each reel spinning with a slight delay
            for (int i = 0; i < reels.Length; i++)
            {
                SymbolType[] reelResults = new SymbolType[3];
                for (int j = 0; j < 3; j++)
                {
                    reelResults[j] = results[i, j];
                }
                reels[i].Spin(reelResults, i * 0.1f);
            }

            // Wait for all reels to stop
            yield return new WaitForSeconds(3f);

            // Evaluate the spin using PaylineManager
            var wins = paylineManager.EvaluatePaylines(results, betPerLine);

            // Create SpinResult object
            lastSpinResult = new SpinResult(results, currentBet);
            lastSpinResult.SetWins(wins);

            // Process the result
            ProcessSpinResult(lastSpinResult);

            // Re-enable spin button
            if (spinButton != null)
                spinButton.interactable = true;

            isSpinning = false;
        }

        private void ProcessSpinResult(SpinResult result)
        {
            // Log debug info (remove in production)
            result.LogDebugInfo();

            // Update credits
            if (result.IsWin)
            {
                currentCredits += result.TotalWin;

                // Animate winning symbols
                foreach (var win in result.Wins)
                {
                    AnimateWinningSymbols(win);
                }

                // Special effects for big wins
                if (result.IsJackpot)
                {
                    // Add jackpot celebration
                    Debug.Log("JACKPOT CELEBRATION!");
                    if (winText != null)
                    {
                        winText.transform.DOScale(1.5f, 0.5f)
                            .SetLoops(4, LoopType.Yoyo)
                            .SetEase(Ease.OutElastic);
                    }
                }
                else if (result.IsMegaWin)
                {
                    // Add mega win effects
                    Debug.Log("MEGA WIN EFFECTS!");
                    if (winText != null)
                    {
                        winText.transform.DOScale(1.3f, 0.4f)
                            .SetLoops(3, LoopType.Yoyo)
                            .SetEase(Ease.OutBounce);
                    }
                }
                else if (result.IsBigWin)
                {
                    // Add big win effects
                    Debug.Log("BIG WIN EFFECTS!");
                    if (winText != null)
                    {
                        winText.transform.DOScale(1.2f, 0.3f)
                            .SetLoops(2, LoopType.Yoyo);
                    }
                }
            }

            // Update win text
            if (winText != null)
            {
                winText.text = result.GetWinMessage();

                // Color code based on win tier
                if (result.IsJackpot)
                    winText.color = new Color(1f, 0.84f, 0f); // Gold
                else if (result.IsMegaWin)
                    winText.color = new Color(1f, 0f, 1f); // Magenta
                else if (result.IsBigWin)
                    winText.color = new Color(0f, 1f, 1f); // Cyan
                else if (result.IsWin)
                    winText.color = new Color(0f, 1f, 0f); // Green
                else
                    winText.color = Color.white; // Normal
            }

            UpdateUI();
        }

        private SymbolType[,] GenerateResults()
        {
            SymbolType[,] results = new SymbolType[5, 3];

            for (int reel = 0; reel < 5; reel++)
            {
                for (int row = 0; row < 3; row++)
                {
                    results[reel, row] = GetRandomSymbol();
                }
            }

            return results;
        }

        private SymbolType GetRandomSymbol()
        {
            int totalWeight = 0;
            foreach (int weight in symbolWeights)
                totalWeight += weight;

            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            for (int i = 0; i < symbolWeights.Length; i++)
            {
                currentWeight += symbolWeights[i];
                if (randomValue < currentWeight)
                {
                    return (SymbolType)i;
                }
            }

            return SymbolType.Cherry; // Default fallback
        }

        // Add this new method to animate winning symbols
        private void AnimateWinningSymbols(PaylineWin win)
        {
            // Animate the winning symbols on each reel
            for (int reelIndex = 0; reelIndex < win.matchCount; reelIndex++)
            {
                if (reelIndex < reels.Length)
                {
                    var symbols = reels[reelIndex].GetVisibleSymbols();
                    int row = win.positions[reelIndex];

                    if (row < symbols.Length)
                    {
                        symbols[row].AnimateWin();
                    }
                }
            }
        }

        [ContextMenu("Display Last Spin Statistics")]
        public void DisplayStatistics()
        {
            if (lastSpinResult != null)
            {
                Debug.Log($"Last Spin Statistics:");
                Debug.Log($"- Win Multiplier: {lastSpinResult.GetWinMultiplier():F1}x");
                Debug.Log($"- Winning Lines: {lastSpinResult.TotalWinningLines}");
                Debug.Log($"- Wild Count: {lastSpinResult.WildCount}");
                Debug.Log($"- Longest Match: {lastSpinResult.LongestLineMatch}");
            }
        }
    }
}
