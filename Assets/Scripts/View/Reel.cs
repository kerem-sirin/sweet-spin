using DG.Tweening;
using SweetSpin.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SweetSpin
{
    /// <summary>
    /// Controls a single reel column in the slot machine.
    /// Manages symbol creation, spinning animation, and final positioning.
    /// Attached to each reel prefab instance.
    /// </summary>
    public class Reel : MonoBehaviour
    {
        [Header("Reel Configuration")]
        [SerializeField] private int reelIndex; // 0-4 for identifying which reel this is
        [SerializeField] private GameObject symbolPrefab;
        [SerializeField] private Transform symbolContainer;
        [SerializeField] private int visibleSymbols = 3;
        [SerializeField] private int bufferSymbols = 2; // Extra symbols above and below for smooth spinning

        // Default values from configuration
        private float defaultSpinSpeed;
        private float defaultSpinDuration;
        private float defaultStopDelay;

        // Current values for active spin (can be different for turbo mode)
        private float currentSpinSpeed;
        private float currentSpinDuration;

        private List<ReelSymbol> symbols = new List<ReelSymbol>();
        private SymbolData[] availableSymbols;
        private bool isSpinning = false;
        private float currentPosition = 0;
        private Sequence spinSequence;

        // Dynamic symbol height - retrieved from prefab
        private float symbolHeight = 150f; // Default fallback value

        // Final symbols that will be shown when reel stops
        private SymbolType[] targetSymbols = new SymbolType[3];

        public bool IsSpinning => isSpinning;
        public int ReelIndex => reelIndex;
        public float SymbolHeight => symbolHeight;

        private float defaultSnapDuration;
        private float turboSnapDuration;
        private bool isInTurboMode = false;

        private void Awake()
        {
            if (symbolContainer == null)
                symbolContainer = transform;

            // Get symbol height from prefab
            if (symbolPrefab != null)
            {
                ReelSymbol prefabSymbol = symbolPrefab.GetComponent<ReelSymbol>();
                if (prefabSymbol != null)
                {
                    // Instantiate temporarily to get the actual height
                    GameObject tempSymbol = Instantiate(symbolPrefab, transform);
                    RectTransform rt = tempSymbol.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        // Need this or height returns 0
                        Canvas.ForceUpdateCanvases();
                        symbolHeight = rt.rect.height;

                        // If height is 0 or negative, use the sizeDelta
                        if (symbolHeight <= 0)
                        {
                            symbolHeight = rt.sizeDelta.y;
                        }

                        // If still invalid, use default
                        if (symbolHeight <= 0)
                        {
                            Debug.LogWarning($"Could not get valid symbol height from prefab. Using default: 150");
                            symbolHeight = 150f;
                        }
                        else
                        {
                            Debug.Log($"Reel {reelIndex}: Dynamic symbol height = {symbolHeight}");
                        }
                    }
                    Destroy(tempSymbol);
                }
            }
        }

        public void Initialize(int index, SymbolData[] symbolData, float spinSpeed, float spinDuration,
                              float stopDelay, float snapDuration, float turboSnapDur)
        {
            reelIndex = index;
            availableSymbols = symbolData;

            // Store default configuration values
            defaultSpinSpeed = spinSpeed;
            defaultSpinDuration = spinDuration;
            defaultStopDelay = stopDelay;

            // Set current values to defaults initially
            currentSpinSpeed = spinSpeed;
            currentSpinDuration = spinDuration;

            // Store snap durations
            defaultSnapDuration = snapDuration;  
            turboSnapDuration = turboSnapDur;

            CreateSymbols();
            SetRandomSymbols();
            SnapToGrid();
            Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// Animate a specific symbol position with win effect
        /// </summary>
        /// <param name="rowIndex">Row position of the symbol</param>
        /// <param name="frameColor">Color for the win frame</param>
        /// <param name="animationDelay">Delay before starting animation</param>
        /// <param name="animationDuration">Total duration for the win animation</param>
        public void AnimateSymbolAt(int rowIndex, Color frameColor, float animationDelay, float animationDuration)
        {
            var visibleSymbols = GetVisibleSymbols();
            if (rowIndex >= 0 && rowIndex < visibleSymbols.Length)
            {
                visibleSymbols[rowIndex].AnimateWin(frameColor, animationDelay, animationDuration);
            }
        }

        /// <summary>
        /// Clear win animations from all symbols in this reel
        /// </summary>
        public void ClearWinAnimations()
        {
            foreach (var symbol in symbols)
            {
                if (symbol != null)
                {
                    symbol.StopWinAnimation();
                }
            }
        }

        /// <summary>
        /// Instantly clear all win animations (for emergency cleanup)
        /// </summary>
        public void ClearWinAnimationsInstant()
        {
            foreach (var symbol in symbols)
            {
                if (symbol != null)
                {
                    symbol.ResetToNormal();
                }
            }
        }

        private void CreateSymbols()
        {
            // Clear existing symbols
            foreach (var symbol in symbols)
            {
                if (symbol != null)
                    Destroy(symbol.gameObject);
            }
            symbols.Clear();

            // Create symbols (visible + buffer)
            int totalSymbols = visibleSymbols + (bufferSymbols * 2);
            for (int i = 0; i < totalSymbols; i++)
            {
                GameObject symbolGO = Instantiate(symbolPrefab, symbolContainer);
                ReelSymbol symbol = symbolGO.GetComponent<ReelSymbol>();

                // Get the actual height from the first created symbol if not already set
                if (i == 0 && symbolHeight <= 0)
                {
                    RectTransform firstRt = symbolGO.GetComponent<RectTransform>();
                    Canvas.ForceUpdateCanvases();
                    symbolHeight = firstRt.rect.height > 0 ? firstRt.rect.height : 150f;
                    Debug.Log($"Symbol height retrieved from first instantiated symbol: {symbolHeight}");
                }

                // Position symbol using the dynamic height
                RectTransform rt = symbolGO.GetComponent<RectTransform>();
                float yPos = CalculateSymbolYPosition(i);
                rt.anchoredPosition = new Vector2(0, yPos);

                symbols.Add(symbol);
            }
        }

        private float CalculateSymbolYPosition(int index)
        {
            // Calculate Y position based on index
            // Center the visible symbols (index bufferSymbols to bufferSymbols+visibleSymbols-1 should be centered)
            return (index - bufferSymbols - 1) * -symbolHeight;
        }

        private void SetRandomSymbols()
        {
            foreach (var symbol in symbols)
            {
                var randomSymbol = availableSymbols[Random.Range(0, availableSymbols.Length)];
                symbol.SetSymbol(randomSymbol);
            }
        }

        public void Spin(SymbolType[] resultSymbols, float speed, float duration, float startDelay)
        {
            if (isSpinning) return;

            targetSymbols = resultSymbols;
            isSpinning = true;

            // Use the passed parameters for this spin
            currentSpinSpeed = speed;
            currentSpinDuration = duration;

            // Kill any existing spin sequence
            spinSequence?.Kill();

            spinSequence = DOTween.Sequence();

            // Add start delay for staggered reel starts
            spinSequence.AppendInterval(startDelay);

            // Start spinning animation
            spinSequence.AppendCallback(() => StartSpinning());

            // Spin for the specified duration
            spinSequence.AppendInterval(duration);

            // Stop and snap to result
            spinSequence.AppendCallback(() => StopSpinning());

            isInTurboMode = speed > defaultSpinSpeed;
        }

        private void StartSpinning()
        {
            StartCoroutine(SpinAnimation());
        }

        private IEnumerator SpinAnimation()
        {
            while (isSpinning)
            {
                // NOW using currentSpinSpeed instead of spinSpeed
                currentPosition += currentSpinSpeed * Time.deltaTime;

                foreach (var symbol in symbols)
                {
                    RectTransform rt = symbol.RectTransform;
                    Vector2 pos = rt.anchoredPosition;

                    // Use currentSpinSpeed here too
                    pos.y -= currentSpinSpeed * Time.deltaTime;

                    // Calculate wrap-around threshold using dynamic height
                    float wrapThreshold = -(bufferSymbols + visibleSymbols - 1) * symbolHeight;
                    float wrapDistance = symbols.Count * symbolHeight;

                    // Wrap around when symbol goes too far down
                    if (pos.y < wrapThreshold)
                    {
                        pos.y += wrapDistance;
                        // Set new random symbol when wrapping
                        var randomSymbol = availableSymbols[Random.Range(0, availableSymbols.Length)];
                        symbol.SetSymbol(randomSymbol);
                    }

                    rt.anchoredPosition = pos;
                }

                yield return null;
            }
        }

        private void StopSpinning()
        {
            StopAllCoroutines();

            // Set the target symbols to the visible positions
            for (int i = 0; i < visibleSymbols; i++)
            {
                if (i < targetSymbols.Length && i + bufferSymbols < symbols.Count)
                {
                    var targetSymbolData = System.Array.Find(availableSymbols, s => s.type == targetSymbols[i]);
                    if (targetSymbolData != null)
                    {
                        symbols[i + bufferSymbols].SetSymbol(targetSymbolData);
                    }
                }
            }

            // Snap positions to grid
            SnapToGrid();

            // Publish reel stopped event for audio
            var eventBus = ServiceLocator.Instance.Get<IEventBus>();
            eventBus?.Publish(new ReelStoppedEvent(reelIndex));

            isSpinning = false;
        }

        private void SnapToGrid()
        {
            float snapDuration = isInTurboMode ? turboSnapDuration : defaultSnapDuration;

            for (int i = 0; i < symbols.Count; i++)
            {
                RectTransform rt = symbols[i].RectTransform;
                float targetY = CalculateSymbolYPosition(i);

                // Animate snap to position
                rt.DOAnchorPosY(targetY, snapDuration)
                    .SetEase(Ease.OutBounce);
            }
        }

        public ReelSymbol[] GetVisibleSymbols()
        {
            List<ReelSymbol> visible = new List<ReelSymbol>();
            for (int i = bufferSymbols; i < bufferSymbols + visibleSymbols; i++)
            {
                if (i < symbols.Count)
                    visible.Add(symbols[i]);
            }
            return visible.ToArray();
        }

        public SymbolType GetSymbolAt(int row)
        {
            if (row < 0 || row >= visibleSymbols) return SymbolType.Cherry;

            int index = bufferSymbols + row;
            if (index < symbols.Count)
                return symbols[index].Type;

            return SymbolType.Cherry;
        }

        // Helper method to get world position of a symbol at a specific row (useful for line rendering)
        public Vector3 GetSymbolWorldPosition(int row)
        {
            if (row < 0 || row >= visibleSymbols)
                return transform.position;

            int index = bufferSymbols + row;
            if (index < symbols.Count && symbols[index] != null)
            {
                return symbols[index].transform.position;
            }

            // Fallback: calculate position
            float yOffset = (row - 1) * -symbolHeight; // row 1 (middle) is at 0
            return transform.position + new Vector3(0, yOffset, 0);
        }
    }
}