# M-05 — Graybox content

**Status:** done
**Design version:** v1
**Depends on:** M-04

## Objective

Add Unity graybox sink playfield (faucet, soap, towel, hands) with stage-mapped foci and optional germ pop VFX.

## Deliverables

- [ ] Graybox GameObjects + colliders for foci
- [ ] Camera framing for ~30° sink view (2D approximation)
- [ ] Optional germ circles that despawn on soap/wash contact
- [ ] Focus enable/disable per stage

## Done-when (acceptance criteria)

1. AC01: Each stage has a visible focus target in the playfield
2. AC02: Input only registers on the active stage focus
3. AC03: Germs (if present) clear during soap and/or rinse progress

## Touch list

```
Assets/Scripts/Core/PlayfieldFoci.cs
Assets/Scripts/Core/GermPop.cs
Assets/Scenes/Gameplay.unity
Assets/Art/ (optional sprites)
```

## Technical constraints

- Unity-native graybox only; no final art
- Hitboxes define gameplay — keep simple shapes

## Risks / unknowns

- Visual polish deferred to M-06

## Links

- Acceptance: [`../tests/M-05-acceptance.md`](../tests/M-05-acceptance.md)
- GDD: Content scope, Mechanics
- TAD: Asset pipeline Unity-native
