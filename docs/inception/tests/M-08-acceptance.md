# M-08 — Acceptance & Demo Script

**Design version:** v3
**Milestone:** [`../milestones/M-08-rive-background-anchors.md`](../milestones/M-08-rive-background-anchors.md)

## Prerequisites

- Unity Editor, Vulkan on Linux (Rive 0.4.3)
- Scene: `Assets/Scenes/Gameplay.unity`

## Demo script

1. Open `Assets/Scenes/Gameplay.unity`.
2. Press Play; expect the Rive intro overlay and a `background` playfield (not artboard `main`).
3. Confirm sibling widgets at faucet/hands/soap/towel/character/step/progressBar anchors.
4. Press `Jugar`; expect intro to dismiss and `OpenFaucetStage` to enable only the Faucet.
5. Confirm hands/soap/towel/character do not consume pointer hits.
6. Activate either Faucet handle; expect overlay progress to fill and the stage to complete.

## Acceptance checklist

| ID | Criterion | Automated test | Manual | Pass |
|----|-----------|----------------|--------|------|
| AC01 | Gameplay mounts `background`, not `main`. | `AC01_BackgroundArtboard_IsMounted` | [ ] | [ ] |
| AC02 | Sibling widgets exist for each `*-anchor`. | `AC02_AnchorSlots_HaveWidgets` | [ ] | [ ] |
| AC03 | Progress writes to overlay `progressBar`. | `AC03_ContainCenter_MapsAabbToView` | [ ] | [ ] |
| AC04 | Disabled Faucet ignores activation and is not hittable. | `AC04_DisabledFaucet_IgnoresActivation` | [ ] | [ ] |
| AC05 | Non-faucet components stay `HitTestBehavior.None`. | `AC05_NonFaucetSlots_StartWithoutHits` | [ ] | [ ] |
| AC06 | Intro Jugar still enters the first GameStage. | `AC06_IntroDismiss_StillEntersStage` | [ ] | [ ] |

## Automated tests

- EditMode: `Assets/Tests/EditMode/M08RiveAnchorTests.cs`
- PlayMode: `Assets/Tests/PlayMode/M08RiveAnchorPlayModeTests.cs`

## Sign-off

- [x] All automated tests pass
- [ ] Manual checklist complete
- [ ] User approves milestone → update `docs/inception/STATUS.md`
