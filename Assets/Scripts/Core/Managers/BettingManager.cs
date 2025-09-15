using SweetSpin.Core.Managers;

namespace SweetSpin.Core
{
    /// <summary>
    /// Manages betting operations including bet changes, validation, and credit management
    /// </summary>
    public class BettingManager : IBettingManager
    {
        private SlotMachineModel gameModel;
        private SlotMachineView slotMachineView;
        private IEventBus eventBus;
        private IAudioService audioService;
        private ISaveService saveService;

        public void Initialize(
            SlotMachineModel model,
            SlotMachineView view,
            IEventBus events,
            IAudioService audio,
            ISaveService save)
        {
            gameModel = model;
            slotMachineView = view;
            eventBus = events;
            audioService = audio;
            saveService = save;
        }

        /// <summary>
        /// Change the bet per line
        /// </summary>
        public void ChangeBet(int direction)
        {
            audioService.PlayButtonClick();
            gameModel.ChangeBetPerLine(direction);
            UpdateUI();
        }

        /// <summary>
        /// Validate if the current bet is valid
        /// </summary>
        public bool ValidateBet()
        {
            return gameModel.CanSpin();
        }

        /// <summary>
        /// Get the current total bet amount
        /// </summary>
        public int GetCurrentBet()
        {
            return gameModel.CurrentBet;
        }

        /// <summary>
        /// Get the current bet per line
        /// </summary>
        public int GetBetPerLine()
        {
            return gameModel.BetPerLine;
        }

        /// <summary>
        /// Handle insufficient credits situation
        /// </summary>
        public void HandleInsufficientCredits()
        {
            eventBus.Publish(new InsufficientCreditsEvent(
                gameModel.CurrentBet,
                gameModel.Credits
            ));
        }

        /// <summary>
        /// Add credits to the player's balance
        /// </summary>
        public void AddCredits(int amount)
        {
            if (gameModel == null) return;

            int previousCredits = gameModel.Credits;
            gameModel.AddCredits(amount);
            saveService?.SaveCredits(gameModel.Credits);

            // Play coin drop sound when credits are added
            if (amount > 0)
            {
                audioService.PlayCoinDrop();
            }

            eventBus.Publish(new CreditsChangedEvent(previousCredits, gameModel.Credits));

            if (amount > 0)
            {
                slotMachineView.ShowWinMessage($"+{amount} Credits!", WinTier.Small);
            }

            UpdateUI();
        }

        /// <summary>
        /// Set credits to a specific amount (used for initialization)
        /// </summary>
        public void SetCredits(int credits)
        {
            gameModel.SetCredits(credits);
            saveService?.SaveCredits(gameModel.Credits);
            UpdateUI();
        }

        /// <summary>
        /// Get current credits
        /// </summary>
        public int GetCredits()
        {
            return gameModel.Credits;
        }

        /// <summary>
        /// Update the UI with current betting information
        /// </summary>
        private void UpdateUI()
        {
            if (slotMachineView != null)
            {
                slotMachineView.UpdateUI(
                    gameModel.Credits,
                    gameModel.CurrentBet
                );
            }
        }

        /// <summary>
        /// Handle add credits request event
        /// </summary>
        public void OnAddCreditsRequest(AddCreditsRequestEvent e)
        {
            AddCredits(e.Amount);
        }
    }
}