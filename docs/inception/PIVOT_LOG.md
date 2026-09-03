# Pivot Log

Record of design and technical approach changes. Current TAD is [`tech/TAD-v3.md`](tech/TAD-v3.md). Milestone-driven inception is retired.

## Entries

## 2026-09-03 — v2 → v3: Rive background-anchor harness

- **Reason:** The prototype `.riv` no longer composes gameplay on artboard `main`. `background` is an empty shell with `*-anchor` nodes; component artboards and `progressBar` must be sibling Unity widgets.
- **Design:** [`design/GDD-v2.md`](design/GDD-v2.md) unchanged (six-stage wash, `OpenFaucetStage` first).
- **Technical approach:** Folded into [`tech/TAD-v3.md`](tech/TAD-v3.md). GameStage / `IGameFlowServices` stay; Rive mount switches to background + anchored widgets + progress overlay. Enable/disable gates hit-testing only.
- **Tests:** `Assets/Tests/EditMode/M08RiveAnchorTests.cs` and `Assets/Tests/PlayMode/M08RiveAnchorPlayModeTests.cs`. GameStage tests remain.
- **User decision:** Approved sibling widgets at empty anchors.

## 2026-08-30 — v1 → v2: polymorphic GameStage architecture

- **Reason:** Stage configuration and sequencing in `GameFlowController` are too tightly coupled. Stage-specific behavior should live in inheritable `GameStage` classes with an injected flow-services interface.
- **Design:** Created [`design/GDD-v2.md`](design/GDD-v2.md). The six-stage wash sequence remains unchanged; `OpenFaucetStage` is the first concrete stage and Intro/Outro remain flow-level states.
- **Technical approach:** `GameFlowController` became the ordered stage factory/coordinator; `GameStage` implementations consume `IGameFlowServices` and request transitions through that interface. That architecture now lives in [`tech/TAD-v3.md`](tech/TAD-v3.md).
- **Tests:** GameStage EditMode/PlayMode coverage under `Assets/Tests/`.
- **User decision:** User requested the `GameStage` / `OpenFaucetStage` inheritance and injected-interface architecture.

---

Milestone-driven inception (ROADMAP, per-milestone docs, acceptance checklists, one-milestone-per-session) is retired. Current design is [`design/GDD-v2.md`](design/GDD-v2.md); current TAD is [`tech/TAD-v3.md`](tech/TAD-v3.md).
