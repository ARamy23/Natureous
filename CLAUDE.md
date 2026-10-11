# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Natureous — 2.5D action-adventure / hack-and-slash game in Unity (desktop + mobile). Game vision, story and backlog live on Trello ("First Game", https://trello.com/b/aawcUoCf/first-game); snapshot in `docs/trello/` (re-export via the claude.ai Trello connector). Docs follow nWave DIVIO layout (`docs/{tutorials,howto,reference,explanation}/`, features under `docs/feature/`).

## Branches

- `develop` — default branch; all work merges here.
- `master` — releases only. Merge `develop` → `master` when cutting a release.

## Toolchain

- Unity **6000.6.5f1** (Unity 6.6, arm64), migrated from 2019.3 in 2026-10 (see `docs/feature/unity6-migration/spike/findings.md`). URP 17.6, Cinemachine 6.6, Timeline 6.6, uGUI 2.6 (includes TextMeshPro), Unity Test Framework 1.8.
- SDKs in `Assets/`: Smartlook, AppCenter (service shut down; unused by our code), Play Services Resolver, Joystick Pack. **GameAnalytics was removed**; its calls in `AnalyticsManager` are commented out, call sites kept for a replacement.
- Editor binary: `/Applications/Unity/Hub/Editor/6000.6.5f1/Unity.app/Contents/MacOS/Unity` (`$U` below). Unity CLI: `~/.unity/bin/unity` (not on PATH).
  - PlayMode tests: `$U -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults results.xml -logFile test.log` (single test: add `-testFilter <FullTestName>`; EditMode: `-testPlatform EditMode`)
  - Compile check: `$U -batchmode -quit -projectPath . -logFile -`, then grep the log for `error CS` and "Scripts have compiler errors"
  - `unity test --mode PlayMode .` also works, but needs the macOS keychain prompt allowed once
  - Measured: cold open ~49 s, warm open ~21 s, boot smoke test ~13 s
- Tests: `Assets/Tests/PlayMode/` (`Natureous.Tests.PlayMode` asmdef). Game code has no asmdef, so tests reach it by type name via reflection. Don't list the test runner in both `references` and `optionalUnityReferences` of a test asmdef ("duplicate references" aborts compilation with no `error CS` line).
- Change scenes, prefabs and assets through editor scripts (see `Assets/Editor/Migration/`) run with `-executeMethod`, never by hand-editing their YAML.
- Always commit `.meta` files alongside assets; never hand-edit GUIDs.

## Scenes & boot

Build order (`EditorBuildSettings`): `_preload` (index 0) → `Sandbox` → `Game Over Scene`. `_preload` holds `ApplicationManager`, which sets 60 fps and loads scene 1. Play from `_preload` in the editor, or singletons/analytics may not initialise as in a build.

Keyboard: A/D move, W/S up/down, Shift sprint, Space jump (hold = higher), J attack. Mobile: joystick + attack button (`MobileInput`); `KeyboardInput` disables itself on Android/iOS.

## Architecture (`Assets/Natureous/`, namespace `Natureous`)

Gameplay logic is driven by the **Animator state machine**, not MonoBehaviour Update loops:

- `CharacterState` (a `StateMachineBehaviour`) sits on Animator states and holds a list of `StateData` ScriptableObjects ("abilities"); it forwards OnEnter/Update/Exit to each.
- `StateData` subclasses live in `States/Abilities States Scripts/` (movement, jump, ledge, attack, turn, sprint checks, toggles…) and `States/AI/Locomotion/` (AI path-following). Configured **instances** are `.asset` files grouped in `States/{General,Movement,Attacks,Cliff Climbing,Death,AI}/`. New behaviour = new `StateData` subclass + asset + attach to an Animator state in the editor.
- Abilities drive the Animator via the `TransitionParameter` enum (in `CharacterControl.cs`) — enum names must match Animator parameter names exactly.
- `CharacterControl` is the per-character hub: input flags, Rigidbody, extra gravity/pull, procedurally spawned edge-sphere colliders (ground/edge detection), ragdoll parts, facing direction (Y rotation 0/180; movement is along Z).
- Input: `KeyboardInput` / `MobileInput` → `VirtualInputManager` (singleton) → `ManualInput` copies into the player's `CharacterControl`. AI characters are driven by `AIProgress` + `PathFindingAgent` instead.
- Combat: `Attack` state data pulls a pooled `AttackInfo` from `PoolManager` and registers it in `AttackManager.CurrentAttacks`; `DamageDetector` on targets matches attacks against ragdoll-part `TriggerDetector`s, swaps in a death animator from `DeathAnimationsManager`, shakes camera via `CameraManager`.
- Level flow: `Checkpoint` → `CheckpointManager.CurrentCheckpoint`; `Respawner` respawns at it (and on abyss falls); `PlayerDetector` spawns enemies; `Portal` ends level; `SceneFader` handles transitions.
- `AnalyticsManager` wraps Smartlook (GameAnalytics calls commented out); gameplay events (checkpoint, death, abyss fall, kill, game over, survey) go through it.
- `Singleton<T>` caches its instance (looked up once with `FindAnyObjectByType`, again only if destroyed), auto-creates a GameObject if missing, and marks it `DontDestroyOnLoad`.
- Pooling: `PoolManager` / `PoolObject` / `PoolObjectLoader`, prefabs loaded from `Resources` by `PoolObjectType` name.

Third-party content — don't modify: `AppCenter*/`, `Smartlook/`, `Joystick Pack/`, `PlayServicesResolver/`, `Plugins/`, `Roundbeargames_Tutorial/` (tutorial series the architecture follows).

## Known issues and decisions

Technical decisions, confirmed bugs and the agreed direction live in `docs/feature/tech-decisions/spike/` (start with `wave-decisions.md`). Don't fix the gameplay-changing bugs listed there (squared `LethalRange`, grounded at jump apex, shared `Attack` asset state, per-attack `MaxHits`) without the owner's sign-off.
