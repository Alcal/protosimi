# Rive Interfaces — Manos Limpias (v1)

**Audience:** Tech Art / Artists / Unity integration  
**Viewport:** 1920 × 1080  
**Constraint:** Graybox shapes only — do **not** import or generate final image assets in this phase. HUD appearance and layout are owned by **Rive layout systems**; Unity binds data only.

**File status:** Open Rive project **protosimi** authored with `AB_HUD_Root` (1920×1080), graybox layout, and ViewModel `VM_HUD` matching this contract. Export `.riv` to `Assets/Art/Rive/ManosLimpias_HUD.riv` for Unity import. Until export, Unity `HudPresenter` mirrors the same property names.

Related: [`GDD-v1.md`](GDD-v1.md) · sketch [`screens/refs/hud-sketch.png`](screens/refs/hud-sketch.png)

---

## Artboard inventory (required names)

Tag / name artboards **exactly** as below so Unity and tools can resolve them.

| Artboard name | Size | Role |
|---------------|------|------|
| `AB_HUD_Root` | 1920×1080 | Layout root. Uses Rive layouts to place progress, stage list, and host. Mounted as the primary HUD runtime. |
| `AB_HUD_StageProgress` | Flexible (nested or component) | Top horizontal progress bar for **current stage** fill 0–1. |
| `AB_HUD_StageList` | Flexible (nested or component) | Right column of **4** step icons + state visuals. |
| `AB_HUD_Host` | Flexible (nested or component) | Bottom-left circular host portrait / assist takeover presentation. |
| `AB_FX_WaterSoap` | Flexible | Optional shared graybox FX (water drops, soap bubbles). May nest under gameplay Rive later; keep name reserved. |
| `AB_Gameplay_Sink` | 1920×1080 (or matched safe area) | Optional separate file/artboard for sink + hands + props graybox. If deferred, Unity sprites may graybox playfield until Tech Art owns it — **HUD artboards above remain mandatory.** |

### Naming rules

- Prefix `AB_` = artboard (do not rename without updating this doc + Unity bindings).
- Nested layout nodes under `AB_HUD_Root` should keep readable English names: `Layout_TopBar`, `Layout_RightRail`, `Layout_HostSlot`.
- Icon slots under `AB_HUD_StageList`: `Icon_Faucet`, `Icon_WetRinse`, `Icon_Soap`, `Icon_Towel`.

---

## Layout composition (`AB_HUD_Root`)

Match sketch (`hud-sketch.png`):

```
+--------------------------------------------------+
|  [======== Stage Progress Bar ========]          |
|                                      [Icon0]     |
|                                      [Icon1]     |
|         (transparent play-through)   [Icon2]     |
|                                      [Icon3]     |
|  (Host)                                          |
+--------------------------------------------------+
```

| Region | Placement | Notes |
|--------|-----------|-------|
| Stage progress | Top, horizontal, full usable width minus right rail | Fill only; no stage index text required |
| Stage list | **Right** column, vertical stack of 4 equal slots | Not left — sketch supersedes early “left column” notes |
| Host | Bottom-left circular frame | Visible when `hostVisible`; stronger present when `hostAssistMode` |
| Center | Empty / transparent | Unity camera + gameplay draw underneath |

Use Rive **Layout** / constraint components so 1920×1080 remains the design size and scales cleanly in WebGL letterboxing if needed.

---

## ViewModel / data binding contract

Preferred integration: **Rive ViewModel** bound from Unity. If ViewModel is not yet available in the pipeline, mirror the same names as **State Machine inputs** on `AB_HUD_Root` (or nested SMs) — do not invent parallel naming.

### Numbers

| Name | Type | Range | Meaning |
|------|------|-------|---------|
| `stageProgress` | number | 0.0–1.0 | Fill of top bar for the **active** stage (resets to 0 on stage change). |
| `stageIndex` | number | 0–5 | Logical wash step: 0 Open, 1 Wet, 2 Soap, 3 Rinse, 4 Close, 5 Dry. |

### Icon states

Prefer a single enum-style number per icon (0/1/2) for WebGL simplicity:

| Name | Type | Values | Meaning |
|------|------|--------|---------|
| `icon0State` | number | 0 pending, 1 active, 2 complete | Faucet (stages 0 & 4) |
| `icon1State` | number | 0 / 1 / 2 | Wet/Rinse (stages 1 & 3) |
| `icon2State` | number | 0 / 1 / 2 | Soap (stage 2) |
| `icon3State` | number | 0 / 1 / 2 | Towel (stage 5) |

**Shared-icon rule:** When stage 3 (Rinse) starts, `icon1State` returns to **1 (active)** even if it was **2** after Wet. Same for faucet at stage 4 Close.

### Booleans

| Name | Type | Meaning |
|------|------|---------|
| `hostVisible` | bool | Host portrait / panel shown |
| `hostAssistMode` | bool | Host is hijacking / demonstrating; emphasize assist presentation |
| `hudVisible` | bool | Entire HUD chrome (false during title; true after intro) |

### Triggers

| Name | Type | Meaning |
|------|------|---------|
| `stageCompletePulse` | trigger | Fire on CAF — checkmark pop / bar celebration |
| `hostSpeakPulse` | trigger | Fire when a VO line starts (lip-flap / talk graybox OK) |
| `wafHighlightPulse` | trigger | WAF1 re-highlight of the active focus / icon |

---

## State machine expectations (per artboard)

### `AB_HUD_StageProgress`

- Idle display driven by `stageProgress`.
- On `stageCompletePulse`: short complete flash, then wait for Unity to zero `stageProgress` for the next stage.

### `AB_HUD_StageList`

- Each icon listens to its `iconNState`.
- **Active** = glow / scale / dashed highlight (graybox).
- **Complete** = green check badge (graybox circle + mark).
- **Pending** = muted.

### `AB_HUD_Host`

- `hostVisible == false` → hidden or collapsed.
- `hostAssistMode == true` → “teaching” pose / frame emphasis.
- `hostSpeakPulse` → brief talk motion (optional graybox).

---

## Graybox content rules

Allowed:

- Solid rectangles, ellipses, simple paths
- Flat placeholder colors
- Simple checkmark path
- Layout guides / hit-area outlines if useful for artists later

Not allowed in this phase:

- Imported bitmaps / AI-generated images
- Final Dr. Simi illustration (silhouette circle is enough)
- Final soap/faucet icon art (label text or simple pictogram shapes OK)

---

## Stage ↔ icon mapping (artist cheat sheet)

| `stageIndex` | Stage | Active icon | Notes |
|--------------|-------|-------------|-------|
| 0 | Open water | `icon0` Faucet | |
| 1 | Wet hands | `icon1` Wet/Rinse | |
| 2 | Rub soap | `icon2` Soap | Foam FX optional via `AB_FX_WaterSoap` |
| 3 | Rinse | `icon1` Wet/Rinse | Re-activate after complete |
| 4 | Close water | `icon0` Faucet | Re-activate after complete |
| 5 | Dry hands | `icon3` Towel | |

---

## Silent audio placeholder keys (Unity)

Do **not** generate real SFX/VO. Unity reserves empty/silent `AudioClip` assets with these stable IDs:

| Key | When |
|-----|------|
| `vo_welcome` | Intro host welcome |
| `vo_stage_0` … `vo_stage_5` | Per-stage instruction |
| `vo_caf_praise` | CAF after any stage |
| `vo_waf_hint` | WAF2 contextual hint |
| `vo_waf_assist` | WAF3 “Déjame ayudarte…” |
| `vo_complete` | Win celebration |
| `sfx_caf_positive` | Stage complete sting |
| `sfx_progress_tick` | Optional progress feedback |
| `sfx_water_loop` | Water running (silent stub) |
| `sfx_soap_rub` | Rub family (silent stub) |
| `sfx_towel_rub` | Dry stage (silent stub) |
| `sfx_germ_pop` | Germ destroy (silent stub) |

Rive may expose `hostSpeakPulse` when Unity starts any `vo_*` clip; Rive does not play audio itself in MVP.

---

## Unity bind checklist (for TAD / eng)

1. Load artboard `AB_HUD_Root` at 1920×1080 overlay.
2. Bind ViewModel properties listed above each frame / on change.
3. On stage advance: set `stageIndex`, reset `stageProgress` to 0, update `iconNState`s, fire `stageCompletePulse` on the frame of completion (before reset).
4. On WAF1: `wafHighlightPulse`. On WAF2/3: `hostVisible=true`; on WAF3 also `hostAssistMode=true`.
5. Title screen: `hudVisible=false`. After intro: `hudVisible=true`.

---

## Handoff checklist (Tech Art)

- [ ] Create `ManosLimpias_HUD.riv` (or agreed filename) with artboards named exactly as in this doc
- [ ] Build `AB_HUD_Root` layout: top bar, right rail, host slot
- [ ] Wire ViewModel (or SM inputs) with the **exact** property names above
- [ ] Graybox only — no final bitmaps
- [ ] Open file in Rive editor so MCP/tools have file context for iteration
- [ ] Export / commit path agreed with eng (TAD will specify Unity StreamingAssets / Rive asset location)

---

## Change control

Any rename of artboards or ViewModel fields requires:

1. Update this document (bump design note in STATUS session log)
2. Update Unity binders
3. Notify Tech Art before merging art

**Document version:** v1 (aligned with GDD-v1)
