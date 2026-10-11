# SPIKE findings: agent-system

**Probe question:** which Claude Code skills, hooks, commands, rules, MCP servers and nWave usage let Claude work safely and productively on Natureous during and after the Unity 6.6 (6000.6.5f1) migration, and does each piece exist?

**Verdict:** WORKS on paper, blocked on the editor install. Verified by the spike agent and re-checked 2026-10-11 (Unity CLI version and subcommands, Blender MCP repo rename).

## Key finding

Unity Hub installed the official **Unity CLI** at `~/.unity/bin/unity` (1.0.0-beta.12). It is not on `PATH`, so hooks and skills must use the absolute path. Relevant subcommands:

| Subcommand | Use |
|---|---|
| `recompile` | Recompile a running editor's scripts and report compile errors. Avoids the project lock that breaks batchmode while the editor is open |
| `test [--mode EditMode\|PlayMode]` | Run tests in the editor and write a results report. Replaces hand-written `-runTests` |
| `run` | Batch-mode run with forwarded args (e.g. `-executeMethod`) |
| `command` / `commands` | Execute commands on a connected editor; machine-readable manifest |
| `status` | Live state of connected editors |
| `mcp` | MCP server/client configuration for the editor |
| `skill` | Read the CLI's agent skill or install it into an AI client |
| `install` / `editors` | Install and list editors |

**Blocker:** no editor installed yet (`/Applications/Unity/Hub/Editor` missing). Also absent on this machine: dotnet, Blender, any Unity plugin or MCP, asmdefs, tests.

## Component verdicts

| Component | Verdict |
|---|---|
| nw-* wave skills and agents | EXISTS-VERIFIED |
| `nw-mutation-test` | Exists, but doesn't fit: Stryker.NET needs dotnet and Unity tests run only in the editor. Recommend disabling mutation testing in CLAUDE.md for now |
| superpowers | Not needed; nWave covers TDD and debugging |
| `Unity-Technologies/unity-agent-plugin` | Repo verified (0.1.8-beta, README: "Unity 6+"). NEEDS-EDITOR. Useful skills: `migrate-birp-to-urp`, `validate-urp-render-graph-renderer-feature`, `physics-3d-collision`, `ui-ugui`, `unity-cli` |
| `unity skill install` | Exists, not run. Use this **or** the plugin, not both |
| `nowsprinting/unity-coding-skills` | Repo verified, active. Several skills need Rider MCP; template only |
| CoplayDev/unity-mcp | Verified, v10.3.0 (2026-10-04), supports Unity 2021.3 → 6.x. 6.6 not explicitly tested |
| `unity mcp` (built into CLI) | Exists. Whether it needs a paid Unity AI entitlement: UNVERIFIED |
| Blender MCP | Repo renamed to `ahujasid/mcp-for-blender`. Install: `claude mcp add blender uvx mcp-for-blender` + `uvx mcp-for-blender install-addon`. Deferred |
| Trello connector | EXISTS-VERIFIED |
| Global hooks | `deny-repo-docs.sh` has the Natureous `docs/` exception. Coexistence with `check-test-roi.sh` and DES hooks untested |
| PostToolUse compile hook | NEEDS-EDITOR; timing unmeasured |
| `.claude/rules/` path-scoped rules | UNVERIFIED |

## Draft configuration (not installed)

Project `.claude/settings.json`:

```json
{
  "hooks": {
    "PreToolUse": [
      { "matcher": "Edit|Write|MultiEdit|NotebookEdit",
        "hooks": [{ "type": "command", "command": ".claude/hooks/unity-path-guard.sh", "timeout": 5 }] }
    ],
    "PostToolUse": [
      { "matcher": "Edit|Write|MultiEdit",
        "hooks": [{ "type": "command", "command": ".claude/hooks/unity-recompile.sh", "timeout": 150 }] }
    ]
  },
  "permissions": {
    "deny": ["Edit(Library/**)", "Edit(Temp/**)", "Edit(Logs/**)", "Edit(**/*.meta)", "Bash(git clean*)"]
  }
}
```

`unity-path-guard.sh` (bash + jq, same `permissionDecision` shape as `deny-repo-docs.sh`):
- **deny:** `*.meta`, `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `ProjectVersion.txt`, third-party folders (AppCenter*, GameAnalytics, Smartlook, Joystick Pack, PlayServicesResolver, Plugins, Roundbeargames_Tutorial)
- **ask:** `*.unity`, `*.prefab`, `*.controller`, `*.asset`, `*.anim`, `*.mat`, `ProjectSettings/*`

`unity-recompile.sh`:
- runs only for `.cs` / `.asmdef` edits
- runs only when `unity status` shows an editor connected to this project; never cold-launches the editor per edit
- `unity recompile`; on failure exit 2 and return up to 20 `error CS` lines
- `status --json` output shape: UNVERIFIED

## Proposed project skills

| Skill | Purpose |
|---|---|
| `unity-compile` | `recompile` against the open editor; batchmode `unity run` fallback |
| `unity-test` | `unity test`, parse the results into pass/fail |
| `unity-editor-script-bridge` | All asset changes go through `Assets/Natureous/Editor/Agent/*.cs`, run via `unity command` or `-executeMethod` |
| `animator-statedata-authoring` | StateData subclass → `.asset` → `AnimatorController` API wiring → check `TransitionParameter` enum matches Animator params |
| `unity-profiling` | `ProfilerRecorder` + performance PlayMode test |

## nWave mapping

- **DISCUSS:** Trello cards in; acceptance criteria as observable Animator / `CharacterControl` state.
- **DESIGN:** `VirtualInputManager` is the driving port; Animator, physics and pools are driven.
- **DISTILL:** data invariants → EditMode tests; gameplay criteria → PlayMode `[UnityTest]` that boots `_preload`, sets `VirtualInputManager` flags, waits frames, asserts Animator state. `[Ignore]` replaces `@skip`. No Gherkin runner: scenario names go into test names.
- **DELIVER:** GREEN gate = `unity test --mode PlayMode` with a filter; review includes `.asset` / `.controller` diffs produced by editor scripts.

## Rollout

| # | Step | Needs editor |
|---|---|---|
| 1 | Path-guard hook + CLAUDE.md rules | No |
| 2 | Install 6000.6.5f1; migration Phase B; measure cold open, warm recompile, `unity test` times | Yes |
| 3 | `unity-compile` / `unity-test` skills; recompile hook only if warm recompile ≤ 30 s | Yes |
| 4 | Migration safety net: asmdefs, EditMode integrity tests, PlayMode boot smoke test | Yes |
| 5 | Unity plugin (project scope) | Yes |
| 6 | `unity mcp`, CoplayDev as fallback | Yes |
| 7 | Bridge + animator skills; first feature through all waves | Yes |
| 8 | Profiling, Blender, GameCI | Yes |
