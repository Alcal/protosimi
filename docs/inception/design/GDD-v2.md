# Game Design Document — Manos Limpias (v2)

**Collection:** Hábitos Saludables con el Dr. Simi  
**Supersedes:** [`GDD-v1.md`](GDD-v1.md) for future implementation planning  
**Reason for revision:** Stage behavior is now authored as polymorphic `GameStage` implementations while the flow controller owns only lifecycle and ordering.

## Game concept

**Manos Limpias** is a short, fail-free hand-washing minigame for children ages 3–6. From a first-person sink view, the player completes six hygiene stages—open water, wet hands, rub soap, rinse, close water, and dry hands—while Dr. Simi guides, celebrates, and can assist when the child stalls.

The game remains intent-driven: rough taps, drags, and rubs advance the active stage without a fail state.

## Core loop

The session lifecycle is:

```
Intro → ordered GameStage instances → Outro
```

Only the active `GameStage` owns its interaction subscriptions and stage-specific behavior. When it completes, the flow controller exits it and enters the next configured stage. After the final configured stage, the flow controller enters Outro.

## Stages

The MVP keeps the six logical stages and their existing input families:

| Order | Stage class | Input family | Rive focus |
|------:|-------------|--------------|------------|
| 0 | `OpenFaucetStage` | `TapOpenClose` | Tap the faucet widget |
| 1 | `WetHandsStage` | `HandsUnderWater` | Hands |
| 2 | `RubSoapStage` | `RubOnHands` | Soap / hands |
| 3 | `RinseStage` | `HandsUnderWater` | Hands |
| 4 | `CloseFaucetStage` | `TapOpenClose` | Either Faucet handle |
| 5 | `DryHandsStage` | `RubOnHands` | Towel |

The configured list currently contains only `OpenFaucetStage`. Later stage classes can be added without changing `GameFlowController` sequencing code.

### OpenFaucetStage

`OpenFaucetStage` extends `GameStage`. On entry it enables the Faucet, marks its StepIcon active, and subscribes to pointer hits plus left/right activation. A tap on the faucet widget is enough for this stage. The stage sets the progress bar to 100%, marks the StepIcon completed, and asks the flow controller to advance once. On exit it unsubscribes and disables the Faucet.

## UI and feedback

- The progress bar represents the current stage and is controlled through the flow-services interface.
- The configured stage owns its `StepIcon` metadata and updates the active/completed state through the same interface.
- Rive remains the visual and interaction source for the Faucet; the Unity Faucet GameObject is not an interaction target.
- Intro and Outro remain flow-level states, not `GameStage` implementations.

## Scope

No other gameplay scope changes from v1:

- Six-stage hand-washing vertical slice remains the target.
- No fail state, persistence, accounts, or additional levels.
- Rive artwork remains graybox and WebGL-first.
- WAF, audio placeholders, camera focus, analytics, and replay remain flow services.

## Approval

- [x] User approves this design revision for technical planning.
