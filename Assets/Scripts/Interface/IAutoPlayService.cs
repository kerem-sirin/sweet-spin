namespace SweetSpin
{
    public interface IAutoPlayService
    {
        bool IsActive { get; }
        int RemainingSpins { get; }
        bool StopRequested { get; }

        void Initialize(IEventBus eventBus);
        void StartAutoPlay(int spinCount);
        void RequestStop();
        void OnSpinCompleted();
        void StopDueToInsufficientCredits();
        bool ShouldContinue();
    }
}