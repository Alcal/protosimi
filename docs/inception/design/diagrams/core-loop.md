# Core loop — Manos Limpias

Design version: **v1**

Player intent on the active focus fills the current stage bar. Completing a stage triggers CAF and unlocks the next stage. Idle time escalates WAF without punishing the player.

```mermaid
flowchart TD
  waitInput[WaitForIntent]
  readIntent[ReadIntentTolerantInput]
  addProgress[AddProgressFromFineTuning]
  updateHud[UpdateRiveStageProgress]
  checkDone{progress >= 1.0}
  caf[CAF_PraiseAndCheckmark]
  nextStage[AdvanceStageIndex]
  idleTick[IdleTimer]
  waf1[WAF1_Rehighlight]
  waf2[WAF2_HostHint]
  waf3[WAF3_HostHijack]
  waitInput --> readIntent
  readIntent --> addProgress
  addProgress --> updateHud
  updateHud --> checkDone
  checkDone -->|no| waitInput
  checkDone -->|yes| caf --> nextStage --> waitInput
  waitInput --> idleTick
  idleTick -->|threshold1| waf1 --> waitInput
  idleTick -->|threshold2| waf2 --> waitInput
  idleTick -->|threshold3| waf3 --> addProgress
```

## Notes

- Only one stage accepts input at a time.
- Hijack (WAF3) drives the same progress pipeline so assist and player share one completion rule.
- Rates and idle thresholds: `FineTuningVariables`.
