using System;

namespace ManosLimpias.Core
{
    public enum FaucetSide
    {
        Left,
        Right
    }

    public interface IFaucetControl
    {
        event Action<FaucetSide> Activated;
        event Action PointerHit;
        bool IsEnabled { get; }
        bool LeftIsOpen { get; }
        bool RightIsOpen { get; }
        bool IsOpen { get; }
        void SetEnabled(bool enabled);
    }

    public interface IProgressBarControl
    {
        float Progress { get; }
        void SetProgress(float progress);
    }

    public interface IStepIconControl
    {
        void SetState(int stepId, bool active, bool completed);
    }

    public interface IHandsControl
    {
        bool IsDraggable { get; }
        void SetDraggable(bool draggable);
    }

    public interface IWaterContactControl
    {
        bool IsOverlapping { get; }
    }

    public interface IGameFlowServices
    {
        IFaucetControl Faucet { get; }
        IHandsControl Hands { get; }
        IWaterContactControl WaterContact { get; }
        IProgressBarControl ProgressBar { get; }
        IStepIconControl StepIcon { get; }
        void RequestStageCompletion(GameStage stage);
    }
}
