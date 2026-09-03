namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Shared step-icon component. Values 1–4. Animation names <c>step_ 2</c> and
    /// <c>step_ 3</c> include a space after the underscore.
    /// </summary>
    public static class StepIcon
    {
        public const string Artboard = SimiPrototypeArtboards.StepIcon;
        public const string StateMachine = "steps_StateMachine";

        public const string IsCompleted = "isCompleted";
        public const string IsActive = "isActive";
        public const string StepId = "icon_ID";
        public const string StepIdLegacy = "step_ID";

        public const string EventIsCompleted = "isCompleted";

        public static readonly string[] AnchorNames =
        {
            BackgroundAnchors.Step1,
            BackgroundAnchors.Step2,
            BackgroundAnchors.Step3,
            BackgroundAnchors.Step4,
        };

        public static readonly string[] WidgetNames =
        {
            "StepIcon1Rive",
            "StepIcon2Rive",
            "StepIcon3Rive",
            "StepIcon4Rive",
        };

        public const string AnimStep1 = "step_1";
        public const string AnimStep2 = "step_ 2";
        public const string AnimStep3 = "step_ 3";
        public const string AnimStep4 = "step_4";
        public const string AnimStandby = "standby";
        public const string AnimActive = "active";
        public const string AnimComplete = "complete";
    }
}
