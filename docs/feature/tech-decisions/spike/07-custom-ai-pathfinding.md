# 07 Custom AI pathfinding (NavMesh "scout" + Animator locomotion)

## Decision
AI does **use Unity NavMesh**, but only as an invisible scout that finds the next waypoint. The character itself is moved by Animator `StateData`, not by the NavMesh.
- `Characters/PathFindingAgent/PathFindingAgent.cs:4,12,23`: `UnityEngine.AI.NavMeshAgent` sits on a separate prefab (`Characters/PathFindingAgent/Resources/PathFindingAgent.prefab`: 1 NavMeshAgent, `m_AutoTraverseOffMeshLink: 0`, `m_Speed: 300`).
- `PathFindingAgent.cs:55-88`: a coroutine checks for `isOnOffMeshLink` or arrival, writes the result into `StartSphere`/`EndSphere`, stops the agent and sets `StartWalk`. It polls with `WaitForEndOfFrame`.
- `States/AI/Locomotion/Locomotion State Scripts/SendPathFindingAgent.cs:15-23`: lazy `Resources.Load("PathFindingAgent")` plus `Instantiate`, which teleports the agent to the character and calls `GoToTarget`.
- `StartWalking.cs:36-82` compares the sphere Y values to choose walk, jump or fall (`AIWalkTransition` enum, :8-13). `JumpPlatform.cs` and `FallPlatform.cs` steer by setting `MoveRight/MoveLeft/Jump` flags on `CharacterControl`, the same flags the player's input sets.
- To restart the state machine they use `animator.gameObject.SetActive(false); SetActive(true)` (StartWalking.cs:51-52, JumpPlatform.cs:48-49,60-61, FallPlatform.cs:35-36,51-52,68-69).
- `Scenes/Sandbox/NavMesh.asset` is baked, and `Sandbox.unity` has 6 `OffMeshLink` components. These links mark the jumps and drops between platforms.
- `States/AI/PlayerDetector.cs:18-45`: per-frame distance check. When the squared distance is 200 or less (about 14 m), it enables the dummy's `CharacterControl`. `AIProgress.cs:13-18` turns on child 0 at spawn.

## Goal behind it
- **Stated (Trello):** "AI - Enemies Move" ✅ and "AI- Enemies attack" ✅ (board.md:98,103). The commits "Complete basic AI navigation system" (130cb83) and "Setup basic foundation for a follow player system" (f7db851) say the same.
- **Inferred:** AI reuses the player's ability and Animator stack, so enemies get the same jump, fall and attack animations and physics. NavMesh does the graph search, and off-mesh links tell the AI where a jump or drop happens.
- **Stated (commit):** the optimisation commits "try optimization technique" (6f36f1c) and "Fix AI and optimize the game even more" (9c887b5) changed `PlayerDetector`, `AIProgress` and the locomotion scripts. That gives distance-gated spawning as an **inferred** perf goal.

## What it achieved / where it hurt
- Achieved: enemies that follow the player across platforms with the shipped animations (Trello ✅).
- Hurt:
  - Every frame, `Singleton` calls `CharacterManager.Instance` (PathFindingAgent.cs:38 and in each `OnEnter`). Cost: one `FindObjectOfType` per access (performance-bottlenecks.md, Root Cause).
  - `StateData` assets are shared ScriptableObjects, but `StartWalking`/`JumpPlatform`/`FallPlatform` cache `player` in an instance field. This is the shared-instance risk covered in performance-bottlenecks.md, Cascade 4.
  - Exact float equality on Y (`StartWalking.cs:40`, `JumpPlatform.cs:52`, `FallPlatform.cs:27`) is fragile.
  - The "restart" by `SetActive` toggling rebuilds the Animator state each time. Its cost has not been measured.
  - `PlayerDetector.Update` polls every frame until the enemy spawns.
  - "Temporary" attack branch: StartWalking.cs:54.
- Trello still lists "Improve AI Movement", "Allow AI to Sprint", "Allow AI to do Combos" and "Create AI Intelligence Levels" (board.md:403-439). The design is open (where-we-wanted-to-be.md Q7).

## Unity 6 status
The migration findings did not flag `NavMeshAgent`, `OffMeshLink` or a baked `NavMesh.asset`. **UNVERIFIED:** in recent Unity versions, NavMesh baking moved to the AI Navigation package (`com.unity.ai.navigation`, NavMeshSurface/NavMeshLink), and the legacy `OffMeshLink` component is marked obsolete. Phase B should confirm whether the baked scene NavMesh and the 6 OffMeshLinks still load and whether they can be re-baked without that package. `FindObjectOfType` via Singleton: obsolete warning (findings.md item 1).

## Probe question
Does the scout agent's NavMesh path, together with the 6 off-mesh links in Sandbox, still give `StartWalking` the correct jump or fall branch after the Unity 6 import?

Probe method (run later):
- PlayMode test in Sandbox. Spawn a dummy at a known platform, set the target to a point beyond one OffMeshLink, and assert that `AIWalkTransition.JumpPlatform` (or `FallPlatform`) becomes true on the Animator within N seconds.
- Run in batchmode with `-runTests -testPlatform PlayMode`.
- Add a Profiler marker around `PathFindingAgent.Move` and the `SetActive` toggles.
- Budget: half a day once the editor is installed.

## Pending research
- Should navigation move to the AI Navigation package (NavMeshSurface + NavMeshLink) or stay on legacy baked data?
- For a 2.5D game where movement is only along Z, would a hand-authored waypoint/platform graph be simpler than a 3D NavMesh scout?
- Should AI decisions move out of Animator StateData into a separate brain (FSM, behaviour tree, utility AI) that only sets input flags? That would also give the "AI intelligence levels" a config asset.
- Should the `SetActive` restart be replaced by an explicit Animator trigger?

## Verdict
**REVISIT** (provisional). The approach works, but it is fragile and depends on a legacy component.
