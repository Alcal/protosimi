namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Overlay artboard <c>intro</c>. Nested Button label is "Jugar". No SM inputs.
    /// Dismiss via <see cref="ButtonArtboard.EventButtonPress"/> or <see cref="BubbleButtonViewModel"/>.
    /// </summary>
    public static class Intro
    {
        public const string Artboard = SimiPrototypeArtboards.Intro;
        public const string StateMachine = "State Machine 1";
        public const string WidgetName = "IntroRive";

        public const string AnimDefault = "default";
        public const string AnimFadeout = "fadeout";

        public const string TitleText = "Manos Limpias";
    }
}
