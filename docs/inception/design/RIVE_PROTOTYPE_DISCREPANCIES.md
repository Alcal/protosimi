# Rive prototype discrepancies (simi_prototype.riv)

**File:** `Assets/Art/Rive/simi_prototype.riv`  
**Dumped:** 2026-08-30 (Rive canvas runtime; no Rive MCP)  
**Status:** Partially resolved in M-07. Open Water now uses the nested Rive main widget for hit testing, progress is bound to `progress_num`, and one runtime step-icon widget is mounted. The binary asset's nested input path remains optional because its instance name is not exposed by the prototype metadata.

C# mirrors live under `Assets/Scripts/UI/Rive/`. Mounted artboards: `main` and `intro` on Gameplay. Nested contracts exist for later wiring.

**Linux Editor:** Rive Unity 0.4.3 only renders with **Vulkan**. OpenGLCore draws nothing (console: `Rive does not support OpenGLCore on Linux`). Standalone Linux Player Settings is Vulkan; **restart the Editor** after that change.

---

## Playfield doubled

`main` already draws faucet, hands, soap, towel, character, progress, and step icons at 1920×1080.

Gameplay still has Unity graybox (`Playfield` / `Soap` / `Towel` / `Hands` / `Sink` / `Germs`); the legacy Faucet object and uGUI `PlayHudRoot` are disabled for the Rive Faucet milestone.

The Rive panel is a fullscreen overlay on `GameplayCanvas`. Both stacks are visible.

**Remaining:** hide the remaining Unity playfield as each component is migrated to Rive.

---

## HUD contract mismatch

[`RIVE_INTERFACES.md`](RIVE_INTERFACES.md) expected `AB_HUD_Root` / `VM_HUD`:

- `stageProgress`, `stageIndex`
- `icon0State` … `icon3State` (0 pending / 1 active / 2 complete)
- `hostVisible`, `hostAssistMode`, `hudVisible`
- triggers `stageCompletePulse`, `hostSpeakPulse`, `wafHighlightPulse`

The file uses state-machine inputs instead:

| Unity / GDD | Rive file |
|-------------|-----------|
| `stageProgress` | `main` / `progress_num` |
| `iconNState` 0–2 | `stepIcon` `isActive`, `isCompleted`, `step_ID` (1–4) |
| 6 wash stages | 4 step icons |
| `hostVisible` / `hostAssistMode` | `character` `popUp` / `popOut` / `wafID` / `isTalking` / `complete` |

`step_ 2` and `step_ 3` animation names include a space after the underscore.

---

## Faucet model

Rive: independent L/R triggers (`faucet_L_On`, `faucet_L_Off`, `faucet_R_On`, `faucet_R_Off`).

Unity: Open Water is now completed by either Rive handle activation through one Faucet adapter; the legacy `TapOpenClose` proximity path and Faucet collider are disabled. Close Water remains future configuration.

---

## Soap artboard name

The artboard is named `"soap "` (trailing space). Unity artboard dropdowns and `artboardByName` must use that exact string (`SimiPrototypeArtboards.Soap`).

---

## Nested input paths unknown

Child SM inputs are **not** on `main`’s `progress_StateMachine` (only `progress_num`).

Unity can set nested inputs via `Artboard.SetBooleanInputStateAtPath(name, value, path)`, but nested **instance names** inside `main` were not recoverable from the binary.

The new adapter keeps direct nested triggering opt-in; normal pointer interaction is handled by the main Rive widget and reported activation events. The remaining instance names are still needed for future direct control of the Faucet animation and other nested components.

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

Animations include `hidden`, `popup`, `popout`, `waf2`, `waf3`, `celebrate`, `blink`, `intro`.

This is not `hostVisible` / `hostAssistMode` / `hostSpeakPulse`.

---

## ViewModels

Only two file-level view models:

- `ViewModel1` — no properties (bound to `REF`)
- `BubbleButton` — `buttonTrig` (trigger), `buttonBool` (boolean)

Most gameplay state is SM inputs, not data binding. `RiveHudBinder` now writes the direct `main` progress input and the single mounted step icon instead of sending the obsolete `VM_HUD` names.

---

## Open questions for you

1. Hide / remove the remaining Unity graybox once all playfield components are Rive-owned?
2. Map six stages onto four `stepIcon` instances — which `step_ID` for Open vs Close water, Wet vs Rinse?
3. Nested component instance names inside `main` (for `Set*InputStateAtPath`)?
4. Dual faucet handles vs single tap-open/close — M-07 uses both Rive handles for Open Water; Close Water remains to be migrated.
5. Keep Title scene, or delete it now that intro Jugar is the start?
