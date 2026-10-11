# 02 — Singleton managers

## Decision
`Singleton<T> : MonoBehaviour` (`Assets/Natureous/Managers/Singleton.cs:7-25`) behaves as follows:
- Line 15: it assigns `_instance = FindObjectOfType(typeof(T))` on **every** `Instance` get. It never checks the cached field first.
- Lines 16-21: if nothing is found, it auto-creates a new GameObject with `T` attached.
- Line 22: it calls `DontDestroyOnLoad(_instance)` on every access.

Subclasses are `VirtualInputManager`, `AttackManager`, `PoolManager`, `CharacterManager`, `CheckpointManager`, `CameraManager`, `DeathAnimationsManager` and `AnalyticsManager`. `.Instance` appears 58 times across own `.cs` files.

## Goal behind it
**Inferred:** global access with no scene wiring, so a `StateData` asset, which cannot hold scene references, can reach managers (`where-we-wanted-to-be.md` §3). Auto-create lets any scene run without setup. No commit message or Trello card states the goal.

## What it achieved / where it hurt
- **Achieved:** ScriptableObject abilities reach scene services (for example `Attack.cs:27,34`) without any injection.
- **Hurt: per-frame scene searches.** Hot paths trigger a search on every access: `KeyboardInput.Update` makes 7 accesses (`KeyboardInput.cs:19-25`), `ManualInput.Update` 7 (`ManualInput.cs:20-26`), and `DamageDetector.Update` 2 or more per character (`DamageDetector.cs:23,31`). See `performance-bottlenecks.md` Cascades 1-2. The cost per call has not been measured, and that doc's ms figures are not sourced.
- **Hurt: hidden lifetime.** Scene-placed managers become `DontDestroyOnLoad` on first access, so per-level state such as `CheckpointManager.CurrentCheckpoint` and `AttackManager.CurrentAttacks` survives scene loads. The impact on correctness is unverified.
- **Hurt: no isolation in tests.** Global state cannot be isolated (`where-we-wanted-to-be.md` §3).

## Unity 6 status
`FindObjectOfType` is obsolete (a warning, not an error) since 2023.1. The replacement is `FindFirstObjectByType` or `FindAnyObjectByType` (migration findings item 1, UNVERIFIED). It compiles, but with warnings.

## Probe question
Is the `FindObjectOfType` in `Singleton.Instance` a significant share of script CPU time in `Sandbox` with the player and 2 enemies?

**Probe method:** add `ProfilerMarker("Singleton.Instance")` around the getter body. Run a PlayMode test, or a batchmode `-runTests` with the Profiler recorder (`Unity.Profiling.ProfilerRecorder`), that loads `_preload`, lets it move to `Sandbox`, and records 600 frames. Report the marker total time per frame and the call count against `PlayerLoop` script time.
**Budget:** under 3 h. A single number answers it.

## Pending research
- Would a cache-first getter (`if (_instance == null)`) remove most of the cost, and what lifetime semantics change with it?
- Which managers are truly global (Analytics) and which are per-level (Checkpoint, Attack, Character)? Should the per-level ones be scene-scoped?
- Alternatives to compare: ScriptableObject service/event channels, a small service locator registered in `_preload`, or a DI framework (VContainer, Zenject/Extenject). How does each fit `StateData` assets?
- Is the `[RuntimeInitializeOnLoadMethod]` bootstrap a cleaner replacement for auto-create?

## Verdict
**REPLACE-CANDIDATE** for the implementation (the uncached search plus `DontDestroyOnLoad` on every get). The global-access idea itself is a **REVISIT**. Both are provisional, pending the probe.
