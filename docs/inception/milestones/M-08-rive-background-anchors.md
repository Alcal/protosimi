# M-08 — Rive background-anchor harness

**Status:** awaiting_user_verification
**Design version:** v3
**Depends on:** M-07b

## Objective

Mount `simi_prototype.riv` on Gameplay from artboard `background`, place sibling component widgets at `*-anchor` nodes, overlay `progressBar`, and gate Faucet hit-testing through existing `GameStage` enable/disable.

## Deliverables

- [x] Replace `Assets/Art/Rive/simi_prototype.riv` with the background-anchor export
- [x] `background` + sibling component widgets + `progressBar` overlay + `intro` on Gameplay
- [x] Anchor AABB → RectTransform mapper
- [x] Faucet / progress / step-icon adapters rebound off `main`
- [x] EditMode + PlayMode tests for mapping and enable/disable

## Done-when (acceptance criteria)

1. AC01: Gameplay mounts `background` (not `main`) as the playfield shell.
2. AC02: Sibling widgets exist for faucet, hands, soap, towel, character, step1–4, and progressBar, positioned from the named anchors.
3. AC03: `progressBar` is an overlay widget; progress is written to it (not to leftover `main` / `progress_num` on `main`).
4. AC04: `OpenFaucetStage` enable/disable still completes Open Water; disabled Faucet is not hittable and ignores activation.
5. AC05: Hands/soap/towel/character stay visible but `HitTestBehavior.None` until later stages.
6. AC06: Intro overlay still dismisses on Jugar and enters the first `GameStage`.

## Touch list

```
docs/inception/STATUS.md
docs/inception/PIVOT_LOG.md
docs/inception/milestones/ROADMAP.md
docs/inception/milestones/M-08-rive-background-anchors.md
docs/inception/tests/M-08-acceptance.md
docs/inception/tech/TAD-v3.md
docs/inception/design/RIVE_PROTOTYPE_DISCREPANCIES.md
Assets/Art/Rive/simi_prototype.riv
Assets/Art/Rive/README.md
Assets/Scripts/UI/Rive/SimiPrototypeArtboards.cs
Assets/Scripts/UI/Rive/SimiPrototypePresenter.cs
Assets/Scripts/UI/Rive/Faucet.cs
Assets/Scripts/UI/Rive/StepIcon.cs
Assets/Scripts/UI/Rive/MainProgress.cs
Assets/Scripts/UI/Rive/ProgressBar.cs
Assets/Scripts/UI/Rive/Background.cs
Assets/Scripts/UI/Rive/BackgroundAnchors.cs
Assets/Scripts/UI/Rive/ArtboardSpace.cs
Assets/Scripts/UI/Rive/RiveAnchorMount.cs
Assets/Scripts/UI/Rive/RiveAnchorFile.cs
Assets/Scripts/UI/RiveHudBinder.cs
Assets/Scripts/Editor/RivePrototypeMount.cs
Assets/Scenes/Gameplay.unity
Assets/Tests/EditMode/M08RiveAnchorTests.cs
Assets/Tests/PlayMode/M08RiveAnchorPlayModeTests.cs
```

## Technical constraints

- `GameStage` depends only on `IGameFlowServices`; no new stage subclasses.
- Components stay visible when disabled; only hit-testing is gated.
- Empty `*-anchor` groups have no drawable AABB. Place siblings from Node x/y plus component artboard size, shifted by that artboard’s origin (0,0 = top-left, 0.5,0.5 = center); map through Fit.Contain + Center with a Y-down → uGUI flip.
- Do not mount leftover `main`.
- Drive scene wiring through `RivePrototypeMount` / Unity CLI, not hand-edited YAML.

## Risks / unknowns

- `background` / `progressBar` state-machine names come from Unity FileMetadata after import.
- Rive AABB Y-axis vs uGUI must be verified in Play Mode.
- Four step icons vs six wash stages remains a content mapping issue; Open Faucet uses step 1.

## Links

- Acceptance: [`../tests/M-08-acceptance.md`](../tests/M-08-acceptance.md)
- GDD: [`../design/GDD-v2.md`](../design/GDD-v2.md) (unchanged)
- TAD: [`../tech/TAD-v3.md`](../tech/TAD-v3.md)
