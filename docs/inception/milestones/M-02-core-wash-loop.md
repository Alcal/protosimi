# M-02 — Core wash loop

**Status:** done
**Design version:** v1
**Depends on:** M-01

## Objective

Deliver a playable six-stage wash loop with three intent-tolerant input families and CAF stage advance (debug HUD OK).

## Deliverables

- [ ] Scripts: `StageController`, `TapOpenCloseInput`, `HandsUnderWaterInput`, `RubOnHandsInput`
- [ ] Prefabs: stage focus placeholders (colliders)
- [ ] Data: rates driven from `FineTuningVariables`
- [ ] Debug on-screen stage/progress text until Rive lands

## Done-when (acceptance criteria)

1. AC01: Stages advance 0→5 when progress reaches 1.0
2. AC02: Each stage accepts only its mapped input family
3. AC03: Progress increases from intentful pointer input using FineTuning rates
4. AC04: Completing stage 5 reaches a win-ready signal (or Win stub)

## Touch list

```
Assets/Scripts/Core/StageController.cs
Assets/Scripts/Input/TapOpenCloseInput.cs
Assets/Scripts/Input/HandsUnderWaterInput.cs
Assets/Scripts/Input/RubOnHandsInput.cs
Assets/Scripts/UI/DebugHud.cs
Assets/Scenes/Gameplay.unity
```

## Technical constraints

- One active stage at a time; no fail state
- Shared progress pipeline for future WAF3 hijack

## Risks / unknowns

- Pointer intent thresholds need playtest defaults in FineTuning

## Links

- Acceptance: [`../tests/M-02-acceptance.md`](../tests/M-02-acceptance.md)
- GDD: Mechanics (MVP), Core loop
- TAD: Core systems
