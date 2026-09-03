# Rive prototype discrepancies (simi_prototype.riv)

**File:** `Assets/Art/Rive/simi_prototype.riv`  
**Updated:** 2026-09-03 (M-08 background-anchor harness)  
**Status:** Gameplay mounts `background` plus sibling widgets at `*-anchor` nodes. Leftover artboard `main` is not mounted.

C# mirrors live under `Assets/Scripts/UI/Rive/`.

**Linux Editor:** Rive Unity 0.4.3 only renders with **Vulkan**. OpenGLCore draws nothing (console: `Rive does not support OpenGLCore on Linux`). Standalone Linux Player Settings is Vulkan; **restart the Editor** after that change.

---

## Mount (M-08)

| Widget | Artboard | Hit testing |
|--------|----------|-------------|
| `BackgroundRive` | `background` | `None` (shell only) |
| `FaucetRive` | `faucet` at `faucet-anchor` | `Translucent` only while `OpenFaucetStage` enables it |
| `HandsRive` / `SoapRive` / `TowelRive` / `CharacterRive` | matching artboards | `None` until later stages |
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

Rive: independent L/R triggers (`faucet_L_On`, `faucet_L_Off`, `faucet_R_On`, `faucet_R_Off`) on the **faucet** artboard widget.

Unity: Open Water completes from either handle through `Faucet`. Close Water remains future configuration.

---

## Soap artboard name

The artboard is named `"soap "` (trailing space). Unity artboard dropdowns and `artboardByName` must use that exact string (`SimiPrototypeArtboards.Soap`).

---

## Drag vs Unity input

Soap and towel expose `isDragged`. Unity uses world-space `IntentInputRouter` only for future non-Faucet input families.

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
