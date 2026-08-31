# M-07 Acceptance — Rive Faucet state machine

**Status:** superseded by the v2 `GameStage` architecture. Regenerate acceptance criteria after TAD-v2 approval.

## Automated checks

- [ ] A configured stage enters once and invokes `OnEnter` once.
- [ ] Completing a configured stage invokes `OnExit` once and clamps progress to `1`.
- [ ] Completing the final configured stage enters the completed/outro state without advancing to another stage.
- [ ] Both Faucet activation values complete the same Open Water stage.
- [ ] Repeated Faucet activation after completion is ignored.

## Manual Unity/Rive checks

1. Restart the Linux Unity Editor with Vulkan selected.
2. Open `Gameplay` and enter Play Mode.
3. Confirm the Rive intro is visible and `Jugar` enters Open Water.
4. Confirm the Unity Faucet GameObject is not needed: clicking either left or right Faucet handle in the Rive `main` artboard completes Open Water.
5. Confirm the Rive progress bar fills to 100% and the single right-rail `stepIcon` changes from active to complete.
6. Confirm no stage 1 begins after completion and the game remains in its completed/outro state.
7. Confirm a missing Rive input/path logs once and does not break intro or scene startup.

**Sign-off:** pending user verification.
