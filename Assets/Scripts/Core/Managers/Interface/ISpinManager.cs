using System.Collections;

namespace SweetSpin.Core.Managers
{
    /// <summary>
    /// Interface for spin execution management
    /// </summary>
    public interface ISpinManager
    {
        /// <summary>
        /// Initializes the spin manager with required dependencies
        /// </summary>
        void Initialize(SlotMachineConfiguration configuration, 
            SlotMachineModel gameModel, 
            SlotMachineView slotMachineView, 
            GameStateMachine stateMachine, 
            IEventBus eventBus, IAudioService 
            audioService, IPaylineService 
            paylineService, ISaveService 
            saveService);

        /// <summary>
        /// Gets the last spin result
        /// </summary>
        SpinResult LastSpinResult { get; }

        /// <summary>
        /// Validates if a spin can be executed
        /// </summary>
        bool ValidateSpin();

        /// <summary>
        /// Executes a spin cycle
        /// </summary>
        IEnumerator ExecuteSpin(bool isTurboMode = false);

        /// <summary>
        /// Handles reel stop event
        /// </summary>
        void HandleReelStop(int reelIndex);
    }
}