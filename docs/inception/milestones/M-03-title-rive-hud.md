# M-03 — Title + Rive HUD

**Status:** done
**Design version:** v1
**Depends on:** M-02

## Objective

Wire Title → Intro → Play → Win navigation and bind the Rive HUD ViewModel per RIVE_INTERFACES.

## Deliverables

- [ ] Title CTA loads/starts Gameplay
- [ ] `RiveHudBinder` updates `stageProgress`, `stageIndex`, icon states, host/hud flags
- [ ] `.riv` at `Assets/Art/Rive/ManosLimpias_HUD.riv` (or linked export)
- [ ] `hudVisible` false on title; true after intro

## Done-when (acceptance criteria)

1. AC01: Title Start enters Gameplay intro then stage 0
2. AC02: Top bar fill tracks `stageProgress`; resets on stage change
3. AC03: Icon states follow shared-icon rules (faucet / wet-rinse reactivation)
4. AC04: Win state reachable from stage 5 complete with HUD icons complete

## Touch list

```
Assets/Scripts/UI/RiveHudBinder.cs
Assets/Scripts/UI/TitleController.cs
Assets/Art/Rive/ManosLimpias_HUD.riv
Assets/Scenes/Title.unity
Assets/Scenes/Gameplay.unity
```

## Technical constraints

- Unity binds data only; Rive owns layout
- Fallback debug HUD if Rive runtime package binding fails

## Risks / unknowns

- Rive ViewModel API version differences

## Links

- Acceptance: [`../tests/M-03-acceptance.md`](../tests/M-03-acceptance.md)
- GDD: UI/UX, SCREEN_MAP
- TAD: Asset pipeline, Rive
- RIVE_INTERFACES.md
