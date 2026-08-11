# Game Design Document — Manos Limpias (v1)

**Collection:** Hábitos Saludables con el Dr. Simi  
**Source inputs:** GDD PDF prototype, HUD sketch (`screens/refs/hud-sketch.png`), project constraints (1920×1080, Rive HUD, graybox art, silent SFX placeholders)

## Elevator pitch

**Manos Limpias** is a short, fail-free hand-washing minigame for children ages 3–6. From a first-person sink view, the player completes six hygiene stages—open water, wet hands, rub soap, rinse, close water, dry hands—while Dr. Simi guides, celebrates, and can take over to demonstrate when the child stalls. Progress is always forward: intent matters more than precision, and soap + rinse clear cartoon germs so the habit feels rewarding and transferable to real life.

## Design pillars

1. **Intent over precision** — The game reads the child’s intention (rough taps, drags, and rubs), never requiring dexterity that would frustrate ages 3–6.
2. **No fail state** — There is no lose condition, no punishment, and only one available step at a time; stuck players get escalating help instead of retries.
3. **Dr. Simi as guide** — The host instructs, motivates, and can hijack any mechanic to demonstrate; he is mentor, not the playable hero.

## Target player & platform

- **Audience:** Children 3–6 (México); caregivers may assist but the UI is child-first
- **Platform:** Unity 2D, **WebGL-first** performance; fixed **1920×1080** viewport
- **Session length:** 2–3 minutes per playthrough
- **Locale (assumed):** Spanish (MX) for VO and on-screen copy unless pivoted

## Core loop

Within a single stage, the player repeats a short intent → progress → feedback cycle until the stage bar fills, then advances.

```
[Intentful input on focus] → [Stage progress 0→1] → [Visual/VO feedback]
        ↑                                                      ↓
        └────────── (while < 1.0) ──────────── [CAF + next stage] ─┘
```

Diagram: [`diagrams/core-loop.md`](diagrams/core-loop.md)

## Mechanics (MVP)

Stages share **three input families**. Each stage advances a float **progress 0.0 → 1.0**; advance rates live on a central ScriptableObject **`FineTuningVariables`** (field list finalized in TAD).

| Mechanic / stage | Input family | Description | Priority |
|------------------|--------------|-------------|----------|
| Abrir la llave | `TapOpenClose` | Loose taps / short drags / twists on the faucet focus; no precision required | Must-have |
| Mojar las manos | `HandsUnderWater` | Bring / hold hands under running water; dirt fades as progress rises | Must-have |
| Aplicar jabón y frotar | `RubOnHands` | Drag soap or touch hands; foam appears at contact; germs pop; bar shifts dirty→clean feel | Must-have |
| Enjuagar | `HandsUnderWater` | Same family as wet; soap/foam clears under water | Must-have |
| Cerrar la llave | `TapOpenClose` | Same family as open; faucet focus returns | Must-have |
| Secar las manos | `RubOnHands` | Same family as soap; towel is the prop | Must-have |
| Camera focus ease | — | Camera gently eases toward the active stage’s focus point | Must-have |
| Mechanic hijack (assist) | — | Host can take over any mechanic and partially/fully demonstrate the goal | Must-have |
| CAF | — | On stage complete: checkmark, positive SFX placeholder, VO praise, unlock next | Must-have |
| WAF 1–3 | — | Idle → re-highlight; longer idle → host hint; still stuck → host hijack demo | Must-have |
| Virus/bacteria VFX | — | Germs vanish on soap/wash contact (graybox circles OK) | Nice-to-have |
| Title + win celebration | — | Title “Manos Limpias”; end: clean shiny hands + host praise | Must-have |

### Stage ↔ HUD icon mapping (4 icons / 6 stages)

| Stage index | Stage | Right-column icon |
|-------------|-------|-------------------|
| 0 | Open water | Faucet |
| 1 | Wet hands | Hands under water |
| 2 | Rub soap | Soap |
| 3 | Rinse | Hands under water (re-active) |
| 4 | Close water | Faucet (re-active after rinse) |
| 5 | Dry hands | Towel |

Icon visual states: **pending** | **active** (highlighted) | **complete** (checkmark). Shared icons flip back to **active** when their second logical stage begins.

## Content scope (MVP)

**MVP vertical slice:** The player can complete all six hand-washing stages in one WebGL session at 1920×1080, with Rive HUD progress, camera focus easing, and host assistance hijack—using graybox art and silent audio placeholders.

- **In scope:**
  - Title → intro VO → play (6 stages) → win / celebrate
  - Rive-owned HUD layout (top stage bar, right stage icons, host portrait)
  - Three reusable input families + `FineTuningVariables` rates / WAF timers
  - Camera ease-to-focus per stage
  - Hijackable mechanics for host demonstration (WAF 3)
  - Graybox Rive artwork only; silent SFX/VO clip placeholders
  - Fixed 1920×1080; WebGL-first constraints documented in TAD

- **Out of scope (post-MVP):**
  - Final character/prop illustration, real VO recording, real SFX banks
  - Multiple sinks / levels / difficulty modes
  - Accounts, cloud saves, leaderboards, IAP
  - Full collection hub for other Dr. Simi habit games
  - Localization beyond the single MVP language pack
  - Mobile-native builds as primary target (Web first)

## Progression & economy

N/A for this game — linear six-stage sequence within one short session. No currency or unlocks.

Diagram: [`diagrams/progression.md`](diagrams/progression.md)

## UI / UX overview

- **Top bar:** Fill 0–1 for the **current stage** only (resets each stage).
- **Right column:** Four step icons with pending / active / complete (see mapping above). Sketch reference: [`screens/refs/hud-sketch.png`](screens/refs/hud-sketch.png).
- **Host:** Bottom-left circular portrait; appears for intro, CAF praise, WAF hints, and assist hijack.
- **Play field:** First-person hands + graybox faucet / soap / towel foci; ~30° sink perspective from PDF.
- **Rive:** All HUD chrome appearance and layout via Rive layout systems; Unity binds data only. See [`RIVE_INTERFACES.md`](RIVE_INTERFACES.md).

Screen map: [`screens/SCREEN_MAP.md`](screens/SCREEN_MAP.md)  
States: [`diagrams/game-states.md`](diagrams/game-states.md)

## Technical notes

- **Engine:** Unity (C#), 2D, WebGL-first
- **Viewport:** 1920×1080
- **GUI / imagery mount:** Rive (HUD + graybox artboards); Unity does not own HUD layout
- **Input:** Pointer (mouse/touch) — taps, short drags, rub gestures; intent-tolerant thresholds from `FineTuningVariables`
- **Tuning:** ScriptableObject `FineTuningVariables` — stage advance rates, camera ease, WAF idle timers, hijack demo speeds
- **Audio:** Silent placeholder clips keyed by stable IDs (see RIVE_INTERFACES / TAD audio table); no generated SFX/VO assets in MVP engineering
- **Save data:** None required for MVP (session-only)
- **Key scenes (planned):** `Title`, `Gameplay` (intro + stages + win as state machine inside play scene preferred)
- **Unity project:** Not created yet — scaffold + `com.unity.ai.assistant` land in **tech** phase

## Open questions

- [ ] Confirm Spanish (MX) as the only MVP language for VO/UI copy
- [ ] Confirm title screen + end celebration remain in MVP (assumed yes)
- [ ] Exact default WAF idle seconds — placeholders in `FineTuningVariables` until playtest
- [ ] Whether germ pop VFX is required for vertical-slice acceptance or polish milestone
- [ ] Open a `.riv` file in the Rive editor so graybox artboards can be authored via MCP (server is ready; no file context yet)

## Approval

- [x] User approved this version for milestone planning (advance STATUS → `tech`, produce TAD-v1)
