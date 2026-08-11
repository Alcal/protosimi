# M-06 — Assist + polish

**Status:** done
**Design version:** v1
**Depends on:** M-05

## Objective

Ship WAF1–3 with host hijack, camera ease-to-focus, silent audio placeholders, CAF juice, and win/replay polish.

## Deliverables

- [ ] WAF idle timers from FineTuning; WAF1 pulse, WAF2 host hint, WAF3 hijack
- [ ] `AssistHijack` advances same progress pipeline
- [ ] `CameraFocus` ease per stage
- [ ] Silent audio keys + `AudioPlaceholderPlayer`
- [ ] Win celebration + replay CTA

## Done-when (acceptance criteria)

1. AC01: Idle past WAF thresholds triggers escalating help without fail state
2. AC02: WAF3 hijack can complete the current stage bar
3. AC03: Camera eases toward active focus on stage change
4. AC04: Silent clips keyed by RIVE_INTERFACES IDs exist and fire (no audible requirement)
5. AC05: Full Title→Win→Replay loop completable in one Play Mode session

## Touch list

```
Assets/Scripts/Core/AssistHijack.cs
Assets/Scripts/Core/CameraFocus.cs
Assets/Scripts/Core/WafController.cs
Assets/Scripts/Audio/AudioPlaceholderPlayer.cs
Assets/Audio/Placeholders/
Assets/Scenes/Gameplay.unity
```

## Technical constraints

- Hijack shares StageController progress API
- No real VO/SFX generation

## Risks / unknowns

- Default WAF seconds are placeholders until playtest

## Links

- Acceptance: [`../tests/M-06-acceptance.md`](../tests/M-06-acceptance.md)
- GDD: WAF/CAF, Mechanics
- TAD: Audio, analytics waf/assist events
