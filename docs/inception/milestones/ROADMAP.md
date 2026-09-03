# Implementation Roadmap

Design version: **v3**
Status: **approved**

**Sources:** [`../design/GDD-v2.md`](../design/GDD-v2.md) · [`../tech/TAD-v3.md`](../tech/TAD-v3.md)

## Tech constraints from TAD

- Front-load: Unity scaffold + `com.unity.ai.assistant`, `FineTuningVariables`, `AnalyticsStub`, 1920×1080
- Analytics events in early milestones: stub in M-01; fire stage/session events by M-04
- Asset pipeline: Rive HUD; Unity-native graybox playfield; Blender/ComfyUI unused for MVP
- No disk persistence; M-04 = session state machine + analytics
- `GameFlowController` sequences instantiated `GameStage` instances; concrete stages consume `IGameFlowServices`
- Stage behavior must not resolve scene objects or hard-code next-stage indices
- M-07b implements only `OpenFaucetStage`; future stages are additional concrete stage types
- Gameplay Rive mount is `background` + sibling widgets at `*-anchor` nodes; leftover `main` is not mounted

## Milestone index

| ID | Title | Status | Depends on |
|----|-------|--------|------------|
| M-01 | Project bootstrap | done | — |
| M-02 | Core wash loop | done | M-01 |
| M-03 | Title + Rive HUD | done | M-02 |
| M-04 | Session progression | done | M-03 |
| M-05 | Graybox content | done | M-04 |
| M-06 | Assist + polish | done | M-05 |
| M-07 | Rive Faucet state machine | superseded | M-06 |
| M-07b | Polymorphic GameStage flow + Open Faucet | awaiting_user_verification | M-06 |
| M-08 | Rive background-anchor harness | awaiting_user_verification | M-07b |

## Vertical slice definition

The MVP is **done** when the player can:

1. Start from Title, complete intro, and wash through all six stages at 1920×1080
2. See Rive HUD progress / icons / host bindings update with stage state
3. Receive WAF assist (including hijack) if idle, then reach Win and replay

## Out of roadmap (post-MVP)

- Final illustration, real VO/SFX
- ComfyUI / Blender art pipelines
- Cloud saves, collection hub, localization packs
- Mobile-native primary target

## Notes

- Fast-track: verification gates skipped; milestones marked `done` by implementer after delivery.
- M-02 must deliver a playable core loop (debug HUD OK).
