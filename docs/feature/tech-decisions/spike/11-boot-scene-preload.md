# 11 Boot scene (_preload)

## Decision
- `ProjectSettings/EditorBuildSettings.asset`: the build order is `_preload` (0), `Sandbox` (1), `Game Over Scene` (2). `Tutorial Scene.unity` and `Assets/Scenes/SampleScene.unity` exist but are **not** in the build.
- `Managers/ApplicationManager.cs:10-14`: `Awake` sets `Application.targetFrameRate = 60` and calls `SceneManager.LoadScene(1)`, a hard-coded index.
- Managers do not rely on _preload being loaded. `Singleton<T>` creates them on first `Instance` access with `DontDestroyOnLoad` (CLAUDE.md; performance-bottlenecks.md Root Cause).

## Goal behind it
- **Inferred:** this is the common "_preload" pattern, a single entry point that sets app-wide settings before gameplay. No commit message or Trello card states it. CLAUDE.md warns to play from `_preload` so singletons and analytics initialise as they do in a build.
- **Inferred:** 60 fps was chosen as the mobile frame target.

## What it achieved / where it hurt
- Achieved: a predictable startup and a frame cap.
- Hurt:
  - Because Singletons auto-create anywhere, _preload does not actually own manager lifetime. Playing Sandbox directly still works but can differ from a build (CLAUDE.md).
  - `LoadScene(1)` and `Portal.sceneToLoad` (int) tie behaviour to the build order (project-as-built.md "Scene Build Index" invariant).
  - On mobile, `targetFrameRate = 60` is a cap, not a guarantee. The documented perf issues are CPU-bound (performance-bottlenecks.md).

## Unity 6 status
The findings flagged nothing for `SceneManager.LoadScene` or `Application.targetFrameRate`. **UNVERIFIED:** Unity 6 also offers `RuntimeInitializeOnLoadMethod` bootstrapping and Addressables scene loading, but neither is a migration requirement.

## Probe question
Does every manager the game uses exist exactly once after boot from `_preload`, and does it still exist after the Portal load to Game Over?

Probe method (run later):
- PlayMode test. Load scene 0, wait for scene 1, assert `FindObjectsOfType<T>().Length == 1` for each Singleton type, trigger the Portal, and assert again.
- Batchmode: `-runTests -testPlatform PlayMode`.
- Budget: 2 hours.

## Pending research
- Should bootstrapping move to `[RuntimeInitializeOnLoadMethod]` or a bootstrap prefab so that playing any scene in the editor behaves like the build?
- Should scenes be referenced by name, a ScriptableObject scene registry, or Addressables instead of index?
- Should frame rate be per-platform (for example, 30 or 60 on mobile, vSync on desktop) and user-configurable?

## Verdict
**KEEP-CANDIDATE** (provisional). The pattern is fine. The coupling to hard-coded indices should be revisited together with 08 and the Singleton work.
