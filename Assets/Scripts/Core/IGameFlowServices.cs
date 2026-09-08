using System;

namespace ManosLimpias.Core
{
    public enum FaucetSide
    {
        Left,
        Right
    }

    public interface IGlowHint
    {
        bool IsGlowing { get; }
        void SetGlow(bool on);
    }

    public interface IFaucetControl : IGlowHint
    {
        event Action<FaucetSide> Activated;
        event Action<FaucetSide> PointerHit;
        bool IsEnabled { get; }
        bool LeftIsOpen { get; }
        bool RightIsOpen { get; }
        bool IsOpen { get; }
        void SetEnabled(bool enabled);
        /// <summary>
        /// Enables C# tap detection. Pass <paramref name="rivePointerHits"/> false
        /// so the artboard listeners do not fire from hover or a held pointer.
        /// </summary>
        void SetEnabled(bool enabled, bool rivePointerHits);
        /// <summary>
        /// Opens the chosen side visually and keeps it on while disabling further hits.
        /// </summary>
        void LockOpen(FaucetSide side);
        /// <summary>
        /// Closes whichever side is open and disables further hits.
        /// </summary>
        void LockClosed();
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

    public interface IHandsControl : IGlowHint
    {
        event Action DragStarted;
        bool IsDraggable { get; }
        void SetDraggable(bool draggable);
        void ReturnHome();
    }

    public interface IWaterContactControl
    {
        bool IsOverlapping { get; }
    }

    public interface ISoapControl : IGlowHint
    {
        event Action DragStarted;
        bool IsDraggable { get; }
        bool IsOverlapping { get; }
        void SetDraggable(bool draggable);
        void ReturnHome();
    }

    public interface ISoapFoamControl
    {
        void SetCoverage(float progress01);
        void SetScrubbing(bool scrubbing);
        void ResetFoam();
    }

    public interface IWetnessControl
    {
        float Wetness { get; }
        void SetWetness(float progress01);
        void ResetWetness();
    }

    public interface IGameFlowServices
    {
        IFaucetControl Faucet { get; }
        IHandsControl Hands { get; }
        IWaterContactControl WaterContact { get; }
        ISoapControl Soap { get; }
        ISoapFoamControl SoapFoam { get; }
        IWetnessControl Wetness { get; }
        IProgressBarControl ProgressBar { get; }
        IStepIconControl StepIcon { get; }
        void RequestStageCompletion(GameStage stage);
    }
}
