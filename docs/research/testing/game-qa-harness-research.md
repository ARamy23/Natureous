# Research: A test harness for every part of Natureous (automated QA for agentic coding)

Date: 2026-10-11 · Researcher: Nova (nw-researcher) · Depth: comprehensive · Status: desk research; nothing here has run on Unity 6000.6.5f1 yet (editor not installed, see `docs/feature/agent-system/spike/findings.md`).

## Executive summary

1. **The goal is a fast feedback loop for the agent, not "QA automation" in general.** Unity's own CLI announcement puts it this way: an assistant can "open the project, apply a change, run the tests, enter Play mode to check behavior, read the result, and decide what to do next" [S1]. Every layer below exists to give Claude a pass/fail signal it can parse, at the lowest cost that still catches the bug.
2. **Most of the value comes from plain C# cores tested in EditMode.** Move data, combo and cancel windows, the damage pipeline, the state machine's transition table, health, checkpoints and save data can all be pure C# behind thin MonoBehaviours (the humble-object pattern). These tests run in milliseconds to seconds and never flake. This is the practitioner consensus on what is worth automating in games [S20, S21] (medium confidence; see Gaps).
3. **PlayMode tests cover the integration seams.** That means physics casts, the Animator receiving hashed IDs, NavMesh and scene boot. They need frame and physics control: `WaitForFixedUpdate`, a fixed `Time.fixedDeltaTime`, and optionally `Physics.simulationMode = Script` with `Physics.Simulate(fixedStep)` for deterministic stepping [S5, S6].
4. **Input should be driven through the Input System test fixture or a virtual input port.** `InputTestFixture` provides `Press`, `Release`, `Set` and `Trigger` on fake devices [S7]. `InputEventTrace` can record and replay real play sessions as regression artefacts [S8].
5. **Visual checks have three tiers.** (a) Unity's Graphics Test Framework (`com.unity.testframework.graphics`, `ImageAssert.AreEqual`, reference images per ColorSpace/Platform/GraphicsAPI) [S9, S10]. (b) Cheap screenshot captures the agent looks at. (c) Optionally a separate vision model that sees only screenshots and never the code, so the coding agent can't grade its own work (Godogen's design, from HN) [S17]. Community reports say LLMs are weak at spatial and real-time judgement [S18, S19], so screenshots should be a supplementary check, not the main oracle.
6. **Performance gets its own tests.** The Performance Testing Extension (`com.unity.test-framework.performance`: `Measure.Frames`, `Measure.Method`, ProfilerMarkers including `GC.Alloc`) and `ProfilerRecorder` [S11, S12] can enforce frame-time and zero-allocation budgets. Treat editor numbers as trends only. Real budgets (60 fps on mid-range phones) need device runs: Firebase Test Lab Game Loop [S14] or a local device.
7. **Several tools are not worth adopting.** Unity's Automated QA package was put on hold in Dec 2021 and is unsupported [S13]. AltTester (Unity SDK 2.3.1) [S15] and GameDriver (commercial, free for solo developers) [S16] are for black-box tests against built players and are overkill until Natureous has device builds to protect. GameCI (`game-ci/unity-test-runner`, MIT) is the standard way to run headless CI, but needs Unity license activation [S22, S23].
8. **Agent-specific tooling now exists.** The official Unity CLI (`unity test`, `recompile`, `command eval` as a live C# REPL, `[CliCommand]` custom commands) [S1]. Community bridges such as `hatayama/unity-cli-loop` v3 (run tests, logs, Game View screenshot, video record, simulate keyboard and mouse, replay recorded input, control play mode) [S24], plus several Unity MCP servers [S25].
9. **What to leave manual:** game feel (hit-stop, combo timing feel, camera), art direction and fun. Every source agrees [S20, S21, S18].

Confidence overall: **Medium-High** for the Unity APIs (official docs). **Medium** for the community and practitioner claims. Reddit could not be fetched (see Gaps), so community evidence comes from Hacker News and practitioner blogs instead.

---

## 1. Unity Test Framework: EditMode vs PlayMode

**Evidence**
- The Unity Test Framework (UTF) is NUnit-based. `[UnityTest]` runs as a coroutine in PlayMode, and all coroutine yield instructions are available in tests, including `WaitForFixedUpdate` "to ensure changes expected within the next cycle of physics calculations". UTF also adds `EnterPlayMode`, `ExitPlayMode` and `WaitForDomainReload` [S5]. From Unity 6.2 onward the UTF docs live in the core Unity Manual, which means UTF ships as a core package (UTF 1.5.x) [S26].
- Tests live in assemblies defined by `.asmdef` files that reference the test assemblies. Graphics and Performance tests also need their own assembly references (`UnityEngine.TestTools.Graphics`, `Unity.PerformanceTesting`) [S9, S11].
- The Unity CLI runs `unity test --mode EditMode|PlayMode` against the open editor and writes a results report (verified in the local agent-system spike).

**Patterns that work for Natureous** (my analysis, based on the sources above and standard humble-object practice)

| Pattern | How | Why |
|---|---|---|
| Pure C# core + humble MonoBehaviour | `CombatStateMachine`, `MoveData` evaluation, `DamageResolver`, `Health` are plain classes. The MonoBehaviour only forwards `Update`/`FixedUpdate`, input and physics query results | EditMode tests take milliseconds and don't depend on scenes, which keeps them free of flakiness |
| Injected time | Core takes `deltaTime` as a parameter, never reads `Time.time` | Tests can step exact windows (startup/active/recovery) |
| Ports for engine queries | `IGroundProbe`, `IHitQuery` interfaces; fakes in EditMode, real `BoxCast`/`OverlapCapsule` in PlayMode | Lets combat and movement logic be tested without physics |
| Test-only scenes | Minimal `Tests/Scenes/Arena_Flat.unity`, `Arena_Ledge.unity` loaded via `SceneManager.LoadSceneAsync` | Keeps PlayMode tests isolated from production scenes |
| asmdef split | `Natureous.Runtime`, `Natureous.Editor`, `Natureous.Tests.EditMode`, `Natureous.Tests.PlayMode` | Recompiles faster. EditMode tests can't reach play-only code by accident |

**Time control**
- `Time.timeScale` speeds up soak tests, but large timescales make each fixed step see a larger slice of time per frame. Prefer stepping physics deterministically (§2).
- Pin `Time.fixedDeltaTime` (e.g. 1/50 or 1/60) in `[SetUp]`, and assert in fixed-step counts rather than wall-clock seconds.

Confidence: High for the APIs (official docs). The patterns are an interpretation.

## 2. Deterministic physics in tests

**Evidence**
- With `Physics.simulationMode = SimulationMode.Script`, you call `Physics.Simulate(step)` manually, and each call runs collision detection, integration and callbacks. For deterministic results, pass a fixed step every time. Steps above 0.03 are likely to be inaccurate [S6]. The same API is documented for 6000.6 [S6].

**Use**
- For movement and ledge tests, set Script mode in `[UnitySetUp]`, then loop `Physics.Simulate(1/60f)` N times and assert the position, grounded state and ledge-grab state. This doesn't wait on real frames, so it's fast and repeatable on the same machine. Restore the mode in teardown.
- Caveat (interpretation): PhysX is deterministic for the same inputs on the same build and platform. It isn't guaranteed across platforms or CPUs, so position assertions need tolerances (`Assert.That(x, Is.EqualTo(e).Within(0.01f))`).

Confidence: High (official docs, single authoritative source).

## 3. Input simulation and replay

**Evidence**
- `InputTestFixture` (Input System package) gives tests isolated fake devices: `InputSystem.AddDevice<Gamepad>()`, `Press(gamepad.buttonSouth)`, `Release`, `PressAndRelease`, `Set(control, value)`, `Trigger(action)`. Custom layouts registered at startup are *not* present inside the fixture and must be registered in the test setup [S7].
- `InputEventTrace` captures input events into an unmanaged buffer and supports WriteTo/ReadFrom/Replay. It must be `Dispose()`d to avoid native leaks [S8].
- `unity-cli-loop` exposes keyboard and mouse simulation plus "replay input recorded in the Recordings window" to agents [S24].

**Recommendation for Natureous**
- The legacy codebase drives the character through `VirtualInputManager` flags (agent-system spike). Keep a **virtual input port** (`IPlayerIntent`: move axis, jump, attack, block) as the seam between devices and gameplay. Most gameplay tests set the intent directly. A few tests use `InputTestFixture` to prove the action map binds to the port (keyboard, gamepad, on-screen joystick).
- Golden replays: record short `InputEventTrace` sessions (e.g. "3-hit combo then block", "ledge climb") and replay them in PlayMode with assertions on the end state and events. They are brittle when move data changes, so re-record on purpose and version them alongside the move assets.

Confidence: High (APIs). Medium for replay brittleness (interpretation plus HN reports about real-time state [S19]).

## 4. Visual regression and screenshots for an agent

**Evidence**
- Graphics Test Framework (`com.unity.testframework.graphics`): `ImageAssert.AreEqual()` compares images or a camera's output with configurable per-pixel and whole-image thresholds. Test cases can be generated from scenes (`[UseGraphicsTestCases]` plus `IPrebuildSetup`). Reference images are stored per `ColorSpace/Platform/GraphicsAPI`, with a platform-agnostic `ReferenceImagesBase` fallback. Android and WebGL need asynchronous reference loading [S9]. The changelog is active (8.x line) [S10]. It is Unity's internal graphics QA tool and not heavily promoted for games. Compatibility with 6000.6 is **unverified**.
- Agent-side screenshots: `unity-cli-loop` provides Game View screenshots and MP4 recording [S24]. The Unity CLI's `command eval` can call `ScreenCapture.CaptureScreenshot` [S1] (interpretation of the documented eval capability).
- HN practitioners report that coding agents "can't see" output: code compiles but assets float [S17]. Godogen uses a separate vision model that sees only screenshots, compared against a reference image, so the coding agent can't confirm its own mistakes [S17]. Another practitioner found text renderings of game state more reliable and cheaper than screenshots. Physics-heavy games were reported as "nearly impossible" to test through screenshots because too much changes between captures [S18]. A separate thread lists weak spatial and temporal reasoning as the core LLM limits [S19]. *(Community opinion; HN.)*

**Recommendation**
1. Deterministic visual tests use `ImageAssert` on frozen test scenes (fixed camera, no particles, a fixed animation frame). Use them for URP tier settings, shaders and the UI layout at 2–3 aspect ratios.
2. The agent's "look" step captures screenshots at fixed points (after boot, mid-combo, after a ledge climb), stored in `Temp/agent-shots/`, then (a) a text "state dump" alongside (player state, HP, position, active move), which the agent should trust more than the pixels, and (b) an optional vision review by a separate prompt with no code context.
3. Never let a screenshot verdict be the only gate for gameplay behaviour.

Confidence: Medium (official package docs plus community reports).

## 5. Performance testing

**Evidence**
- The Performance Testing Extension measures profiler markers and custom metrics in the editor and in players, and records hardware and config metadata. The docs list 3.0.3 as current, with stated support up to 2023.2 [S11]. `Measure.Frames()` supports `WarmupCount`, `MeasurementCount` and `ProfilerMarkers(...)`. `Measure.Method` can sample `GC.Alloc` markers [S12].
- `ProfilerRecorder` reads counters such as `ProfilerCategory.Memory "GC.Alloc"` or main-thread time, either immediately or at frame end [S12].
- Firebase Test Lab Game Loop launches the game with a scenario number. The game runs scripted or AI play (e.g. two minutes in the heaviest level), logs frame times and memory, then calls finish. This works on real Android devices, and an iOS variant exists [S14].

**Recommendation**
- In the editor, PlayMode perf tests assert **0 B GC.Alloc per frame** in steady-state combat (a stable signal) and track frame-time *trends*, but don't fail on absolute ms (editor overhead dominates).
- On device, a development build has a `-scenario soak_arena` entry point that plays a scripted bot for 120 s and writes frame-time p50/p95/p99 and memory to JSON. Gate on p95 ≤ 16.6 ms on the reference phone. Run it locally first; Firebase Test Lab later.
- Compatibility of the performance package with Unity 6 still needs checking. The 3.0 docs page lists support only up to 2023.2 [S11].

Confidence: High for the APIs, Medium for 6.6 compatibility.

## 6. Automated playtesting, bots, smoke and soak tests

**Evidence**
- EA SEED research: agents exploring a stripped-down re-implementation of the game's mechanics can play in minutes what would take testers days [S27]. Curiosity-driven RL agents improve coverage [S28]. Recent arXiv work uses LLM agents for game testing [S29, S30].
- An HN practitioner found an LLM playing the game good at **validating a feature against its spec** and poor at finding emergent bugs. It cost about 300K tokens per feature validation. Commenters suggested using the LLM to *write* bot policies, or Monte Carlo headless runs whose aggregated reports go to the LLM [S18]. *(Community opinion.)*

**Recommendation (interpretation)**
- **Boot smoke test** (the first PlayMode test): load `_preload` and then the first level. Assert no errors or exceptions in the log (`LogAssert.NoUnexpectedReceived()`), a player exists, the camera is active, and input is accepted within N seconds.
- **Scripted bots**: a small `BotPolicy` that writes to `IPlayerIntent` (walk right, attack on range, block on enemy windup). Combined with `Time.timeScale` 2–4×, this gives a soak run that checks for no NaN positions, no stuck states (state unchanged > X s while input is active), no fall-through-world, and pooled-object counts staying bounded.
- **Monkey/fuzz**: a random intent stream with a fixed seed. Assert the invariants above and log the seed on failure for a reproducible run.
- ML-Agents is out of scope now. The cost of training is high for a solo project, and scripted bots plus invariants cover the risk.

Confidence: Medium.

## 7. Black-box and device tooling: status check

| Tool | Status (verified 2026-10-11) | Fit for Natureous now |
|---|---|---|
| Unity Automated QA (`com.unity.automated-testing`) | Development on hold since 2021-12-06. Unsupported, won't be updated for new Unity versions [S13] | **Avoid** |
| AltTester Unity SDK | Active. v2.3.1 adds native pop-up support for Appium/Selenium on Android/iOS/WebGL. Pricing changed July 2026 [S15] | Later, for device UI flows |
| GameDriver | Commercial. Test Assistant standalone (2025.01). Free for solo developers and educators, from $150/mo for teams [S16] | Optional alternative to AltTester |
| Appium | Can't see Unity UI natively, so use it with AltTester [S15]. Firebase notes generic automation tools can't inspect engine-rendered UI [S14] | Only via AltTester |
| Firebase Test Lab Game Loop | Official Google feature for Android, iOS variant exists [S14] | Device perf and soak, phase 2 |
| GameCI `unity-test-runner` | Open source, MIT, active, 267 stars. Needs Unity license activation. Unity dropped manual Personal-license activation, which affected workflows [S22, S23] | CI once tests exist. Unity 6.6 image availability unverified |
| Unity CLI (`~/.unity/bin/unity`) | Official, beta. `test`, `recompile`, `command eval`, `[CliCommand]` [S1] | **Primary local agent runner** |
| `hatayama/unity-cli-loop` | Active, v3.15.1 (latest release, checked 2026-10-11), Unity 2022.3+ [S24] | Strong candidate for screenshots, input sim and replay |
| Unity MCP servers (CoplayDev, CoderGamester, IvanMurzak…) | Many active. Expose test list/run/results, scene and prefab ops [S25] | Fallback. Pick one bridge |

## 8. The agentic-coding angle

**Evidence (community opinion unless noted)**
- Official: the Unity CLI exists so agents get structured JSON/TSV output with predictable exit codes and a live C# REPL, rather than scraped console text [S1].
- HN: agents can't see output; separating the "judge" model from the "coder" model reduces self-confirmation [S17]. LLMs struggle with "hold this button for a few frames until x happens" (temporal control) and with spatial reasoning [S19]. Feature-vs-spec validation works best, while undirected bug hunting works poorly [S18]. Combining source analysis with live state snapshots doubled test quality for one practitioner [S18].
- Practitioner blogs: automate logic and critical paths, and leave feel and fun to humans [S20, S21].
- Known Unity-specific failure modes (from the project's own spike plus MCP docs [S25]): hand-editing scene/prefab YAML and `.meta` GUIDs, PlayMode flakiness from frame timing, and slow domain reloads. Mitigation is already drafted in `agent-system/spike/findings.md`: a path guard, and changes to assets only through editor scripts.

**Harness design for an agent (interpretation)**
- **Test-as-spec.** Every DISTILL acceptance criterion becomes a named test (`Combo_ThirdHit_CancelsIntoBlock_WithinCancelWindow`). The agent's job is "make these red tests green without editing them". A hook should deny edits to test files during DELIVER unless asked.
- **Machine-readable oracles.** Prefer state assertions and event logs (`DamageApplied{attacker,target,amount,frame}`) over pixels.
- **A state dump command.** A `[CliCommand] natureous.dump` prints player and enemy states as JSON. This is the agent's "eyes" between tests.
- **Layering.** Run the cheapest check first and stop at the first failure (see blueprint).

## 9. Test pyramid for games and flakiness control

- Base: EditMode unit tests on pure cores (many). Middle: PlayMode integration in tiny test scenes (dozens). Top: boot smoke, golden replay, soak bot and visual (a handful). Device and manual playtests sit above automation.
- Flakiness rules (interpretation, consistent with [S5, S6]): never assert on wall-clock time. Use fixed steps or Script physics. Use seeded RNG. Reset the scene per test (load an additive test scene and unload it). Use `LogAssert` to catch hidden errors. Quarantine flaky tests with `[Ignore("flaky: #id")]` instead of retry loops. Retries hide real timing bugs in combat windows.
- Keep manual: hit feel, camera comfort, difficulty, readability and animation quality. Write a short manual checklist per build.

---

## Harness blueprint for Natureous

### Per game part

| Part | What to test | Type | Tool | Example assertion | Agent-runnable? | Needs editor? |
|---|---|---|---|---|---|---|
| Input | Action map → `IPlayerIntent`; touch joystick dead-zone | PlayMode (fixture) | `InputTestFixture` | `Press(gamepad.buttonWest)` → `intent.Attack == true` that frame | Yes | Yes |
| Movement/physics | Grounded via BoxCast, slope, wall stop, ledge grab/climb, plane lock | PlayMode, Script physics | UTF + `Physics.Simulate` | After 60 steps of jump at ledge: `state == LedgeHang`, `z == 0 ±0.01` | Yes | Yes |
| Combat/damage | Move windows, cancels, per-swing hit set, block reduces damage, i-frames | EditMode (pure) + 1 PlayMode | UTF | Swing hits same target once; blocked hit → `hp -= 0`, `HitReaction=BlockStun` | Yes | EditMode via CLI (editor running) |
| Animation state | State machine calls the right clip hash; Animator params exist | EditMode (asset integrity) + PlayMode | UTF, `AnimatorController` API | Every `MoveData.clipHash` resolves to a state in the controller | Yes | Yes |
| AI/navigation | Enemy reaches player; leaves plane on knockback, returns; no stuck | PlayMode | UTF + NavMesh test scene | Within 300 steps `distance < attackRange`; `stuckSeconds < 2` | Yes | Yes |
| Level flow/checkpoints | Touch checkpoint → respawn there with full HP; death → respawn | EditMode (logic) + PlayMode | UTF | After death: `player.pos ≈ checkpoint.pos`, `hp == max` | Yes | Yes |
| UI | HUD reflects HP; pause works; layout on 3 aspect ratios | PlayMode + visual | UTF, `ImageAssert` | HP bar fill == hp/max; screenshot diff < threshold | Yes (diff); look = advisory | Yes |
| Rendering/visual | URP tier settings, shaders compile, key scenes render | Visual regression | Graphics Test Framework | `ImageAssert.AreEqual(ref, cam, settings)` | Yes | Yes (GPU) |
| Performance | 0 GC/frame in combat; frame-time trend; device p95 | PlayMode perf + device soak | Perf Testing Ext, `ProfilerRecorder`, Firebase Game Loop | `GC.Alloc == 0` over 300 frames; device p95 ≤ 16.6 ms | Editor: yes. Device: partial | Editor yes; device build |
| Save/analytics | Save round-trip, version migration; analytics events fired with schema | EditMode | UTF + fake analytics sink | `Load(Save(s)) == s`; `events.Contains("checkpoint_reached")` | Yes | EditMode (could be plain .NET) |
| Build/platform | Project compiles for Android/iOS/desktop; boot smoke in player | Batch build + smoke | `unity run -executeMethod`, GameCI | Build exit 0; player log has "BOOT_OK" within 20 s | Yes (slow) | Batchmode |

### Layered agent feedback loop (stop at first failure)

| Layer | Command | Target time (to measure; none measured yet) |
|---|---|---|
| L0 Compile | `unity recompile` (hook after `.cs` edits) | ≤ 30 s warm |
| L1 EditMode | `unity test --mode EditMode` (filtered to touched area, then all) | ≤ 20 s |
| L2 PlayMode smoke | `unity test --mode PlayMode --filter Smoke` | ≤ 60 s |
| L3 PlayMode feature | filter by category of the feature (Combat, Movement…) | ≤ 3 min |
| L4 Look | state-dump JSON + 3 screenshots; optional separate vision review | ≤ 1 min |
| L5 Perf/visual | perf + `ImageAssert` suites | ≤ 5 min, pre-merge only |
| L6 Device/soak | dev build + soak scenario on phone/Firebase | nightly / manual |

### First 5 tests to write (prioritized)

1. **PlayMode boot smoke**: `_preload` → first level, no unexpected logs, player spawned, input accepted. This is the migration safety net.
2. **EditMode asset integrity**: every MoveData, enemy and checkpoint asset is valid (no missing refs, windows ordered startup < active < recovery, clip hashes exist in the controller).
3. **EditMode damage pipeline**: `DamageInfo` → `IDamageable` → block → health → reaction, including the per-swing hit set (covers bugs A1–A3 from 06-consult).
4. **PlayMode grounded/ledge** in a flat-plus-ledge test scene with Script physics stepping.
5. **PlayMode combat GC test**: 300 frames of a scripted combo against a dummy, asserting 0 GC.Alloc and that the dummy took the expected damage.

### Open questions for the owner

1. Is a CI runner (GitHub Actions plus GameCI) wanted now, or are local agent runs enough? CI needs a Unity license secret, and Personal licenses need the newer activation flow.
2. Which reference phone defines the 60 fps budget, and is it physically available for device runs?
3. Allow a second model (vision-only) as visual QA, or keep screenshots for human review only?
4. Should test files be write-protected for the agent during DELIVER (test-as-spec enforcement)?
5. Adopt `unity-cli-loop` for screenshots and input replay, or stick to the official CLI plus custom `[CliCommand]`s?
6. Is analytics staying (GameAnalytics folder exists), and does it need event-schema tests?

---

## Knowledge gaps

- **Reddit (r/Unity3D, r/gamedev, r/ClaudeAI) could not be read.** reddit.com is blocked for this research agent's fetcher, and the redlib mirror returned 403. Community evidence comes from Hacker News [S17–S19] and practitioner blogs [S20, S21]. The owner may want to skim Reddit manually.
- Unity 6000.6 compatibility of `com.unity.testframework.graphics` and `com.unity.test-framework.performance` is unverified. The perf docs list support only up to 2023.2 [S11].
- GameCI Docker images for 6000.6.x and the latest release tag were not confirmed (the GitHub page fetch had no release info; `gh api` was unavailable in this session).
- None of the timings above have been measured. The editor isn't installed.
- The Unity CLI's `test` filter syntax and result-file format weren't checked against docs.

## Conflicting information

- Unity's CLI blog describes agents verifying gameplay in Play mode [S1], while HN practitioners say physics and real-time games are very hard for LLMs to verify visually [S18, S19]. Resolution: agents verify through coded assertions and state dumps, and use screenshots only as supporting evidence.

## Sources (accessed 2026-10-11)

- S1 Unity blog, "Meet the Unity CLI": https://unity.com/blog/meet-the-unity-cli (high)
- S5 Unity Test Framework `UnityTest` / coroutine yield instructions: https://docs.unity3d.com/Manual/coroutines-yield-instructions.html ; https://docs.unity.cn/Packages/com.unity.test-framework@2.0/api/UnityEngine.TestTools.UnityTestAttribute.html (high)
- S6 `Physics.Simulate` (6000.6): https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/physics/simulate ; `SimulationMode.Script`: https://docs.unity3d.com/ScriptReference/SimulationMode.Script.html (high)
- S7 Input System testing: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.6/manual/Testing.html (high)
- S8 InputEventTrace summary (tessl registry, medium; API names consistent with Input System docs): https://tessl.io/registry/testland/gameplay-recording-replay
- S9 Graphics Test Framework manual 8.9: https://docs.unity3d.com/Packages/com.unity.testframework.graphics@8.9/manual/index.html (high)
- S10 Graphics TF changelog 8.7: https://docs.unity3d.com/Packages/com.unity.testframework.graphics@8.7/changelog/CHANGELOG.html (high)
- S11 Performance Testing 3.0 manual: https://docs.unity3d.com/Packages/com.unity.test-framework.performance@3.0/manual/index.html (high)
- S12 Performance Testing 2.0 API; ProfilerRecorder: https://docs.unity3d.com/Packages/com.unity.test-framework.performance@2.0/manual/index.html ; https://docs.unity3d.com/ScriptReference/Unity.Profiling.ProfilerRecorder.html (high)
- S13 Automated QA status: https://docs.unity3d.com/Packages/com.unity.automated-testing@latest (high)
- S14 Firebase Test Lab Game Loop: https://firebase.google.com/docs/test-lab/android/game-loop (high)
- S15 AltTester Unity SDK v2.3.1: https://alttester.com/introducing-alttester-unity-sdk-v2-3-1-alttester-unreal-sdk-v1-2-1/ (vendor, medium)
- S16 GameDriver: https://gamedriver.io ; https://www.silicon.co.uk/press-release/gamedriver-unveils-standalone-test-assistant-for-unity-and-unreal-alongside-transparent-pricing (vendor, medium)
- S17 HN, Godogen (visual QA agent): https://news.ycombinator.com/item?id=47400868 (community)
- S18 HN, "Letting AI play my game": https://news.ycombinator.com/item?id=47947525 (community)
- S19 HN, "Why are LLMs so terrible at video games?": https://news.ycombinator.com/item?id=48354405 (community)
- S20 Bugnet, "Do I need automated testing for my game": https://bugnet.io/blog/do-i-need-automated-testing-for-my-game (practitioner, medium)
- S21 MY.GAMES on Medium, "When and how to properly do automated QA testing in gamedev": https://medium.com/my-games-company/when-and-how-to-properly-do-automated-qa-testing-in-gamedev-a4729ebabb05 (medium)
- S22 GameCI unity-test-runner: https://github.com/game-ci/unity-test-runner (medium-high)
- S23 Unity Discussions, manual Personal license activation removed: https://discussions.unity.com/t/unity-no-longer-supports-manual-activation-of-personal-licenses/926760 (high)
- S24 hatayama/unity-cli-loop: https://github.com/hatayama/unity-cli-loop (medium-high)
- S25 Unity MCP servers (CoplayDev, CoderGamester run_tests): https://glama.ai/mcp/servers/CoderGamester/mcp-unity/tools/run_tests ; https://github.com/CoplayDev/unity-mcp (medium)
- S26 UTF 1.5 landing page (docs moved to Unity Manual 6.2+): https://docs.unity3d.com/Packages/com.unity.test-framework@1.5/manual/index.html (high)
- S27 EA SEED, "Exploring Gameplay With AI Agents": https://arxiv.org/abs/1811.06962 (academic)
- S28 EA SEED, curiosity-driven RL playtesting (CoG 21): https://arxiv.org/abs/2103.13798 (academic)
- S29 "Leveraging LLM Agents for Automated Video Game Testing": https://arxiv.org/abs/2509.22170 (academic, preprint)
- S30 "CA2: Code-Aware Agent for Automated Game Testing": https://arxiv.org/abs/2605.13918 (academic, preprint)

> **Coverage note (2026-10-11):** Reddit could not be read (HTTP 403 from both the research agent and the local machine). Community evidence comes from Hacker News and practitioner blogs only.
