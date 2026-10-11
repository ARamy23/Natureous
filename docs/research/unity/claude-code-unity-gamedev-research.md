# Claude Code for Unity Game Development: Research for Natureous

Access date for all sources: 2026-10-11. Status: complete (see Knowledge Gaps).

## Executive Summary
- **Unity 2019.3.5f1 blocks almost all modern agent tooling.** The official Unity MCP and Unity's Claude Code plugin need Unity 6+. Community MCP bridges need 2021.3+ or Unity 6+. ProfilerRecorder needs 2020.2+ and the Performance Testing Extension 2019.4+. On Apple Silicon, 2019.3 runs only under Rosetta.
- **Some things work today with no upgrade**: Claude Code hooks and skills, batchmode `-executeMethod` editor scripts, Unity Test Framework 1.1 `-runTests`, the `AnimatorController` editor API, UnityYAMLMerge, and Blender MCP. Use these to build a test safety net first.
- **Core rule for agents in Unity:** never hand-edit scene, prefab or controller YAML or `.meta` files. Use editor scripts (AssetDatabase/PrefabUtility/AnimatorController APIs) or an MCP bridge. Enforce it with `PreToolUse` hooks.
- **Upgrade target:** Unity 6.3 LTS (supported to Dec 2027). Main breaks: URP 7 → 17 Render Graph, `FindObjectOfType` deprecation (affects `Singleton<T>`), lighting rebakes, mobile SDK updates.

## 1. Unity MCP servers / editor bridges

All four bridges below are verified to exist (fetched 2026-10-11). Each one runs a C# package inside the open Unity Editor and talks to a local MCP server/relay that Claude Code connects to. **None of them lists Unity 2019.3 as supported.** The lowest documented floor is 2021.3 LTS.

| Bridge | What it can do | Install | Min Unity | Maintenance | Source |
|---|---|---|---|---|---|
| **Unity MCP (official, part of `com.unity.ai.assistant`)** | Tools include `Unity_ManageScene`, `Unity_ManageGameObject`, `Unity_ReadConsole`; inspect scenes, read console, edit scripts, trigger Editor actions | Package Manager; Edit > Project Settings > AI > Unity MCP; Claude Code points at the relay binary `~/.unity/relay/relay_mac_arm64.app/Contents/MacOS/relay_mac_arm64 --mcp`; connections have to be approved in Unity | **Unity 6 (6000.0)+**; needs a Unity AI subscription/entitlement | Official, beta/2.x docs | [S1], [S2] |
| **MCP for Unity (CoplayDev/unity-mcp)** | About 50 tool entry points: scenes, GameObjects, C# script editing, assets, **tests**, profiling, builds | UPM git URL `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main` or OpenUPM; Python 3.10+ / uv; "Window > MCP for Unity > Configure All Detected Clients" sets up Claude Code | **2021.3 LTS to 6.x** | Active: v10.3.0 released 2026-10-04 | [S3] |
| **Unity-MCP "AI Game Developer" (IvanMurzak/Unity-MCP)** | 70+ tools: assets (21), scene/hierarchy (24), scripting/editor (16), profiling/diagnostics (12); can generate Claude Code skills | `.unitypackage`, `npm i -g unity-mcp-cli` then `unity-mcp-cli install-plugin <project>`, or `openupm add com.ivanmurzak.unity.mcp`; `unity-mcp-cli setup-skills claude-code <project>` | **Not stated in the README** (unverified, see Gaps) | Active: about 4.4k stars, 3.1k commits | [S4] |
| **MCP Unity (CoderGamester/mcp-unity)** | 30+ tools: `execute_menu_item`, `select_gameobject`, `update_component`, `add_package`, `run_tests`, prefab/material/transform ops; resources for hierarchy, logs, assets | UPM git URL; Tools > MCP Unity > Server Window writes the client config; Node bridge has to match the version (1.5.0 rejects 1.4.x bridges) | **Unity 6+** | Active | [S5] |

**Findings**
- F1.1 The official Unity MCP needs Unity 6 or later. Confidence: High (Unity docs plus Unity blog [S1][S2]).
- F1.2 Every verified community bridge needs at least 2021.3 or Unity 6. Natureous (2019.3.5f1) has to upgrade before it can use any MCP bridge. Confidence: Medium-High (three repo READMEs; IvanMurzak's floor is not documented).
- F1.3 Without a bridge, the agent's only way into Unity on 2019.3 is the CLI: `-batchmode -executeMethod` for custom editor scripts, `-runTests`, and log files (see Section 5). This is an interpretation built from the documented CLI flags [S10].
- Caution: MCP bridges control a live editor and can run arbitrary editor code. Treat their tool output as untrusted input and keep the work under git.

## 2. Claude Code configuration for Unity

Official mechanisms (code.claude.com docs [S6][S7][S8]):

- **CLAUDE.md**: project memory that loads every session. Natureous already has one with good coverage of the toolchain, architecture and the rule against touching third-party code.
- **Skills** (`.claude/skills/<name>/SKILL.md`, frontmatter `name`, `description`, `allowed-tools`, `paths`, `context: fork`, `disable-model-invocation`). They work as slash commands (`/name`) and Claude can also load them automatically. `.claude/commands/<name>.md` still works as the older way to write commands. `` !`cmd` `` injects command output into the prompt. `paths:` can limit a skill to e.g. `Assets/**/*.cs` [S7].
- **Hooks** in `.claude/settings.json` (commit it) or `.claude/settings.local.json`. Events include `PreToolUse` (the only tool event that can block: exit code 2 or `permissionDecision: "deny"`), `PostToolUse`, `Stop`, `SessionStart`. The matcher accepts `Edit|Write`. The `if` field takes permission-rule syntax such as `Edit(node_modules/**)`. `${CLAUDE_PROJECT_DIR}` gives the project path. The default timeout is 600 s [S6].
- **Plugins/marketplaces**: a plugin bundles skills, agents, hooks and MCP servers (`.claude-plugin/plugin.json`). Install via `/plugin`. The official marketplace is `claude-plugins-official`, and anything else is third-party and runs with your privileges [S8]. The IvanMurzak bridge can generate Claude Code skills itself [S4]. No official Anthropic Unity plugin was found (see Gaps).

**Hooks that fit Natureous** (my design, built from the documented mechanisms):
1. `PreToolUse` on `Edit|Write`: deny paths matching `*.meta`, `Library/**`, `Temp/**`, `Logs/**`, `ProjectSettings/ProjectVersion.txt`, and the third-party folders listed in CLAUDE.md. Exit 2 with a reason.
2. `PreToolUse` on `Edit|Write` for `*.unity`, `*.prefab`, `*.controller`, `*.asset`: use `ask`, not `deny`, so hand-edited YAML always gets a human look (see Section 6).
3. `PostToolUse` on `Edit|Write` for `*.cs`: run a batchmode compile check (`Unity -batchmode -quit -nographics -projectPath . -logFile -`) and grep the output for `error CS`. It fails if the editor already has the project open, because Unity locks a project to one instance. That makes it a good fit for `async`/`Stop` hooks, or for a cheaper `dotnet build` against generated `.csproj` files. This is an interpretation and was not verified on 2019.3.
4. A `Stop` hook or `/test` skill that runs `-runTests -testPlatform EditMode -testResults` and summarises the NUnit XML.

Sources: [S6], [S7], [S8], [S10].

**Unity content in plugins (verified)**
- **Unity's official Claude Code plugin**: `/plugin marketplace add Unity-Technologies/unity-agent-plugin` then `/plugin install unity@unity-agent-plugin`. Skills appear under `/unity:`. They cover UI (uGUI/UI Toolkit), 2D/tilemaps, IAP, ads, localization, multiplayer, Shader Graph and Render Graph review. It **requires Unity 6+**. Many skills drive the open editor through the "Unity CLI", which the agent installs itself. Unity's own limitations note says the plugin mostly adds correctness, because models fall back to older approaches without it [S11][S12]. Confidence: High (vendor docs plus repo).
- **nowsprinting/claude-code-settings-for-unity**: a community example of a Unity-oriented `.claude/` with skills `code-writing-guide`, `fix-bug`, `plan-feature`, `run-tests`, `edit-scene`, test design/writing guides and a `test-designer` agent. It was **archived 2026-05-26**, and its skills moved to `nowsprinting/unity-coding-skills`. I did not verify that successor repo directly. Useful as a template, not as a dependency [S13].
- Third-party skill directories (claudemarketplaces.com, skills.cat, vibeindex.ai) list many Unity skills. None of these domains is trusted, so they are left out of the evidence.

## 3. Blender MCP and procedural assets

- **blender-mcp (github.com/ahujasid/blender-mcp)**: verified, about 30.5k stars. Its tools run arbitrary bpy Python, take viewport screenshots from several angles, query the scene, and import from Poly Haven (CC0, no key), Poly Pizza (low-poly, free key), Sketchfab (key), plus AI generation through Hunyuan3D, Tripo and Hyper3D Rodin. It needs Blender 3.0+, Python 3.10+ and uv. Claude Code setup: `claude mcp add blender uvx mcp-for-blender` (the README's current package name; older guides say `blender-mcp`, so check the README when installing). The README warns that `execute_blender_code` runs arbitrary code and offers `BLENDER_MCP_SAFE_MODE=1` [S14]. Confidence: Medium (single source, but it is the primary repo).
- **Works on 2019.3 today.** Blender MCP is independent of the Unity version. Blender is not installed on this machine yet.
- **bpy for stylized/low-poly assets.** An agent can write deterministic, re-runnable Python scripts. The pattern is: primitives, then modifiers (Decimate, Bevel, Solidify), then flat-shaded vertex colours or a small palette texture, then `bpy.ops.export_scene.fbx(...)`. Scripts in git are reviewable and reproducible, while MCP sessions are interactive. This is an interpretation. The Blender FBX manual page could not be retrieved (see Gaps).
- **Export to Unity.** FBX is native in all Unity versions. glTF/GLB needs `com.unity.cloud.gltfast` (supports design-time editor import) [S15]. Its minimum Unity version was not stated on the fetched page. Older glTFast releases supported 2019.x/2020.x according to general knowledge, but that is unverified. For 2019.3, FBX is the safe path. Typical gotchas: Blender is Z-up and Unity is Y-up, scale 0.01 vs 1, and "Apply Transform" (unverified against docs, see Gaps).
- **Round trip.** Blender script → `Assets/Art/Imported/*.fbx` → Unity import settings (via an `AssetPostprocessor` editor script, so the settings are code rather than hand-edited `.meta`). Prefab assembly happens in an editor script run with `-executeMethod` (Section 6).

## 4. Animation via code

- **Authoring AnimatorControllers safely.** `UnityEditor.Animations.AnimatorController` exposes `CreateAnimatorControllerAtPath`, `AddParameter`, `AddLayer`, state machine/state/transition creation and `AddEffectiveStateMachineBehaviour` [S16]. That is the documented way to build or modify a controller. Hand-editing `.controller` YAML is "not supported by Unity" and risks broken fileID references [S17]. **For Natureous:** an editor script can (a) assert that every `TransitionParameter` enum name exists as an Animator parameter, which is a cheap EditMode test, (b) add a state plus `CharacterState` behaviour and assign `StateData` assets, and (c) dump the controller graph to JSON or Markdown so the agent can "see" it. These APIs exist in 2019.3, so it works **today** without MCP. Confidence: High for API existence, and the application to Natureous is my interpretation.
- **Animation Rigging** (IK/Aim/procedural constraints): current 1.3.x requires 2023.2+ [S18]. Earlier 1.0.x versions supported 2019.4/2020 per general knowledge, which is unverified, so treat it as an after-upgrade item.
- **DOTween**: free core plus paid Pro. It currently supports Unity 2020.3 to 6, and older releases go back to Unity 5 [S19]. Good for code-driven UI/camera/prop tweens, not for character locomotion, which belongs to the Animator here.
- **Timeline**: suited to cutscenes. It was not researched in depth (Gap).
- **Mixamo**: the Adobe FAQ returned 403, so licensing and format claims are **unverified**. General knowledge says it gives free rigged FBX animations for Unity ("FBX for Unity" export) and needs a humanoid mesh. Verify the licence terms before shipping.
- **Mobile note**: Generic rigs cost less CPU than Humanoid. Avoid Animators for simple UI motion [S20].

## 5. Performance tooling and headless harnesses

| Tool | Agent-drivable without GUI? | Works on 2019.3? | Source |
|---|---|---|---|
| CLI `-batchmode -nographics -executeMethod -logFile` | Yes | Yes | [S10] |
| `-runTests -testPlatform EditMode/PlayMode -testResults -testFilter -testCategory` (UTF 1.1) | Yes (NUnit XML) | Yes (UTF 1.1.11 is already in the project) | [S21] |
| Profiler CLI `-profiler-enable -profiler-log-file x.raw -profiler-capture-frame-count -deepprofiling` | It writes a `.raw`, but reading `.raw` needs the Profiler window. Agent-readable only through custom exporters | Yes | [S10] |
| `ProfilerRecorder` API (read markers/counters such as GC Reserved Memory and main-thread time in code, including release players) | Yes, log to file/console | **No, introduced in 2020.2** | [S22] |
| Performance Testing Extension `com.unity.test-framework.performance` (`[Performance]`, `Measure.*`, profiler-marker sampling) | Yes, through test runs | 2.8.1-preview for 2019.4; 3.x for 2020.3+. **2019.3 is not listed** | [S23] |
| Memory Profiler package | GUI for analysis | 0.7.x supports 2019.3.0f1+; 1.x needs 2022.2+ | [S24] |
| Frame Debugger | GUI only | Yes | not researched in depth |
| GameCI `unity-test-runner` / `unity-builder` (GitHub Actions, Docker) | Yes, in CI | Docs reference 2020.1.14f1+; 2019.3 image availability not verified. Needs a Unity licence as secrets (`UNITY_LICENSE`/`UNITY_EMAIL`/`UNITY_PASSWORD`) | [S25][S26] |

**Mobile guidance** (Unity official [S20]): stay within frame budgets (16.66 ms at 60 fps; Natureous targets 60), enable the SRP Batcher, use static batching or GPU instancing, prefer baked lighting and limit real-time lights and shadows, prefer simpler or unlit shaders on low-end devices, limit cameras and full-screen post effects. Zero per-frame GC allocations in the core loop is a widely repeated target (dev.to source, medium trust, single source, so Low confidence alone, but consistent with Unity guidance).
**Natureous-specific hotspot** (from CLAUDE.md): `Singleton<T>.Instance` calls `FindObjectOfType` on every access. That is a CPU cost to cache away, and Unity 6 deprecates `FindObjectOfType` [S27]. A ProfilerRecorder/perf test around a busy combat scene would quantify the cost once upgraded.

**Interpretation, the agent harness on 2019.3:** EditMode tests plus `-executeMethod` scripts that write JSON reports (scene hierarchy dumps, Animator parameter validation, draw-call or stat counts gathered in PlayMode tests) are the main signal the agent can read. Note that the project cannot be open in the editor while a batchmode run uses it.

## 6. How LLM agents work on game projects

**What fails**
- Hand-editing scene, prefab or asset YAML. Unity says manual editing "is not supported", can corrupt files with unhelpful errors, breaks fileID references, and loses data when prefab hierarchies change. It recommends AssetDatabase APIs and Smart Merge instead [S17]. Community guides for Claude Code + Unity agree that `.meta`/YAML edits are the most common way agents damage projects (thepromptshelf.dev and devinreimer.substack.com, untrusted domains, consistent with [S17]).
- GUID/`.meta` integrity: GUIDs in `.meta` link assets project-wide. Deleting or regenerating a `.meta` breaks every reference [S17]. CLAUDE.md already bans this. A hook should enforce it (Section 2).
- Binary assets (textures, FBX, audio) and anything visual: the agent cannot see the game unless something gives it screenshots (MCP screenshot tools, Blender MCP viewport captures [S14], or `ScreenCapture.CaptureScreenshot` in a PlayMode test).
- Merge conflicts in YAML: configure UnityYAMLMerge (`/Applications/Unity/Hub/Editor/<ver>/Unity.app/Contents/Tools/UnityYAMLMerge` or similar path) as the git mergetool [S28].

**What works**
1. **Editor scripts as the agent's interface.** The agent writes C# under `Assets/Editor/` (or a temporary `Editor/AgentScripts/` folder) that uses `AssetDatabase`, `PrefabUtility` and `AnimatorController`, and runs it via `-executeMethod`. It's deterministic, reviewable and version-independent [S10][S16][S17].
2. **MCP bridge** (after the upgrade) for live hierarchy reads, console logs and test runs [S1][S3].
3. **Tests as eyes**: EditMode tests for data integrity (enum ↔ Animator params, every `CharacterState` has non-null `StateData`, pool prefabs exist in `Resources`), and PlayMode tests that load `_preload`, simulate `VirtualInputManager` flags and assert state transitions [S21].
4. **Screenshots for visual checks**, reviewed by a human or a multimodal model. This is an interpretation.
5. **Project setting**: Asset Serialization = Force Text (needed for diffs/merges), Visible Meta Files. Check `ProjectSettings/EditorSettings.asset`.

## 7. Unity version and upgrade path

- **2019.3 is a non-LTS tech stream** that predates 2019.4 LTS. Apple Silicon fixes were backported only to 2019.4.18f1+. On an Apple Silicon Mac, 2019.3 runs only as the Intel build under Rosetta 2, which is slower. Unity community posts note that macOS 27 is the last macOS with Rosetta 2 [S29]. Unity 6.0 LTS is supported through October 2026, which is the current month, and **Unity 6.3 LTS is supported until December 2027** [S30].
- **Tooling blocked by staying on 2019.3:** every MCP bridge (2021.3+ or 6+), Unity's Claude Code plugin (6+), ProfilerRecorder (2020.2+), the Performance Testing Extension (2019.4+), Memory Profiler 1.x (2022.2+), current Animation Rigging (2023.2+), current DOTween (2020.3+) [S1][S3][S5][S11][S18][S19][S22][S23][S24].
- **What breaks going to Unity 6.x / URP 17:**
  - URP 7.1.8 → 17 brings Render Graph, on by default. Custom `ScriptableRendererFeature`s must be rewritten, or temporarily kept running with "Compatibility Mode (Render Graph Disabled)", which Unity no longer develops [S31].
  - `FindObjectOfType`/`FindObjectsOfType` are deprecated in favour of `FindFirstObjectByType`/`FindAnyObjectByType`/`FindObjectsByType`, which hits `Singleton<T>` directly [S27].
  - Lighting: Enlighten baked GI removed, lighting data must be rebaked, light probe brightness changed [S27].
  - Android toolchain: Gradle/AGP/JDK 17 updates [S27].
  - Third-party SDKs (GameAnalytics, Smartlook, AppCenter, Play Services Resolver) are likely to need newer versions. Microsoft retired App Center in 2025 per general knowledge, which is unverified here (Gap).
- **Path (interpretation):** branch, back up, open in Unity 6.3 LTS directly (Unity re-imports and runs API Updater). An intermediate 2022.3 hop is optional and was not found as an official requirement (Gap). Fix compile errors, upgrade URP assets and materials (Render Pipeline Converter), replace or remove dead SDKs, then verify with the EditMode/PlayMode tests written beforehand on 2019.3.

**Recommendation:** writing tests **before** upgrading gives the upgrade a safety net, and UTF 1.1 already works on 2019.3.

## Knowledge Gaps
- IvanMurzak/Unity-MCP minimum Unity version: the README doesn't state it and package.json returned 404. Check `Packages/manifest` compatibility on install.
- `nowsprinting/unity-coding-skills`: referenced by an archived repo but not fetched directly.
- Blender FBX export manual page: the fetched page returned only navigation, so the axis/scale settings come from general knowledge.
- Mixamo licensing: Adobe FAQ returned 403.
- glTFast minimum Unity version, GameCI Docker images for 2019.3, Animation Rigging 1.0 on 2019.x, and whether an intermediate 2022.3 hop is officially recommended: none of these found on trusted pages.
- Status of the App Center SDK (believed retired): not verified this session.
- Timeline and Frame Debugger automation: not researched in depth.
- Whether the batchmode compile check runs acceptably under Rosetta on this Mac: needs an empirical test once the editor is installed.

## Conflicting Information
- Two install names exist for blender-mcp (`blender-mcp` in older guides, `mcp-for-blender` in the current README). Trust the current README [S14].
- Unity's plugin docs say the plugin "doesn't use MCP" but drives the editor through a Unity CLI, while Unity MCP is a separate feature of `com.unity.ai.assistant` [S1][S12]. These are two different official integrations, and both need Unity 6.

## Recommended setup for Natureous

### Do now (on 2019.3.5f1)
1. **Install the editor** 2019.3.5f1 (Intel, Rosetta) via Unity Hub so CLI commands in CLAUDE.md actually run. Otherwise no harness works. [S10][S29]
2. **Add `.claude/settings.json` hooks**: a `PreToolUse` deny on `*.meta`, `Library/`, `Temp/`, `Logs/`, `ProjectVersion.txt` and third-party folders; `ask` on `*.unity|*.prefab|*.controller|*.asset`. [S6][S17]
3. **Add asmdefs plus a `Tests/EditMode` assembly**, then write integrity tests: `TransitionParameter` ↔ Animator params, no null `StateData` in `CharacterState`s, `PoolObjectType` ↔ `Resources` prefabs. [S16][S21]
4. **Create skills**: `/unity-test` (runs `-runTests`, parses XML), `/unity-compile` (batchmode log grep for `error CS`), `/animator-dump` (runs an `-executeMethod` editor script that exports controller graphs to JSON). Use `paths:` to scope them. [S7][S10][S16]
5. **Establish the "editor script as interface" rule** in CLAUDE.md: scene, prefab and controller changes go through `Assets/Editor/Agent/*.cs` + `-executeMethod`, never YAML edits. [S17]
6. **Configure UnityYAMLMerge** as the git mergetool and confirm Force Text serialization. [S28]
7. **Optional, for assets**: install Blender plus blender-mcp. Keep generation as committed bpy scripts that export FBX. [S14]
8. **Add a PlayMode smoke test** that boots `_preload` and captures a screenshot as visual evidence. [S21]

### After the Unity upgrade (target Unity 6.3 LTS, supported to Dec 2027)
1. Upgrade on a branch using the tests above as the safety net. Convert URP 7 to 17 (Compatibility Mode temporarily if needed), replace `FindObjectOfType` in `Singleton<T>`, update or remove SDKs. [S27][S30][S31]
2. Install **Unity's Claude Code plugin** (`unity@unity-agent-plugin`). [S11][S12]
3. Install **one** MCP bridge: official Unity MCP if you have the AI entitlement [S1][S2], otherwise CoplayDev MCP for Unity (most active, runs tests) [S3].
4. Add `com.unity.test-framework.performance` plus `ProfilerRecorder` perf tests for a combat scene (frame time, GC alloc, draw calls). [S22][S23]
5. Memory Profiler 1.x for mobile memory audits. [S24]

### Later
1. GameCI GitHub Actions (`unity-test-runner`, then `unity-builder` for Android/iOS) with licence secrets. [S25][S26]
2. Animation Rigging for IK/aim polish, DOTween for UI/camera juice. [S18][S19]
3. A mobile perf pass per Unity guidance (SRP Batcher, static batching, baked lights, Generic rigs). [S20]
4. Consider an AI 3D generation provider (Hunyuan3D/Tripo/Rodin via blender-mcp) only after an art-direction check, since stylistic consistency is the risk. [S14]

## Sources
All accessed 2026-10-11. Tier: H = high (gamedev_tooling/official), MH = github.com (medium-high).
- [S1] H https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.0/manual/unity-mcp-get-started.html
- [S2] H https://unity.com/blog/unity-ai-mcp-how-to-get-started
- [S3] MH https://github.com/CoplayDev/unity-mcp
- [S4] MH https://github.com/IvanMurzak/Unity-MCP
- [S5] MH https://github.com/CoderGamester/mcp-unity
- [S6] H https://code.claude.com/docs/en/hooks
- [S7] H https://code.claude.com/docs/en/skills
- [S8] H https://code.claude.com/docs/en/plugins
- [S10] H https://docs.unity3d.com/2019.3/Documentation/Manual/CommandLineArguments.html
- [S11] H https://docs.unity.com/en-us/ai/unity-plugin/claude-code.md
- [S12] MH https://github.com/Unity-Technologies/unity-agent-plugin
- [S13] MH https://github.com/nowsprinting/claude-code-settings-for-unity
- [S14] MH https://github.com/ahujasid/blender-mcp
- [S15] H https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.0/manual/index.html
- [S16] H https://docs.unity3d.com/ScriptReference/Animations.AnimatorController.html
- [S17] H https://unity.com/blog/engine-platform/understanding-unitys-serialization-language-yaml
- [S18] H https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/index.html
- [S19] (vendor, not on trusted list; single primary source) https://dotween.demigiant.com/
- [S20] H https://unity.com/how-to/mobile-game-optimization-tips-part-2
- [S21] H https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/reference-command-line.html
- [S22] H https://docs.unity3d.com/2020.2/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.html
- [S23] H https://docs.unity3d.com/Packages/com.unity.test-framework.performance@3.0/manual/index.html
- [S24] H https://docs.unity3d.com/Packages/com.unity.memoryprofiler@1.1/manual/memory-profiler-install.html
- [S25] (game.ci, not on list; official GameCI docs) https://game.ci/docs/github/test-runner
- [S26] MH https://github.com/game-ci/unity-builder
- [S27] H https://docs.unity3d.com/6000.0/Documentation/Manual/UpgradeGuideUnity6.html
- [S28] H https://docs.unity3d.com/Manual/SmartMerge.html
- [S29] H https://discussions.unity.com/t/apple-silicon-support-for-2019-lts/797156 and https://discussions.unity.com/t/announcing-the-transition-of-apple-silicon-unity-editor-from-rosetta-2/1717433
- [S30] H https://unity.com/releases/unity-6/support
- [S31] H https://docs.unity3d.com/6000.2/Documentation/Manual/urp/compatibility-mode.html
- Untrusted/context only (not counted as evidence): thepromptshelf.dev, devinreimer.substack.com, dev.to/oceanviewgames, claudemarketplaces.com.
