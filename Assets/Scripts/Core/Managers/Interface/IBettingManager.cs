namespace SweetSpin.Core.Managers
{
    /// <summary>
    /// Interface for betting and credit management
    /// </summary>
    public interface IBettingManager
    {
        /// <summary>
        /// Initializes the betting manager with required dependencies
        /// </summary>
        void Initialize(SlotMachineModel gameModel, 
            SlotMachineView slotMachineView, 
            IEventBus eventBus, IAudioService 
            audioService, ISaveService 
            saveService);

        /// <summary>
        /// Changes the bet per line by the specified direction
        /// </summary>
        void ChangeBet(int direction);

        /// <summary>
        /// Validates if the current bet is valid
        /// </summary>
        bool ValidateBet();

        /// <summary>
        /// Gets the current total bet amount
        /// </summary>
        int GetCurrentBet();

        /// <summary>
        /// Gets the current bet per line
        /// </summary>
        int GetBetPerLine();

        /// <summary>
        /// Handles insufficient credits situation
        /// </summary>
        void HandleInsufficientCredits();

        /// <summary>
        /// Adds credits to the player's balance
        /// </summary>
        void AddCredits(int amount);

        /// <summary>
        /// Sets credits to a specific amount
        /// </summary>
        void SetCredits(int credits);

        /// <summary>
        /// Gets current credits
        /// </summary>
        int GetCredits();

        /// <summary>
        /// Handles add credits request event
        /// </summary>
        void OnAddCreditsRequest(AddCreditsRequestEvent e);
    }
}