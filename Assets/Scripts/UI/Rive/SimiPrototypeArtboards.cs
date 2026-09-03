using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Artboard names and design sizes from Assets/Art/Rive/simi_prototype.riv.
    /// </summary>
    public static class SimiPrototypeArtboards
    {
        public const string FilePath = "Assets/Art/Rive/simi_prototype.riv";

        public const string Background = "background";
        public const string ProgressBar = "progressBar";
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
        /// <summary>Leftover composed artboard. Gameplay no longer mounts it.</summary>
        public const string Main = "main";

        public static readonly Vector2 BackgroundSize = new(1920f, 1080f);
        public static readonly Vector2 ProgressBarSize = new(1080f, 200f);
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
        public static readonly Vector2 MainSize = new(1920f, 1080f);

        /// <summary>Artboard origin (0–1). Default 0,0 is top-left; 0.5,0.5 is center.</summary>
        public static readonly Vector2 TopLeftOrigin = Vector2.zero;
        public static readonly Vector2 CenterOrigin = new(0.5f, 0.5f);

        public static readonly Vector2 BackgroundOrigin = TopLeftOrigin;
        public static readonly Vector2 ProgressBarOrigin = CenterOrigin;
        public static readonly Vector2 FaucetOrigin = TopLeftOrigin;
        public static readonly Vector2 StepIconOrigin = CenterOrigin;
        public static readonly Vector2 CharacterOrigin = TopLeftOrigin;
        public static readonly Vector2 HandsOrigin = TopLeftOrigin;
        public static readonly Vector2 SoapOrigin = CenterOrigin;
        public static readonly Vector2 TowelOrigin = CenterOrigin;

        public static string ArtboardNameForAnchor(string anchorName)
        {
            switch (anchorName)
            {
                case BackgroundAnchors.Faucet: return Faucet;
                case BackgroundAnchors.Soap: return Soap;
                case BackgroundAnchors.Towel: return Towel;
                case BackgroundAnchors.Hands: return Hands;
                case BackgroundAnchors.Character: return Character;
                case BackgroundAnchors.Step1:
                case BackgroundAnchors.Step2:
                case BackgroundAnchors.Step3:
                case BackgroundAnchors.Step4: return StepIcon;
                case BackgroundAnchors.ProgressBar: return ProgressBar;
                default: return null;
            }
        }

        public static Vector2 OriginForAnchor(string anchorName)
        {
            switch (anchorName)
            {
                case BackgroundAnchors.Faucet: return FaucetOrigin;
                case BackgroundAnchors.Soap: return SoapOrigin;
                case BackgroundAnchors.Towel: return TowelOrigin;
                case BackgroundAnchors.Hands: return HandsOrigin;
                case BackgroundAnchors.Character: return CharacterOrigin;
                case BackgroundAnchors.Step1:
                case BackgroundAnchors.Step2:
                case BackgroundAnchors.Step3:
                case BackgroundAnchors.Step4: return StepIconOrigin;
                case BackgroundAnchors.ProgressBar: return ProgressBarOrigin;
                default: return TopLeftOrigin;
            }
        }
    }
}
