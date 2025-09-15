using System.Collections;
using UnityEngine;

namespace SweetSpin.Core.Managers
{
    /// <summary>
    /// Interface for auto-play execution management
    /// </summary>
    public interface IAutoPlayExecutor
    {
        /// <summary>
        /// Initializes the auto-play executor with required dependencies
        /// </summary>
        void Initialize(SlotMachineConfiguration configuration, 
            SlotMachineModel gameModel, 
            GameStateMachine stateMachine, 
            ISpinManager spinManager, 
            IWinPresentationManager winPresentationManager, 
            IGameStatisticsManager statisticsManager, 
            IAutoPlayService autoPlayService, 
            IAudioService audioService, 
            IEventBus eventBus,
            MonoBehaviour gameController);

        /// <summary>
        /// Executes the auto-play sequence
        /// </summary>
        IEnumerator ExecuteAutoPlaySequence();

        /// <summary>
        /// Stops auto-play immediately
        /// </summary>
        void StopAutoPlay();

        /// <summary>
        /// Cleans up resources and event subscriptions
        /// </summary>
        void Cleanup();
    }
}