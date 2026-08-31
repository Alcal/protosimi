# M-07b Acceptance — Polymorphic GameStage flow

## Demo script

1. Open `Assets/Scenes/Gameplay.unity`.
2. Press Play and confirm the Rive intro is shown.
3. Press `Jugar`; expect the first configured `GameStage` to enter and Open Water to enable the Rive Faucet.
4. Activate either the left or right Faucet handle; expect progress to reach 100%, the stage to exit, and the next configured stage to enter.
5. Complete the final configured stage; expect the flow to enter Outro.
6. Replay; expect Intro to start with fresh stage progress and no duplicate Faucet callbacks.

## Acceptance checklist

| ID | Criterion | Automated test | Manual | Pass |
|----|-----------|----------------|--------|------|
| AC01 | Intro enters the first stage and injects `IGameFlowServices`. | `AC01_FirstStageReceivesServices_OnIntroDismissed` |  | [ ] |
| AC02 | Configured stages enter strictly in list order, then Outro. | `AC02_FlowEntersStages_InConfiguredOrder` | [ ] | [ ] |
| AC03 | `OpenFaucetStage` enables/subscribes on entry; either handle fills progress and requests one transition. | `AC03_OpenFaucetCompletes_OnEitherHandle` | [ ] | [ ] |
| AC04 | Open Faucet exits by unsubscribing and disabling/resetting before the next stage. | `AC04_OpenFaucetCleansUp_OnExit` |  | [ ] |
| AC05 | An inactive stage cannot react or request another transition. | `AC05_InactiveStageIgnoresFaucetEvents` |  | [ ] |
| AC06 | Flow code has no stage-specific branching. | Manual code review | [ ] | [ ] |
| AC07 | Replay resets stage instances, progress, and subscriptions. | `AC07_ReplayResetsStageLifecycle` | [ ] | [ ] |

## Automated tests

- EditMode: `Assets/Tests/EditMode/M07bGameStageTests.cs`
- PlayMode: `Assets/Tests/PlayMode/M07bGameStagePlayModeTests.cs`

## Sign-off

- [x] Automated tests pass: 6/6 EditMode and 1/1 PlayMode.
- [ ] Manual checklist verified.
- [ ] `docs/inception/STATUS.md` advanced after user verification.
