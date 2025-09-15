using System.Collections;
using UnityEngine;
using SweetSpin.Core.Managers;

namespace SweetSpin.Core
{
    /// <summary>
    /// Manages win presentation, animations, and visual feedback
    /// </summary>
    public class WinPresentationManager : MonoBehaviour, IWinPresentationManager
    {
        private SlotMachineConfiguration configuration;
        private SlotMachineView slotMachineView;
        private GameStateMachine stateMachine;
        private IAudioService audioService;

        public void Initialize(
            SlotMachineConfiguration config,
            SlotMachineView view,
            GameStateMachine gameStateMachine,
            IAudioService audio)
        {
            configuration = config;
            slotMachineView = view;
            stateMachine = gameStateMachine;
            audioService = audio;
        }

        /// <summary>
        /// Show win presentation with animations and sounds
        /// </summary>
        public IEnumerator ShowWinPresentation(SpinResult result, bool isTurboMode = false)
        {
            // Transition to showing win state
            stateMachine.TransitionTo(GameState.ShowingWin);

            if (result.IsWin)
            {
                // Determine win tier
                WinTier tier = DetermineWinTier(result);

                // Play appropriate sound
                audioService.PlayWinSound(tier);

                // Show win message with effects
                slotMachineView.ShowWinMessage(result.GetWinMessage(), tier);

                // Start the animation coroutine and wait for it to complete
                yield return StartCoroutine(slotMachineView.AnimateMultipleWinningLinesCoroutine(result.Wins, isTurboMode));

                // Additional hold time after all animations complete
                float additionalHoldTime = isTurboMode ?
                    configuration.turboSequentialDelay :
                    configuration.sequentialAnimationDelay;
                yield return new WaitForSeconds(additionalHoldTime);

                // Clear win animations after presentation
                ClearWinAnimations();
            }
            else
            {
                // Play no-win sound for losses
                audioService.PlayWinSound(WinTier.None);

                slotMachineView.ShowWinMessage("Try Again!", WinTier.None);

                // Only wait 1 frame to let message show briefly
                yield return null;
            }

            // Return to idle state
            stateMachine.TransitionTo(GameState.Idle);
        }

        /// <summary>
        /// Determine the win tier based on multiplier
        /// </summary>
        public WinTier DetermineWinTier(SpinResult result)
        {
            float multiplier = result.GetWinMultiplier();

            if (multiplier >= 50f) return WinTier.Jackpot;
            if (multiplier >= 25f) return WinTier.Mega;
            if (multiplier >= 10f) return WinTier.Big;
            if (multiplier >= 5f) return WinTier.Medium;
            if (multiplier > 0) return WinTier.Small;
            return WinTier.None;
        }

        /// <summary>
        /// Clear all win animations from the view
        /// </summary>
        public void ClearWinAnimations()
        {
            slotMachineView.ClearAllWinAnimations();
        }

        /// <summary>
        /// Instantly clear all win animations (for emergency cleanup)
        /// </summary>
        public void ClearWinAnimationsInstant()
        {
            if (slotMachineView != null && slotMachineView.Reels != null)
            {
                foreach (var reel in slotMachineView.Reels)
                {
                    reel?.ClearWinAnimationsInstant();
                }
            }
        }
    }
}