# Pivot Log

Record design and technical approach changes between versions. Implementation agents must read this before continuing.

## Entries

_(No pivots yet — baseline is GDD-v1.)_

---

## Pivot rules

1. Never delete old GDD or TAD versions; create `GDD-vN+1.md` and `TAD-vN+1.md` as needed
2. Mark completed milestones as-is; supersede or branch (`M-02b`) for in-flight work
3. Regenerate acceptance docs and test stubs only for affected milestones
4. Update `STATUS.md` `design_version` and `next_action`
5. Architecture / pipeline / analytics pivots require a new TAD before regenerating plan
