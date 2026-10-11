# 08 Checkpoint / respawn / portal / fader

## Decision
- `Core/Checkpoint.cs:11-30`: a trigger that accepts only a `BoxCollider` whose root has `CharacterControl` with `ManualInput` enabled. It sets `CheckpointManager.Instance.CurrentCheckpoint = this` (:27) and logs analytics (:28). A one-frame re-entry guard is reset by a coroutine (:32-36).
- `Managers/CheckpointManager.cs:7-9`: a `Singleton` that holds a single `Checkpoint` field. There is no persistence, so a scene reload or app restart loses progress.
- `Core/Respawner.cs`:
  - Detects an abyss fall with the hard-coded check `y < -20` inside `LateUpdate` (:11-17, :80-84).
  - `Transition` (:29-47) and `TransitionBackToLife` (:49-73) fade out, move the player to `(0, cp.y, cp.z)` with **X forced to 0** (:40, :61), restore the animator, colliders and gravity, then fade in.
  - Falls back to the `Awake` spawn position (:75-78).
  - Uses `FindObjectOfType<SceneFader>()` (:31, :51).
- `Objects/Portal/Portal.cs:24-42`: `DontDestroyOnLoad(gameObject)`, then fade, `LoadSceneAsync(sceneToLoad)` by build index (`-1` is guarded at :26), then fade in and `Destroy`.
- `Scenes/Scenes Utilities/SceneFader.cs:17-33`: lerps the `CanvasGroup` alpha using `Time.deltaTime`.

## Goal behind it
- **Stated (Trello):**
  - "Checkpoints and Japanese Gate" ✅ (board.md:78)
  - "Fader For next scene when reaching the end level's gate" ✅ (board.md:183)
- **Stated (commits):**
  - "Implement a Checkpoint System" (280e7cc)
  - "Add Nagy's gate as the last checkpoint" (c823070)
  - "Implement Fader" (3607d16)
  - "Fader when respawning" (5359620)
- **Inferred:** with one-hit death (where-we-wanted-to-be.md Tradeoff 4), checkpoints keep failure cheap. Fades hide the teleport.

## What it achieved / where it hurt
- Achieved: a playable loop through one level up to the Game Over scene.
- Hurt:
  - The `LateUpdate` abyss check runs every frame. Each fall adds a Singleton access, and since `IsFallingIntoTheAbyss` stays true until the coroutine moves the player, the event can be logged and `Respawn()` started repeatedly during the fade-out (performance-bottlenecks.md Cascade 6, impact LOW; repeated-start risk read from code, not measured).
  - `X=0` and `y<-20` are level-specific magic values.
  - Respawn starts no new coroutine guard, so overlapping fades are possible.
  - `SceneFader` is not null-checked, so a scene without one would throw.
  - Portal loads by build index, which ties it to the order in EditorBuildSettings (see 11).

## Unity 6 status
`FindObjectOfType` in Respawner.cs:31,51 and Portal.cs:34 is obsolete (warning) (findings.md item 1). `SceneManager.LoadSceneAsync`, `CanvasGroup` and `DontDestroyOnLoad` were not flagged.

## Probe question
After an abyss fall, does exactly one respawn happen, and is exactly one `LogPlayerHasFallenIntoAbyss` event logged?

Probe method (run later):
- PlayMode test. Place the player at y = -21 with a checkpoint set.
- Count `Respawn` invocations and analytics calls through a test double at the `AnalyticsManager` boundary across the 0.5 s fade.
- Assert a count of 1 and that the final position equals `(0, cp.y, cp.z)`.
- Budget: 2 hours.

## Pending research
- Should the abyss be a trigger volume (`OnTriggerEnter`) or a per-frame poll?
- Should the checkpoint persist (PlayerPrefs, a save file) so progress survives an app restart, which mobile players expect?
- Should scenes be loaded by name or by Addressables instead of build index?
- Should a single transition service own the fader, instead of each caller using `FindObjectOfType`?

## Verdict
**KEEP-CANDIDATE** (provisional). The concept is sound. It needs a respawn guard and a fix for the magic values.
