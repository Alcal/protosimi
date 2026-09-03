# Manos Limpias — Rive (`simi_prototype.riv`)

Source of truth: [`simi_prototype.riv`](simi_prototype.riv)

Mounted on Gameplay (play this scene, not Title):

- Artboard `background` — shell; sibling widgets snap to `*-anchor` nodes
- Artboard `progressBar` — overlay HUD fill (`progressNum`)
- Artboard `intro` — overlay; nested Jugar Button dismisses intro (`ButtonPress` / `BubbleButton`)
- Component artboards `faucet`, `hands`, `soap `, `towel`, `character`, `stepIcon` at their anchors

Leftover artboard `main` is not mounted.

Typed C# contracts: `Assets/Scripts/UI/Rive/`.

The GDD/`VM_HUD` names in `docs/inception/design/RIVE_INTERFACES.md` do **not** match this file. See [`docs/inception/design/RIVE_PROTOTYPE_DISCREPANCIES.md`](../../../docs/inception/design/RIVE_PROTOTYPE_DISCREPANCIES.md).
