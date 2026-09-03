using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Named layout nodes on artboard <c>background</c>. Names do not include brackets.
    /// Origins are Node x/y in Rive artboard space (top-left, Y-down). Empty groups have
    /// no drawable AABB; Unity sizes each sibling from the component artboard and shifts
    /// that box by the artboard's origin (0,0 = top-left, 0.5,0.5 = center).
    /// </summary>
    public static class BackgroundAnchors
    {
        public const string Faucet = "faucet-anchor";
        public const string Soap = "soap-anchor";
        public const string Towel = "towel-anchor";
        public const string Hands = "hands-anchor";
        public const string Character = "character-anchor";
        public const string Step1 = "step1-anchor";
        public const string Step2 = "step2-anchor";
        public const string Step3 = "step3-anchor";
        public const string Step4 = "step4-anchor";
        public const string ProgressBar = "progressBar-anchor";

        /// <summary>Unnamed parent of step1–4 on <c>background</c>.</summary>
        public static readonly Vector2 StepGroupOrigin = new(1758.5f, 540f);

        public static readonly string[] All =
        {
            Faucet,
            Soap,
            Towel,
            Hands,
            Character,
            Step1,
            Step2,
            Step3,
            Step4,
            ProgressBar,
        };

        public static bool TryGetWorldOrigin(string anchorName, out Vector2 origin)
        {
            switch (anchorName)
            {
                case ProgressBar:
                    origin = new Vector2(892.141235f, 104.580109f);
                    return true;
                case Faucet:
                    origin = new Vector2(557f, 105f);
                    return true;
                case Towel:
                    origin = new Vector2(1427f, 375f);
                    return true;
                case Soap:
                    origin = new Vector2(309.5f, 367f);
                    return true;
                case Hands:
                    origin = new Vector2(0f, 87.5f);
                    return true;
                case Character:
                    origin = new Vector2(49.5f, 696.5f);
                    return true;
                case Step1:
                    origin = StepGroupOrigin + new Vector2(0f, -360f);
                    return true;
                case Step2:
                    origin = StepGroupOrigin + new Vector2(0f, -120f);
                    return true;
                case Step3:
                    origin = StepGroupOrigin + new Vector2(0f, 120f);
                    return true;
                case Step4:
                    origin = StepGroupOrigin + new Vector2(0f, 360f);
                    return true;
                default:
                    origin = Vector2.zero;
                    return false;
            }
        }

        public static Vector2 SizeFor(string anchorName)
        {
            switch (anchorName)
            {
                case Faucet: return SimiPrototypeArtboards.FaucetSize;
                case Soap: return SimiPrototypeArtboards.SoapSize;
                case Towel: return SimiPrototypeArtboards.TowelSize;
                case Hands: return SimiPrototypeArtboards.HandsSize;
                case Character: return SimiPrototypeArtboards.CharacterSize;
                case Step1:
                case Step2:
                case Step3:
                case Step4: return SimiPrototypeArtboards.StepIconSize;
                case ProgressBar: return SimiPrototypeArtboards.ProgressBarSize;
                default: return Vector2.zero;
            }
        }

        public static bool TryGetArtboardAabb(string anchorName, Vector2 size, out Rect aabb)
        {
            if (!TryGetWorldOrigin(anchorName, out var origin) || size.x <= 0f || size.y <= 0f)
            {
                aabb = default;
                return false;
            }

            aabb = ArtboardSpace.RectFromOriginSize(
                origin,
                size,
                SimiPrototypeArtboards.OriginForAnchor(anchorName));
            return true;
        }
    }
}
