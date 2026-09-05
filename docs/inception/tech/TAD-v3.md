# Technical Approach Document — Manos Limpias (v3)

**GDD reference:** [`../design/GDD-v2.md`](../design/GDD-v2.md)

Current technical approach. `GameFlowController` is the composition root; concrete `GameStage` instances consume `IGameFlowServices`. Gameplay Rive mounts a `background` shell with sibling artboard widgets at named `*-anchor` nodes and a separate `progressBar` overlay.

## Architecture overview

Unity 2D orthographic minigame at fixed **1920×1080**, WebGL-first. Session flow lives in `GameFlowController`; HUD and playfield chrome are Rive-owned; leftover Unity graybox remains until each component is migrated.

- **Render / perspective:** 2D orthographic (first-person sink framing)
- **Scene strategy:** `Title` + `Gameplay` (intro / ordered stages / assist / win as states inside Gameplay)
- **Core systems:**
  - `GameFlowController` — composition root: Title → Intro → ordered `GameStage` instances → Outro; injects `IGameFlowServices`
  - `GameStage` — one stage’s behavior and subscriptions (`OpenFaucetStage` then `ApplySoapStage`)
  - `InputFamilies` — `TapOpenClose`, `HandsUnderWater`, `RubOnHands` (intent-tolerant); `IntentInputRouter` is a continuous non-Rive adapter only
  - `AssistHijack` — WAF3 host demo driving the same progress pipeline
  - `CameraFocus` — ease toward active stage focus
  - `RiveHudBinder` — progress and StepIcon portions of `IGameFlowServices`
  - `AudioPlaceholderPlayer` — silent clip keys
  - `AnalyticsStub` — Debug.Log / no-op sink for MVP events
- **Data approach:** ScriptableObject `FineTuningVariables` (rates, WAF timers, camera ease, hijack speed); `.riv` under `Assets/Art/Rive/`
- **Namespaces / folder layout:** `Assets/Scripts/{Core,Input,UI,Audio,Analytics}`, `Assets/Scenes`, `Assets/Data`, `Assets/Art/Rive`
- **Required package:** `com.unity.ai.assistant` in `Packages/manifest.json` (Unity MCP for Cursor — never omit)

```mermaid
flowchart TD
    title[TitleScene] -->|Play| gameplay[GameplayScene]
    gameplay --> flow[GameFlowController]
    flow -->|creates and orders| stages[GameStage instances]
    flow -->|injects| services[IGameFlowServices]
    services --> faucet[IFaucetControl]
    services --> hands[IHandsControl]
    services --> water[IWaterContact]
    services --> soap[ISoapControl]
    services --> progress[IProgressBar]
    services --> icon[IStepIcon]
    services --> camera[ICameraFocus]
    services --> host[IHostPresentation]
    stages --> open[OpenFaucetStage]
    stages --> applySoap[ApplySoapStage]
    open -->|SetEnabled| faucet
    open -->|SetDraggable| hands
    applySoap -->|SetDraggable| soap
    open -->|SetProgress| progress
    applySoap -->|SetProgress| progress
    open -->|requests advance| flow
    applySoap -->|requests advance| flow
    flow --> introState[Intro]
    flow --> outroState[Outro]
    flow --> assist[AssistHijack]
    flow --> audio[AudioPlaceholderPlayer]
    flow --> analytics[AnalyticsStub]
    panel[Rive Panel]
    panel --> bg[background widget]
    panel --> comps[anchor widgets]
    panel --> bar[progressBar overlay]
    panel --> intro[intro overlay]
    faucet --> faucetW[FaucetRive]
    progress --> bar
```

## Core contracts

### `GameStage`

`GameStage` is the base class for stage-specific behavior:

- receives `IGameFlowServices` once before entry;
- exposes `OnEnter()` and `OnExit()` lifecycle methods;
- may expose `Tick(float deltaTime)` for continuous input families;
- requests completion through the injected flow service rather than calling another stage directly;
- owns no scene lookup, global singleton, or hard-coded next-stage index.

The stage must unsubscribe from all service events in `OnExit()` so the inactive stage cannot react to later input.

### `IGameFlowServices`

The interface is supplied by `GameFlowController` and exposes narrow capabilities:

- Faucet enable/disable/reset state and left/right activation event;
- Hands drag enable/disable and water-contact overlap;
- Soap drag enable/disable, glow, overlap with hands hitboxes, and return-home;
- current-stage progress value and progress reset/fill operations;
- active `StepIcon` selection and completion state;
- camera focus and host presentation commands where needed;
- `RequestStageCompletion()` for the active stage;
- read-only session state and active stage order information.

Concrete Rive and Unity adapters implement these capabilities. `GameStage` depends on the interface, never on `RiveWidget`, `GameObject`, `HudPresenter`, or `GameFlowController`.

## Stage list and lifecycle

The controller owns an ordered serialized list of stage factories/configurations. At runtime it instantiates each configured stage, injects the service object, and tracks an index:

1. Enter Intro and prepare the first stage.
2. On intro dismissal, call `Enter()` on stage index 0.
3. The active stage enables its controls and subscribes to events.
4. On `RequestStageCompletion()`, the controller fills/commits progress, calls the active stage’s `OnExit()`, increments the index, and enters the next stage.
5. After the final list item exits, transition to Outro.
6. Replay resets the list and returns to Intro/Title without retaining stage state.

The controller may reject completion requests from inactive or already-exited stages. It must not use stage-index switches to determine behavior.

Use Unity-serializable stage assets/factories so designers can reorder or replace stages without editing flow code. The serialized list currently contains `OpenFaucetStage` then `ApplySoapStage`.

## Rive integration

- Gameplay mounts artboard `background` as a fullscreen, non-hittable shell (`HitTestBehavior.None`).
- Named nodes on `background` (`faucet-anchor`, `soap-anchor`, `towel-anchor`, `hands-anchor`, `character-anchor`, `step1-anchor`…`step4-anchor`, `progressBar-anchor`) are empty groups (no drawable AABB; Rive Unity `ComputeBounds()` is 0). Unity spawns a sibling `RiveWidget` per component, places it from the node’s artboard x/y plus the component artboard size shifted by that artboard’s origin (0–1), and maps that box through Fit.Contain + Center with a Y-down → uGUI flip.
- `progressBar` is a sibling overlay (not nested in `background`), placed at `progressBar-anchor`.
- `intro` remains a fullscreen overlay above the playfield.
- Leftover artboard `main` is not mounted.
- `RiveHudBinder` writes progress to the overlay `progressBar` widget and drives four `stepIcon` widgets by id. `isCompleted` is a Rive trigger: the binder fires it once per latched step (prior ids stay complete when the next stage activates) instead of writing a bool.
- `Faucet` binds to the dedicated faucet widget. `SetEnabled` toggles that widget’s `HitTestBehavior` (`Translucent` when enabled, `None` when disabled). Components stay visible; only the active stage’s interactable receives hits. The adapter exposes `LeftIsOpen` / `RightIsOpen` / `IsOpen` from faucet artboard state when Unity can see it, fires `Activated` on the rising edge, and fires `PointerHit` when a press lands in the faucet widget.
- `Hands` binds to the dedicated hands widget. `SetDraggable` toggles that widget’s `HitTestBehavior` and Unity RectTransform drag. After the first drag, `RiveAnchorMount` skips remounting that slot so a view resize does not snap the hands back. Disabling drag leaves `FreezePlacement` so the wet-hands pose survives into ApplySoap.
- Wet-hands overlap uses authored child RectTransforms (`hitbox_1` / `hitbox_2` on Hands, `water-sqspot` on Faucet) plus trigger `BoxCollider2D`s. Starting size is a fraction of the parent widget’s playtime box; designers move them in the Rect tool. Overlap is the root canvas’s local AABB (the 8-unit threshold is in authored canvas pixels, not world units), not Physics2D and not Rive node lookup.
- `Soap` binds to the dedicated soap widget. Unity RectTransform-drags the isolated soap panel; the widget stays `HitTestBehavior.None` so the soap SM cannot run `isDragged` (that pose draws outside the artboard and clips). Release or `ReturnHome` restores the pre-drag layout. A child `soap-hitbox` RectTransform + trigger `BoxCollider2D` (center ~50% of the widget) is the contact box.
- Hands, soap, towel, and character start at `HitTestBehavior.None`. `OpenFaucetStage` enables Hands drag after the faucet is locked open. `ApplySoapStage` enables Soap drag and leaves Hands undraggable.
- `SoapBubbles` nests dormant `game_bubbles` widgets under `hitbox_1` / `hitbox_2` (scale 0 until coverage wakes them; `RiveWidget.Speed` 0.95–1.05). Nested foam widgets do not count toward `RiveGlow` isolation. Scrub bubbles are a `ParticleSystem` on `SoapRive` using the extracted `Assets/Art/Rive/bubble.png` sprite (the in-band Rive image named `bubble`). GameplayCanvas is Screen Space – Camera so those particles composite with the UI.
- `OpenFaucetStage` enables the Faucet and locks it open from `PointerHit` (this stage only) or from `IsOpen` / `Activated`. It sets progress to `0.25`, disables faucet hits, enables Hands drag, then fills `+0.02` every `0.1s` while either hands hitbox overlaps `water-sqspot`. At `1` it marks its StepIcon completed and requests completion once.
- `ApplySoapStage` resets progress to `0`, glows the soap, and enables soap drag. Glow stops on the first grab. After that grab, fill is `+0.02` every `0.1s` while `soap-hitbox` overlaps `hitbox_1` or `hitbox_2`. Coverage of on-hand `game_bubbles` foam tracks that fill (`ISoapFoamControl.SetCoverage`); scrub particles emit only while grabbed and overlapping (`SetScrubbing`). At `1` it snaps soap home, disables soap, marks step 2 completed, and requests completion once. Foam stays on the hands after exit; intro/`ResetFoam` clears it.
- Rive state-machine names: [`../design/RIVE_INTERFACES.md`](../design/RIVE_INTERFACES.md) and [`../design/RIVE_PROTOTYPE_DISCREPANCIES.md`](../design/RIVE_PROTOTYPE_DISCREPANCIES.md).
- No stage class resolves Rive node names, widgets, or input paths.

## System responsibilities

- `GameFlowController`: lifecycle, ordered stage creation, transition guards, Intro/Outro, replay, and service implementation.
- `GameStage`: one stage’s behavior and subscriptions.
- `IntentInputRouter`: continuous non-Rive input adapter only; it must not own Faucet completion.
- `WafController`, `AssistHijack`, `CameraFocus`, audio, and analytics: accessed by the service implementation, not by concrete stage classes.

## Tooling

| Concern | Approach for MVP | Notes |
|---------|------------------|-------|
| Level building | Unity scenes + graybox GameObjects | No Tilemap required |
| Level / content loading | `SceneManager` Title ↔ Gameplay | Single vertical slice |
| Asset management | Folders under `Assets/Art`, `Assets/Data` | Addressables deferred |
| Variable tweaking | `FineTuningVariables` ScriptableObject | Inspector-editable |
| Editor tools | Optional `ManosLimpiasSetup` menu to wire scenes | Only if YAML wiring is brittle |

## Asset creation pipeline

| Asset class | Tool | Export into Unity | MVP scope |
|-------------|------|-------------------|-----------|
| UI / HUD / playfield motion | Rive | `.riv` → `Assets/Art/Rive/` | In |
| Playfield graybox | Unity-native | Sprites / primitives | In (hide as each component migrates to Rive) |
| Optional water/soap FX | Rive or Unity particles | Nested artboard or ParticleSystem | Nice-to-have |
| 3D meshes / rigs | Blender | — | Out |
| Concept / textures | ComfyUI | — | Out (post-MVP look-dev only) |

**ComfyUI for this project:** unused for MVP (post-MVP look-dev only)

**Export formats / import rules:**

- Rive: `Assets/Art/Rive/simi_prototype.riv`; mount `background` plus sibling widgets at `*-anchor` nodes (see discrepancy note)
- Unity sprites: PNG or generated Texture2D sprites for remaining graybox
- Audio: empty/silent `AudioClip` assets with stable keys (no generated banks)

**Linux Editor:** Rive Unity 0.4.3 only renders with **Vulkan**.

## Analytics event catalog

| Event name | Trigger | Payload | Design question answered |
|------------|---------|---------|--------------------------|
| `session_start` | App / Title load | `design_version` | Are sessions instrumented? |
| `play_pressed` | Title CTA | — | Drop-off before play? |
| `stage_start` | Stage index becomes active | `stageIndex` | Where do players enter each step? |
| `stage_complete` | CAF / progress ≥ 1 | `stageIndex`, `duration_s` | Which stages are slow? |
| `waf_triggered` | WAF1/2/3 fires | `stageIndex`, `level` | How often is help needed? |
| `assist_hijack` | WAF3 host demo starts | `stageIndex` | Does assist complete stages? |
| `session_complete` | Win reached | `total_s` | Do players finish the wash? |

MVP implementation: `AnalyticsStub` logs to console; no external SDK.

## Constraints

- Unity scaffold includes `com.unity.ai.assistant`, `FineTuningVariables`, `AnalyticsStub`, 1920×1080 Game view.
- Sequencing risk: Rive Unity package / ViewModel binding may lag — keep Unity debug HUD fallback; still author `.riv` contract.
- Unity MCP may be unavailable — prefer file scripts + Editor setup; Rive MCP for artboards.
- Persistence: none for MVP (session-only). Formal state machine + analytics are in-memory, not disk save.

## Open technical questions

- [x] Spanish (MX) assumed for copy placeholders — silent VO keys only in MVP
- [x] Germ pop VFX: nice-to-have; not blocking vertical slice
- [x] Rive file: `Assets/Art/Rive/simi_prototype.riv` with background-anchor harness
- [ ] Exact Rive Unity package version pin after first successful import (0.4.3 in use)

## Testing

**EditMode:** AABB → view-rect mapping including Fit.Fill; disabled Faucet ignores activation and `PointerHit` and sets `HitTestBehavior.None`; Hands and Soap `SetDraggable` toggle hit testing; Soap `ReturnHome` restores pre-drag layout; flow with fake service adapters verifies ordered entry, exit-before-next-entry, inactive-stage rejection, and final-stage Outro; `OpenFaucetStage` subscribes on entry, locks the faucet at progress `0.25` from `PointerHit` (this stage only) or either Faucet side / `IsOpen` on tick, enables Hands drag, fills `+0.02` per `0.1s` only while overlapping, marks the StepIcon completed at `1`, requests completion once, and unsubscribes on exit; `ApplySoapStage` glows and enables soap on entry, fills only after grab while overlapping a hands hitbox, then returns soap home and completes once; foam coverage tracks soap fill, scrubbing is true only while grabbed and overlapping, exit keeps coverage and clears scrubbing, intro resets foam. `RiveHudBinder` latches prior step ids as completed when the next stage activates; `isCompleted` is a trigger in `simi_prototype.riv`. Nested RiveWidgets under an isolated Hands widget do not break `RiveGlow.IsIsolated`.

**PlayMode:** background widget is present (not artboard `main`); intro still dismisses into the first `GameStage`. Faucet activate / pointer hit leaves the session in Stage with the faucet disabled. A two-stage list enters `OpenFaucetStage` then `ApplySoapStage`.

**Playtest:** Unity Editor, Vulkan on Linux (Rive 0.4.3). Open `Assets/Scenes/Gameplay.unity`, press Play, confirm the intro overlay and `background` playfield with sibling widgets at faucet/hands/soap/towel/character/step/progressBar anchors. Press `Jugar`; expect intro to dismiss and `OpenFaucetStage` to enable only the Faucet. Click the faucet widget; expect overlay progress at 25%, faucet no longer clickable but still open, and Hands draggable. Drag a hands hitbox over the water spot; expect the bar to climb ~2% every 100ms while overlapping, pause when leaving, then step 1 completed. Soap then glows and is draggable; drag it onto a hand hitbox until the bar fills — foam `game_bubbles` should grow on the hands and small bubbles should rise and fade while scrubbing — then soap snaps home, foam stays, step 2 completes, and Outro. Nodes `hitbox_1`, `hitbox_2` (hands), `water-sqspot` (faucet), and `soap-hitbox` (soap) are authored Unity children.
