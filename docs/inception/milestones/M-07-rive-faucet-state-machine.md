# M-07 — Rive Faucet state machine

**Status:** superseded
**Design version:** v1
**Depends on:** M-06

## Objective

Replace the Open Water Unity proximity interaction with the nested Faucet Rive component. This milestone was superseded by the v2 polymorphic `GameStage` architecture before user verification.

## Deliverables

- Configurable stage definitions with `OnEnter` and `OnExit` hooks.
- `GameStateMachine` lifecycle covering Intro, configured stages, and Outro.
- Open Water configured as the only active stage for this milestone.
- Either nested Rive Faucet handle completes Open Water.
- Rive progress bar and one dynamic `stepIcon` reflect stage state.
- Unity Faucet GameObject is no longer required for Open Water input.

## Done-when

1. AC01: Gameplay starts in Intro and enters configured Open Water after the Rive Jugar action.
2. AC02: The left and right Rive Faucet activations produce the same completion result.
3. AC03: Completing Open Water sets Rive `progress_num` to `1`, marks the single step icon complete, and runs `OnExit` once.
4. AC04: The configured stage machine does not enter legacy stage 1 after Open Water; it enters Outro/completed state.
5. AC05: Open Water completion does not depend on the Unity Faucet GameObject, collider, world-space proximity, or `TapOpenClose` polling.
6. AC06: Missing Rive nested paths or inputs fail safely with a single diagnostic and do not prevent the intro/game flow from loading.
7. AC07: Automated stage lifecycle tests pass; remaining Rive pointer/render behavior is covered by the manual checklist.

## Touch list

```
docs/inception/STATUS.md
docs/inception/milestones/ROADMAP.md
docs/inception/milestones/M-07-rive-faucet-state-machine.md
docs/inception/tests/M-07-acceptance.md
Assets/Scripts/Core/StageController.cs
Assets/Scripts/Core/GameStateMachine.cs
Assets/Scripts/Core/GameFlowController.cs
Assets/Scripts/Input/IntentInputRouter.cs
Assets/Scripts/UI/Rive/Faucet.cs
Assets/Scripts/UI/Rive/RiveStateMachineInputs.cs
Assets/Scripts/UI/Rive/RiveHudBinder.cs
Assets/Scripts/UI/Rive/SimiPrototypePresenter.cs
Assets/Scripts/Editor/RivePrototypeMount.cs
Assets/Scenes/Gameplay.unity
Assets/Tests/EditMode/M07StageStateMachineTests.cs
```

## Technical constraints

- `main` remains the single Rive visual source.
- Only one runtime `StepIcon` controller is created for the configured stage.
- The existing six-stage input families remain available for future configuration, but are not advanced by M-07.
- Session state is not persisted.
- Do not modify the Rive binary asset in this milestone.
