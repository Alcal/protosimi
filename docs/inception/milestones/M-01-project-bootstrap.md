# M-01 — Project bootstrap

**Status:** done
**Design version:** v1
**Depends on:** —

## Objective

Scaffold the Unity 2D project with required packages, folder layout, FineTuningVariables, analytics stub, and empty Title/Gameplay scenes at 1920×1080.

## Deliverables

- [ ] Scene(s): `Assets/Scenes/Title.unity`, `Assets/Scenes/Gameplay.unity`
- [ ] Scripts: `FineTuningVariables`, `AnalyticsStub`, optional setup helper
- [ ] Prefabs / assets: none required
- [ ] Data / ScriptableObjects: `Assets/Data/FineTuningVariables.asset`
- [ ] Tooling / analytics: `AnalyticsStub` with event name constants
- [ ] Packages: `com.unity.ai.assistant`, `com.unity.test-framework`, Rive runtime when available

## Done-when (acceptance criteria)

1. AC01: Project opens in Unity with `Packages/manifest.json` containing `com.unity.ai.assistant`
2. AC02: Title and Gameplay scenes exist and are in Build Settings
3. AC03: `FineTuningVariables` asset exists with stage rates and WAF timer fields
4. AC04: Game view / reference resolution documented as 1920×1080

## Touch list

```
Assets/Scenes/Title.unity
Assets/Scenes/Gameplay.unity
Assets/Scripts/Core/FineTuningVariables.cs
Assets/Scripts/Analytics/AnalyticsStub.cs
Assets/Data/FineTuningVariables.asset
Packages/manifest.json
ProjectSettings/
```

## Technical constraints

- Repo-root Unity project; WebGL-first; no Addressables yet
- Rive package pin may land with M-03 if bootstrap blocks

## Risks / unknowns

- Unity MCP unavailable — create via Editor CLI / file scaffold

## Links

- Acceptance: [`../tests/M-01-acceptance.md`](../tests/M-01-acceptance.md)
- GDD sections: Technical notes
- TAD sections: Architecture, Roadmap constraints
