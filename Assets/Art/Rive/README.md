# Manos Limpias HUD (Rive)

Open Rive project **protosimi** contains:

- Artboard `AB_HUD_Root` (1920×1080)
- Layout: `Layout_TopBar` / `Layout_RightRail` (Icon_Faucet, Icon_WetRinse, Icon_Soap, Icon_Towel) / `Layout_HostSlot`
- ViewModel `VM_HUD` with properties from `docs/inception/design/RIVE_INTERFACES.md`

**Export:** File → Export `.riv` from the Rive editor into this folder as `ManosLimpias_HUD.riv`, then assign to a Rive Widget and wire `RiveHudBinder.riveWidget`.

Until then, Unity `HudPresenter` (uGUI) mirrors the same ViewModel contract for WebGL playability.
