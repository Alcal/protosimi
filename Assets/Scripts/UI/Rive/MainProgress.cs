namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Root gameplay artboard <c>main</c> — SM inputs only; nested components keep their own SMs.
    /// </summary>
    public static class MainProgress
    {
        public const string Artboard = SimiPrototypeArtboards.Main;
        public const string StateMachine = "progress_StateMachine";

        public const string ProgressNum = "progress_num";

        public const string AnimProgressMax = "progress_max";
        public const string AnimProgressMin = "progress_min";
    }
}
