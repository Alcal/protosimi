# Manos Limpias — Rive (`simi_prototype.riv`)

Source of truth: [`simi_prototype.riv`](simi_prototype.riv)

Mounted on Gameplay (play this scene, not Title):

- Artboard `main` — `progress_StateMachine` / `progress_num`
- Artboard `intro` — overlay; nested Jugar Button dismisses intro (`ButtonPress` / `BubbleButton`)

Typed C# contracts: `Assets/Scripts/UI/Rive/`.

The GDD/`VM_HUD` names in `docs/inception/design/RIVE_INTERFACES.md` do **not** match this file. See [`docs/inception/design/RIVE_PROTOTYPE_DISCREPANCIES.md`](../../../docs/inception/design/RIVE_PROTOTYPE_DISCREPANCIES.md).

Unity `HudPresenter` / `RiveHudBinder` still mirror the old contract and are not wired to this `.riv`.
