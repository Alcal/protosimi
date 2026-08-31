# Pivot Log

Record design and technical approach changes between versions. Implementation agents must read this before continuing.

## Entries

## 2026-08-30 — v1 → v2: polymorphic GameStage architecture

- **Reason:** Stage configuration and sequencing in `GameFlowController` are too tightly coupled. Stage-specific behavior should live in inheritable `GameStage` classes with an injected flow-services interface.
- **Design:** Created [`design/GDD-v2.md`](design/GDD-v2.md). The six-stage wash sequence remains unchanged; `OpenFaucetStage` is the first concrete stage and Intro/Outro remain flow-level states.
- **Technical approach:** Created [`tech/TAD-v2.md`](tech/TAD-v2.md). `GameFlowController` becomes the ordered stage factory/coordinator; `GameStage` implementations consume `IGameFlowServices` and request transitions through that interface.
- **Affected milestone:** M-07 is superseded before user verification. Its configuration-driven `StageController` approach must not be extended; regenerate the Faucet milestone after TAD-v2 approval.
- **Affected tests:** Supersede `tests/M-07-acceptance.md` and its EditMode coverage when the new M-07b milestone and acceptance artifacts are planned.
- **User decision:** User requested the `GameStage` / `OpenFaucetStage` inheritance and injected-interface architecture.

---

## Pivot rules

1. Never delete old GDD or TAD versions; create `GDD-vN+1.md` and `TAD-vN+1.md` as needed
2. Mark completed milestones as-is; supersede or branch (`M-02b`) for in-flight work
3. Regenerate acceptance docs and test stubs only for affected milestones
4. Update `STATUS.md` `design_version` and `next_action`
5. Architecture / pipeline / analytics pivots require a new TAD before regenerating plan
