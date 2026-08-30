# Rive prototype discrepancies (simi_prototype.riv)

**File:** `Assets/Art/Rive/simi_prototype.riv`  
**Dumped:** 2026-08-30 (Rive canvas runtime; no Rive MCP)  
**Status:** Documented only — not solved this session.

C# mirrors live under `Assets/Scripts/UI/Rive/`. Mounted artboards: `main` and `intro` on Gameplay. Nested contracts exist for later wiring.

**Linux Editor:** Rive Unity 0.4.3 only renders with **Vulkan**. OpenGLCore draws nothing (console: `Rive does not support OpenGLCore on Linux`). Standalone Linux Player Settings is Vulkan; **restart the Editor** after that change.

---

## Playfield doubled

`main` already draws faucet, hands, soap, towel, character, progress, and step icons at 1920×1080.

Gameplay still has Unity graybox (`Playfield` / `Faucet` / `Soap` / `Towel` / `Hands` / `Sink` / `Germs`) and uGUI `PlayHudRoot`.

The Rive panel is a fullscreen overlay on `GameplayCanvas`. Both stacks are visible.

**Clarify later:** hide Unity playfield? replace it with Rive? HUD-only slot?

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

Unity: one `TapOpenClose` family for stages 0 (open) and 4 (close), single `Faucet` collider.

---

## Soap artboard name

The artboard is named `"soap "` (trailing space). Unity artboard dropdowns and `artboardByName` must use that exact string (`SimiPrototypeArtboards.Soap`).

---

## Nested input paths unknown

Child SM inputs are **not** on `main`’s `progress_StateMachine` (only `progress_num`).

Unity can set nested inputs via `Artboard.SetBooleanInputStateAtPath(name, value, path)`, but nested **instance names** inside `main` were not recoverable from the binary.

Need those names before driving faucet / step icons / character from Unity.

---

## Drag vs Unity input

Soap and towel expose `isDragged`. Unity uses world-space `IntentInputRouter` on graybox colliders. Not wired.

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

Most gameplay state is SM inputs, not data binding. `RiveHudBinder` still SendMessages the old `VM_HUD` names to a null `riveWidget`.

---

## Open questions for you

1. Hide / remove Unity graybox and uGUI HUD once Rive `main` is the playfield?
2. Map six stages onto four `stepIcon` instances — which `step_ID` for Open vs Close water, Wet vs Rinse?
3. Nested component instance names inside `main` (for `Set*InputStateAtPath`)?
4. Dual faucet handles vs single tap-open/close — keep both, or pick one model?
5. Keep Title scene, or delete it now that intro Jugar is the start?
