# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Natureous — 2.5D action-adventure / hack-and-slash game in Unity (desktop + mobile). Game vision, story and backlog live on Trello ("First Game", https://trello.com/b/aawcUoCf/first-game); snapshot in `docs/trello/` (re-export via the claude.ai Trello connector). Docs follow nWave DIVIO layout (`docs/{tutorials,howto,reference,explanation}/`, features under `docs/feature/`).

## Branches

- `develop` — default branch; all work merges here.
- `master` — releases only. Merge `develop` → `master` when cutting a release.

## Toolchain

- Unity **2019.3.5f1** (`ProjectSettings/ProjectVersion.txt`), URP 7.1.8, Cinemachine, TextMeshPro, Unity Test Framework 1.1.11 (no tests or asmdefs exist yet).
- SDKs in `Assets/`: GameAnalytics, Smartlook, AppCenter, Play Services Resolver, Joystick Pack (mobile controls).
- No CLI build/test scripts. Typical headless commands (adjust Unity path):
  - Tests: `/Applications/Unity/Hub/Editor/2019.3.5f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml` (single test: add `-testFilter <FullTestName>`)
  - Compile check: same binary with `-batchmode -quit -projectPath . -logFile -`
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
- `AnalyticsManager` wraps GameAnalytics + Smartlook; gameplay events (checkpoint, death, abyss fall, kill, game over, survey) go through it.
- `Singleton<T>` calls `FindObjectOfType` on every `Instance` access and auto-creates a GameObject if missing — cache references in hot paths.
- Pooling: `PoolManager` / `PoolObject` / `PoolObjectLoader`, prefabs loaded from `Resources` by `PoolObjectType` name.

Third-party content — don't modify: `AppCenter*/`, `GameAnalytics/`, `Smartlook/`, `Joystick Pack/`, `PlayServicesResolver/`, `Plugins/`, `Roundbeargames_Tutorial/` (tutorial series the architecture follows).
