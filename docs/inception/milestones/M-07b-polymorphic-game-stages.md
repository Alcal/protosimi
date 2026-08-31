# M-07b — Polymorphic GameStage flow + Open Faucet

**Status:** awaiting_user_verification
**Design version:** v2
**Depends on:** M-06
**Supersedes:** M-07

## Objective

Move stage behavior into inheritable `GameStage` classes and make `GameFlowController` sequence an injected, ordered runtime stage list, beginning with `OpenFaucetStage`.

## Deliverables

- `GameStage` base class with enter, tick, exit, and completion lifecycle.
- `IGameFlowServices` interface implemented by the flow composition root.
- Ordered stage factory/configuration owned by `GameFlowController`.
- `OpenFaucetStage` that enables the Rive Faucet, accepts either handle activation, fills progress, and requests the next stage.
- Flow-level Intro and Outro transitions with no stage-specific branches in the controller.
- Rive progress/StepIcon/Faucet adapters exposed through the service interface.

## Done-when

1. AC01: Dismissing Intro enters the first instantiated stage and injects the same service interface into it.
2. AC02: `GameFlowController` enters configured stages strictly in list order and enters Outro after the final stage.
3. AC03: `OpenFaucetStage.OnEnter` enables the Faucet and subscribes to its activation event; either left or right activation sets progress to `1` and requests exactly one transition.
4. AC04: `OpenFaucetStage.OnExit` unsubscribes and disables/resets the Faucet before the next stage is entered.
5. AC05: An inactive stage cannot react to Faucet events or request another transition.
6. AC06: `GameFlowController` contains no `OpenFaucetStage`, Faucet, or input-family branching; stage-specific behavior is isolated in `GameStage` subclasses.
7. AC07: Replay resets stage instances and returns through Intro without retaining progress or subscriptions.

## Touch list

```
docs/inception/STATUS.md
docs/inception/milestones/ROADMAP.md
docs/inception/milestones/M-07b-polymorphic-game-stages.md
docs/inception/tests/M-07b-acceptance.md
Assets/Scripts/Core/GameStage.cs
Assets/Scripts/Core/IGameFlowServices.cs
Assets/Scripts/Core/GameFlowController.cs
Assets/Scripts/Core/StageController.cs
Assets/Scripts/Core/OpenFaucetStage.cs
Assets/Scripts/ManosLimpias.Runtime.asmdef
Assets/Scripts/Analytics/AnalyticsStub.cs
Assets/Scripts/UI/Rive/Faucet.cs
Assets/Scripts/UI/Rive/RiveHudBinder.cs
Assets/Scripts/UI/Rive/SimiPrototypePresenter.cs
Assets/Scenes/Gameplay.unity
Assets/Tests/EditMode/M07b.EditMode.Tests.asmdef
Assets/Tests/EditMode/M07bGameStageTests.cs
Assets/Tests/PlayMode/M07b.PlayMode.Tests.asmdef
Assets/Tests/PlayMode/M07bGameStagePlayModeTests.cs
```

## Technical constraints

- Concrete stages depend only on `IGameFlowServices`, not on `GameFlowController`, `GameObject.Find`, or Rive implementation classes.
- The runtime list is the only source of stage order.
- Intro and Outro are flow states; they are not `GameStage` subclasses.
- Rive remains the source of Faucet interaction and progress visuals.
- No persistence or new gameplay stages are added in this milestone.

## Risks / unknowns

- Unity serialization strategy for concrete stage configuration must support reordering without losing subtype data.
- Existing M-07 runtime code must be reduced or adapted without preserving duplicate sequencing paths.

## Links

- Acceptance: [`../tests/M-07b-acceptance.md`](../tests/M-07b-acceptance.md)
- GDD: [`../design/GDD-v2.md`](../design/GDD-v2.md)
- TAD: [`../tech/TAD-v2.md`](../tech/TAD-v2.md)
