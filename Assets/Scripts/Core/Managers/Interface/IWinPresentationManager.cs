using System.Collections;

namespace SweetSpin.Core.Managers
{
    /// <summary>
    /// Interface for win presentation and animations
    /// </summary>
    public interface IWinPresentationManager
    {
        /// <summary>
        /// Initializes the win presentation manager with required dependencies
        /// </summary>
        void Initialize(SlotMachineConfiguration configuration, 
            SlotMachineView slotMachineView, 
            GameStateMachine stateMachine, 
            IAudioService audioService);

        /// <summary>
        /// Shows win presentation with animations and sounds
        /// </summary>
        IEnumerator ShowWinPresentation(SpinResult result, bool isTurboMode = false);

        /// <summary>
        /// Determines the win tier based on multiplier
        /// </summary>
        WinTier DetermineWinTier(SpinResult result);

        /// <summary>
        /// Clears all win animations from the view
        /// </summary>
        void ClearWinAnimations();

        /// <summary>
        /// Instantly clears all win animations (for emergency cleanup)
        /// </summary>
        void ClearWinAnimationsInstant();
    }
}