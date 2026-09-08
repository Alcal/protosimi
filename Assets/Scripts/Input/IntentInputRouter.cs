using ManosLimpias.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManosLimpias.Input
{
    public class IntentInputRouter : MonoBehaviour
    {
        public FineTuningVariables tuning;
        public StageController stages;
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
            var definition = stages.CurrentDefinition;
            var family = definition != null ? definition.inputFamily : StageController.FamilyFor(stages.StageIndex);

            // TapOpenClose is owned by the interactive Rive component. Do not let
            // world-space proximity or the legacy Faucet collider complete it.
            if (family == InputFamily.TapOpenClose) return;

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

            if (family == InputFamily.HandsUnderWater)
            {
                if (down)
                    stages.AddProgressFromFamily(family, Time.deltaTime);
            }
            else if (family == InputFamily.RubOnHands)
            {
                if (down)
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
