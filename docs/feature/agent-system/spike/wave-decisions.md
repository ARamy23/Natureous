# Wave decisions: agent-system (spike)

**Date:** 2026-10-11 | **Wave:** SPIKE (phase 1 probe) | **Promotion decision: PENDING user**

## Decisions proposed

| ID | Decision | Status |
|---|---|---|
| D1 | Use the official Unity CLI (`~/.unity/bin/unity`, already installed by Hub) as the main agent-to-editor interface: `recompile`, `test`, `command`, `run` | Proposed |
| D2 | Project hooks: PreToolUse path guard (deny meta, Library, third-party; ask for serialized YAML). PostToolUse `unity recompile`, only when a warm editor is connected | Proposed. The PostToolUse part depends on a timing measurement |
| D3 | Scene, prefab and controller changes go only through committed editor scripts under `Assets/Natureous/Editor/Agent/` | Proposed |
| D4 | MCP: `unity mcp` first, CoplayDev/unity-mcp as fallback, one bridge only. Blender MCP deferred | Proposed. Entitlement UNVERIFIED |
| D5 | DISTILL maps gameplay ACs to PlayMode `[UnityTest]` driven through `VirtualInputManager`, and data invariants to EditMode tests | Proposed |
| D6 | Mutation testing disabled for this project (no in-editor mutation tool; Stryker.NET needs dotnet, which is absent) | Proposed |
| D7 | Do not install nowsprinting/unity-coding-skills or superpowers. Use them as templates only | Proposed |

## Promotion options
- **Promote rollout step 1 now** (path-guard hook plus CLAUDE.md rules). It does not need the editor.
- **Hold the rest** until editor 6000.6.5f1 is installed and Phase B of `unity6-migration` has recorded timings.
- **Reject or rescope.**

Decision: _PENDING user_
