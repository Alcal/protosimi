# Game states — Manos Limpias

Design version: **v1**

High-level session state machine. Play contains six ordered stages; assist is a nested mode that returns to the same stage.

```mermaid
stateDiagram-v2
    [*] --> Title
    Title --> Intro: StartPressed
    Intro --> Stage0_OpenWater: HudRevealed

    Stage0_OpenWater --> Stage1_WetHands: StageComplete
    Stage1_WetHands --> Stage2_RubSoap: StageComplete
    Stage2_RubSoap --> Stage3_Rinse: StageComplete
    Stage3_Rinse --> Stage4_CloseWater: StageComplete
    Stage4_CloseWater --> Stage5_DryHands: StageComplete
    Stage5_DryHands --> Win: StageComplete

    state AssistMode {
        [*] --> HostDemo
        HostDemo --> [*]: ProgressCompleteOrYield
    }

    Stage0_OpenWater --> AssistMode: WAF3
    Stage1_WetHands --> AssistMode: WAF3
    Stage2_RubSoap --> AssistMode: WAF3
    Stage3_Rinse --> AssistMode: WAF3
    Stage4_CloseWater --> AssistMode: WAF3
    Stage5_DryHands --> AssistMode: WAF3
    AssistMode --> Stage0_OpenWater: ResumeSameStage
    AssistMode --> Stage1_WetHands: ResumeSameStage
    AssistMode --> Stage2_RubSoap: ResumeSameStage
    AssistMode --> Stage3_Rinse: ResumeSameStage
    AssistMode --> Stage4_CloseWater: ResumeSameStage
    AssistMode --> Stage5_DryHands: ResumeSameStage

    Win --> Title: Replay
```

## Notes

- No pause/lose states in MVP.
- Resume after assist returns to the **same** stage index with progress preserved (or advanced if hijack completed the bar).
