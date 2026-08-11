# Progression — Manos Limpias

Design version: **v1**

Linear six-stage wash sequence. HUD shows four icons; shared icons reactivate for their second logical stage.

## Stage sequence

```mermaid
flowchart LR
  s0[OpenWater]
  s1[WetHands]
  s2[RubSoap]
  s3[Rinse]
  s4[CloseWater]
  s5[DryHands]
  s0 --> s1 --> s2 --> s3 --> s4 --> s5
```

## Icon state machine (per icon)

```mermaid
stateDiagram-v2
    [*] --> Pending
    Pending --> Active: StageNeedsThisIcon
    Active --> Complete: OwningStagesDone
    Complete --> Active: SharedIconSecondStage
    Active --> Complete: SecondStageDone
```

## Icon ownership

| Icon id | Label | Owns stages |
|---------|-------|-------------|
| `icon0` | Faucet | 0 Open, 4 Close |
| `icon1` | Wet/Rinse | 1 Wet, 3 Rinse |
| `icon2` | Soap | 2 Rub soap |
| `icon3` | Towel | 5 Dry |

## Economy

None — no currency, XP, or unlocks in MVP.
