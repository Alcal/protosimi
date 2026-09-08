using System;
using Rive;
using Rive.Components;
using UnityEngine;

namespace ManosLimpias.UI.Rive
{
    /// <summary>
    /// Drives the dedicated character artboard. Before each stage, GameFlow
    /// plays popUp, holds <see cref="IsTalking"/> for
    /// <see cref="talkDurationSeconds"/>, then popOut. The stage starts after
    /// the pop-out wait.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class Character : MonoBehaviour
    {
        public const string Artboard = SimiPrototypeArtboards.Character;
        public const string StateMachine = "drSimi_StateMachine";
        public const string WidgetName = "CharacterRive";

        public const string IsTalking = "isTalking";
        public const string PopUp = "popUp";
        public const string PopOut = "popOut";
        public const string Complete = "complete";
        public const string WafId = "wafID";

        public const string StatusText = "Dr. Simi Status";

        public const string AnimXAxis = "X axis";
        public const string AnimYAxis = "Y axis";
        public const string AnimIsNotTalking = "isNotTalking";
        public const string AnimIsTalking = "isTalking";
        public const string AnimCelebrate = "celebrate";
        public const string AnimWaf3 = "waf3";
        public const string AnimWaf2 = "waf2";
        public const string AnimBlink = "blink";
        public const string AnimIntro = "intro";
        public const string AnimPopout = "popout";
        public const string AnimPopup = "popup";
        public const string AnimHidden = "hidden";

        public const float DefaultTalkDurationSeconds = 3f;
        public const float DefaultPopOutDurationSeconds = 0.5f;

        public RiveWidget widget;
        public float talkDurationSeconds = DefaultTalkDurationSeconds;
        public float popOutDurationSeconds = DefaultPopOutDurationSeconds;

        public event Action BriefingCompleted;

        public bool IsBriefing => _phase != Phase.Idle;
        public bool IsTalkingDesired { get; private set; }
        public int PopUpCount { get; private set; }
        public int PopOutCount { get; private set; }

        enum Phase
        {
            Idle,
            Talking,
            PoppingOut
        }

        Phase _phase;
        float _elapsed;
        bool _popUpPending;
        bool _popOutPending;
        bool _inputsLogged;
        SMIBool _isTalkingInput;
        SMITrigger _popUpTrigger;
        SMITrigger _popOutTrigger;

        public void Bind(RiveWidget riveWidget)
        {
            widget = riveWidget;
            _isTalkingInput = null;
            _popUpTrigger = null;
            _popOutTrigger = null;
            _inputsLogged = false;
            ApplyInputs();
        }

        public void BeginBriefing()
        {
            _phase = Phase.Talking;
            _elapsed = 0f;
            IsTalkingDesired = true;
            _popUpPending = true;
            _popOutPending = false;
            ApplyInputs();
        }

        public void Cancel()
        {
            bool wasBriefing = IsBriefing;
            _phase = Phase.Idle;
            _elapsed = 0f;
            IsTalkingDesired = false;
            _popUpPending = false;
            if (wasBriefing || PopUpCount > PopOutCount)
                _popOutPending = true;
            ApplyInputs();
        }

        public void Tick(float deltaTime)
        {
            if (_phase == Phase.Idle)
                return;

            ApplyInputs();
            if (_phase == Phase.Talking && _popUpPending)
                return;
            if (_phase == Phase.PoppingOut && _popOutPending)
                return;

            _elapsed += Mathf.Max(0f, deltaTime);
            if (_phase == Phase.Talking && _elapsed >= TalkDuration)
            {
                StartPopOut();
                return;
            }

            if (_phase == Phase.PoppingOut && _elapsed >= PopOutDuration)
                FinishBriefing();
        }

        void Update()
        {
            Tick(Time.deltaTime);
        }

        void LateUpdate()
        {
            if (IsBriefing || _popUpPending || _popOutPending || IsTalkingDesired)
                ApplyInputs();
        }

        void StartPopOut()
        {
            _phase = Phase.PoppingOut;
            _elapsed = 0f;
            IsTalkingDesired = false;
            _popOutPending = true;
            ApplyInputs();
        }

        void FinishBriefing()
        {
            _phase = Phase.Idle;
            _elapsed = 0f;
            IsTalkingDesired = false;
            ApplyInputs();
            BriefingCompleted?.Invoke();
        }

        float TalkDuration => Mathf.Max(0f, talkDurationSeconds);
        float PopOutDuration => Mathf.Max(0f, popOutDurationSeconds);

        void ApplyInputs()
        {
            ResolveInputs();
            if (_isTalkingInput != null)
                _isTalkingInput.Value = IsTalkingDesired;

            if (_popUpPending && TryFire(_popUpTrigger, out var popUpResolved))
            {
                PopUpCount++;
                _popUpPending = !popUpResolved;
            }

            if (_popOutPending && TryFire(_popOutTrigger, out var popOutResolved))
            {
                PopOutCount++;
                _popOutPending = !popOutResolved;
            }
        }

        bool TryFire(SMITrigger trigger, out bool resolved)
        {
            if (trigger != null)
            {
                trigger.Fire();
                resolved = true;
                return true;
            }

            if (widget == null)
            {
                resolved = true;
                return true;
            }

            if (widget.StateMachine == null)
            {
                resolved = widget.Status != WidgetStatus.Uninitialized &&
                           widget.Status != WidgetStatus.Loading;
                return resolved;
            }

            resolved = true;
            return true;
        }

        void ResolveInputs()
        {
            var sm = widget?.StateMachine;
            if (sm == null)
                return;

            if (_isTalkingInput == null)
                _isTalkingInput = RiveStateMachineInputs.FindBool(sm, IsTalking);
            if (_popUpTrigger == null)
                _popUpTrigger = RiveStateMachineInputs.FindTrigger(sm, PopUp);
            if (_popOutTrigger == null)
                _popOutTrigger = RiveStateMachineInputs.FindTrigger(sm, PopOut);

            if (_inputsLogged)
                return;
            _inputsLogged = true;
            Debug.Log($"[Character] SM inputs: {RiveStateMachineInputs.DescribeInputs(sm)}");
        }
    }
}
