using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Artboard names and design sizes from Assets/Art/Rive/simi_prototype.riv.
    /// </summary>
    public static class SimiPrototypeArtboards
    {
        public const string FilePath = "Assets/Art/Rive/simi_prototype.riv";

        public const string Main = "main";
        public const string Intro = "intro";
        public const string Button = "Button";
        public const string Faucet = "faucet";
        public const string StepIcon = "stepIcon";
        public const string Character = "character";
        public const string Hands = "hands";
        /// <summary>Trailing space is part of the artboard name in the .riv.</summary>
        public const string Soap = "soap ";
        public const string Towel = "towel";
        public const string Bubble = "Bubble";
        public const string GameBubbles = "game_bubbles";
        public const string GameBubble = "game_bubble";
        public const string Ref = "REF";

        public static readonly Vector2 MainSize = new(1920f, 1080f);
        public static readonly Vector2 IntroSize = new(1920f, 1080f);
        public static readonly Vector2 ButtonSize = new(500f, 400f);
        public static readonly Vector2 FaucetSize = new(748f, 468f);
        public static readonly Vector2 StepIconSize = new(250f, 250f);
        public static readonly Vector2 CharacterSize = new(300f, 300f);
        public static readonly Vector2 HandsSize = new(1920f, 1080f);
        public static readonly Vector2 SoapSize = new(400f, 400f);
        public static readonly Vector2 TowelSize = new(300f, 340f);
        public static readonly Vector2 BubbleSize = new(300f, 300f);
        public static readonly Vector2 GameBubblesSize = new(171f, 135f);
        public static readonly Vector2 GameBubbleSize = new(80f, 63f);
        public static readonly Vector2 RefSize = new(1920f, 1080f);
    }
}
