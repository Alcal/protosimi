# Rive prototype discrepancies (simi_prototype.riv)

**File:** `Assets/Art/Rive/simi_prototype.riv`  
**Updated:** 2026-09-03 (background-anchor harness)  
**Status:** Gameplay mounts `background` plus sibling widgets at `*-anchor` nodes. Leftover artboard `main` is not mounted.

C# mirrors live under `Assets/Scripts/UI/Rive/`.

**Linux Editor:** Rive Unity 0.4.3 only renders with **Vulkan**. OpenGLCore draws nothing (console: `Rive does not support OpenGLCore on Linux`). Standalone Linux Player Settings is Vulkan; **restart the Editor** after that change.

---

## Mount

| Widget | Artboard | Hit testing |
|--------|----------|-------------|
| `BackgroundRive` | `background` | `None` (shell only) |
| `FaucetRive` | `faucet` at `faucet-anchor` | `Translucent` only while `OpenFaucetStage` enables it; `None` after the 25% lock so it stays visually open |
| `HandsRive` | `hands` at `hands-anchor` | `None` until the faucet is locked open, then `Translucent` and Unity-draggable; stays frozen after OpenFaucet exits |
| `SoapRive` | `"soap "` at `soap-anchor` | Always `None` (Unity panel drag; Rive hits would fire `isDragged` and clip the bar off the artboard). Draggable during `ApplySoapStage`; snaps home on release. |
| `TowelRive` / `CharacterRive` | matching artboards | `None` until later stages |
| `StepIcon1Rive` … `StepIcon4Rive` | `stepIcon` at `stepN-anchor` | `None` |
| `ProgressBarRive` | `progressBar` at `progressBar-anchor` | `None` |
| `IntroRive` | `intro` | `Opaque` until dismissed |

Anchor names do **not** include brackets (`faucet-anchor`, not `[faucet-anchor]`).

Empty `*-anchor` groups have no drawable AABB (`ComputeBounds()` is always 0 in Rive Unity 0.4.3). Unity places each sibling from the node’s artboard x/y plus the component artboard size, shifted by that artboard’s origin (faucet 0,0; soap/towel/stepIcon/progressBar 0.5,0.5), mapped through Fit.Contain + Center with a Y-down → uGUI flip.

---

## Playfield doubled

`background` + anchored Rive components draw the sink playfield. Gameplay still has Unity graybox (`Playfield` / `Soap` / `Towel` / `Hands` / `Sink` / `Germs`).

**Remaining:** hide the remaining Unity playfield as each component is migrated to Rive.

---

## HUD contract mismatch

[`RIVE_INTERFACES.md`](RIVE_INTERFACES.md) expected `AB_HUD_Root` / `VM_HUD`. The prototype still uses SM / ViewModel names from the `.riv`:

| Unity / GDD | Rive file |
|-------------|-----------|
| `stageProgress` | overlay `progressBar` `progressNum` (fallback `progress_num`) |
| `iconNState` 0–2 | four `stepIcon` widgets: `isActive`, `isCompleted`, `icon_ID` (fallback `step_ID`) 1–4 |
| 6 wash stages | 4 step icons |
| `hostVisible` / `hostAssistMode` | `character` `popUp` / `popOut` / `wafID` / `isTalking` / `complete` |

`step_ 2` and `step_ 3` animation names include a space after the underscore.

---

## Faucet model

Rive: independent L/R triggers (`faucet_L_On`, `faucet_L_Off`, `faucet_R_On`, `faucet_R_Off`) on the **faucet** artboard widget. Open vs closed is the persistent SM state (`active_L` / `activeR` vs `offL` / `off_R`).

Unity: `Faucet` polls those states in LateUpdate (plus bool/event fallbacks) and reports a `PointerHit` when a press lands in the faucet widget. Open Water locks the faucet at 25% from that pointer hit. Close Water remains future configuration and should not subscribe to `PointerHit`.

Wet-hands fill uses authored Unity hitboxes: `hitbox_1` / `hitbox_2` children of `HandsRive`, and `water-sqspot` under `FaucetRive`. Soap fill uses `soap-hitbox` under `SoapRive` against those same hand boxes. They are RectTransforms + trigger `BoxCollider2D`s sized as a fraction of the parent widget (the playtime artboard box). They do **not** follow Rive node names. Adjust them in the Rect tool; cyan gizmos mark the boxes.

---

## Soap artboard name

The artboard is named `"soap "` (trailing space). Unity artboard dropdowns and `artboardByName` must use that exact string (`SimiPrototypeArtboards.Soap`).

---

## Drag vs Unity input

Hands drag is Unity RectTransform motion on `HandsRive` after the faucet lock. Soap drag is the same family on `SoapRive` during `ApplySoapStage`, but the soap widget stays `HitTestBehavior.None` so the SM never sees the pointer (`isDragged` lift pose draws outside the 400×400 artboard and clips). Release snaps the panel home. Towel still exposes `isDragged` for later stages. Unity uses world-space `IntentInputRouter` only for leftover non-Rive input families.

---

## Intro vs Title

Intro’s nested Button label is **Jugar**; that click dismisses the overlay and starts play.

Title scene still has its own Jugar → Gameplay. **Intended path is play Gameplay directly.** Title is leftover.

---

## Host / character

Character SM `drSimi_StateMachine`: `isTalking`, `popUp`, `popOut`, `complete`, `wafID`.

This is not `hostVisible` / `hostAssistMode` / `hostSpeakPulse`.

---

## ViewModels

- `ProgressBar` — `progressNum` (number), bound to the overlay progress artboard
- `BubbleButton` — `buttonTrig` (trigger), `buttonBool` (boolean)

Most gameplay state is still SM inputs.

---

## Open questions

1. Hide / remove the remaining Unity graybox once all playfield components are Rive-owned?
2. Map six stages onto four `stepIcon` instances — which id for Open vs Close water, Wet vs Rinse?
3. Dual faucet handles vs single tap-open/close — Close Water remains to be migrated.
4. Keep Title scene, or delete it now that intro Jugar is the start?
