using ManosLimpias.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManosLimpias.Input
{
    public class IntentInputRouter : MonoBehaviour
    {
        public FineTuningVariables tuning;
        public StageController stages;
        public PlayfieldFoci playfield;
        public Camera worldCamera;

        Vector2 _lastPointerWorld;
        bool _pointerDown;
        float _rubAccum;

        void Awake()
        {
            if (worldCamera == null) worldCamera = Camera.main;
        }

        void Update()
        {
            if (stages == null || !stages.AcceptingInput || tuning == null) return;
            var family = StageController.FamilyFor(stages.StageIndex);

            bool down = false;
            Vector2 screen = Vector2.zero;
            if (Pointer.current != null)
            {
                down = Pointer.current.press.isPressed;
                screen = Pointer.current.position.ReadValue();
            }
            else if (Mouse.current != null)
            {
                down = Mouse.current.leftButton.isPressed;
                screen = Mouse.current.position.ReadValue();
            }

            if (worldCamera == null) return;
            Vector3 world3 = worldCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -worldCamera.transform.position.z));
            Vector2 world = world3;

            var focus = playfield != null ? playfield.FocusForStage(stages.StageIndex) : null;
            bool nearFocus = focus == null || Vector2.Distance(world, focus.transform.position) <= tuning.tapRadiusWorld;

            if (family == InputFamily.TapOpenClose)
            {
                if (down && !_pointerDown && nearFocus)
                    stages.AddProgressFromFamily(family, 0.35f);
            }
            else if (family == InputFamily.HandsUnderWater)
            {
                if (down && nearFocus)
                    stages.AddProgressFromFamily(family, Time.deltaTime);
            }
            else if (family == InputFamily.RubOnHands)
            {
                if (down && nearFocus)
                {
                    if (_pointerDown)
                    {
                        float dist = Vector2.Distance(world, _lastPointerWorld);
                        _rubAccum += dist;
                        if (_rubAccum >= tuning.rubDistancePerPulse)
                        {
                            float pulses = _rubAccum / tuning.rubDistancePerPulse;
                            _rubAccum %= tuning.rubDistancePerPulse;
                            stages.AddProgressFromFamily(family, pulses * 0.2f);
                        }
                    }
                }
                else
                {
                    _rubAccum = 0f;
                }
            }

            _pointerDown = down;
            _lastPointerWorld = world;
        }
    }
}
