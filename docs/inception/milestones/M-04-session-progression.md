# M-04 — Session progression

**Status:** done
**Design version:** v1
**Depends on:** M-03

## Objective

Formalize `GameFlowController` session states and fire analytics events on transitions (no disk save).

## Deliverables

- [ ] `GameFlowController` states: Title, Intro, Stage, Assist, Win
- [ ] Analytics: `session_start`, `play_pressed`, `stage_start`, `stage_complete`, `session_complete`
- [ ] Stage duration tracking for payloads

## Done-when (acceptance criteria)

1. AC01: Flow transitions match game-states diagram (no pause/lose)
2. AC02: Each listed analytics event fires at least once in a full run (console)
3. AC03: Replay from Win returns to Title without residual stage progress

## Touch list

```
Assets/Scripts/Core/GameFlowController.cs
Assets/Scripts/Analytics/AnalyticsStub.cs
Assets/Scenes/Gameplay.unity
Assets/Scenes/Title.unity
```

## Technical constraints

- Session-only; no PlayerPrefs
- Assist state reserved; wired fully in M-06

## Risks / unknowns

- None significant

## Links

- Acceptance: [`../tests/M-04-acceptance.md`](../tests/M-04-acceptance.md)
- GDD: Progression (N/A save), game-states
- TAD: Analytics catalog
