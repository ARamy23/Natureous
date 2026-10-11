# 04 — Edge spheres, raycasts, extra gravity and Translate movement

## Decision
- **Sphere spawning.** `CharacterControl.SetupColliderSpheres` instantiates `ColliderEdgePrefab` at the corners of the BoxCollider bounds, then adds middle spheres (`Controllers/CharacterController/CharacterControl.cs:134-162,177-193`). Counting from the code, that is 3 corners + 4 horizontal + 9 vertical = **16 instantiations** per character. `BottomSpheres` holds 6 and `FrontSpheres` holds 11, with the bottom-front sphere shared. `performance-bottlenecks.md` Cascade 5 says 13 and Cascade 3 says "10-13" bottom raycasts. The code gives 16 and 6.
- **Ground check.** `GroundDetector.IsGrounded` treats a Y velocity in (-0.001, 0] as grounded. Otherwise it casts one ray down from each bottom sphere and filters out ragdoll, ledge, ledge-checker and character hits (`States/Abilities States Scripts/GroundDetector.cs:35-61`). Each hit runs `GetComponent` and `transform.root.GetComponent` (`Characters/Ledge.cs:13-26`).
- **Block check.** `MoveForward.IsBlocked` casts one ray forward from each front sphere, which is 11 rays (`MoveForward.cs:119-138`).
- **Ledge detection.** A trigger on `LedgeChecker` matches `Ledge` components (`Characters/LedgeChecker.cs:13-35`).
- **Extra gravity.** `FixedUpdate` adds downward velocity when falling, plus a pull when rising without Jump held (`CharacterControl.cs:164-175`).
- **Movement.** `transform.Translate(Vector3.forward * ...)` on a Rigidbody object, in local space (`CharacterControl.cs:195-198`), called from `StateData` `OnStateUpdate` (`MoveForward.cs:64,94,104`).

## Goal behind it
- **Inferred:** this comes from the tutorial lineage (commit `85bdd52`). It gives per-point control over edges for falling off platforms and ledge grabs.
- **Stated (Trello):** "Auto detect ledge and climb" (`docs/trello/board.md:28`) and "when character is falling or coming of the edges of the world, it goes into falling animation" (`:178`).
- **Inferred:** the snappy jump feel from extra gravity and hold-to-jump-higher (CLAUDE.md controls).

## What it achieved / where it hurt
- **Achieved:** ledge grab and climb, edge falls and variable jump height shipped in Release 0.2.
- **Hurt: raycast count.** While moving and falling, one character casts up to 6 down rays and 11 forward rays per Animator update. That scales with character count (`performance-bottlenecks.md` Cascade 3).
- **Hurt: spawn cost.** 16 GameObjects are instantiated per character on spawn (`performance-bottlenecks.md` Cascade 5).
- **Hurt: physics mismatch.** `Translate` on a non-kinematic Rigidbody moves the transform outside the physics step. Movement runs in Animator update, which follows the frame rate, while gravity runs in `FixedUpdate`. Tunnelling and jitter are possible; this is unverified.
- **Hurt: frame-rate-dependent gravity.** The extra gravity is a fixed velocity added per physics step with no `Time.fixedDeltaTime` factor. It depends on the fixed timestep setting, which has not been analysed (`performance-bottlenecks.md` "Not covered").

## Unity 6 status
`Rigidbody.velocity` is renamed to `linearVelocity`. The old name is obsolete and the API Updater offers to rewrite it (migration findings item 3, sites `CharacterControl.cs:122,130,166,168,171,173` and `GroundDetector.cs:37`, UNVERIFIED). `Physics.Raycast` and `Transform.Translate` are unchanged.

## Probe question
Does replacing the 6 bottom-sphere raycasts with a single `Physics.BoxCast` or `SphereCast` produce the same `Grounded` result on every frame of a scripted run over `Sandbox` platforms and edges?

**Probe method:** a PlayMode test that records `IsGrounded` (current implementation) and a candidate single-cast result side by side over a fixed input script (walk off an edge, jump, land on a ledge). It asserts zero divergent frames and logs a `ProfilerMarker` for each variant.
**Budget:** about half a day (needs a scripted input driver, which could be shared with probe 03).

## Pending research
- Should ground and wall checks use `Physics.BoxCast`, `SphereCast`, `CapsuleCast` or `OverlapBox` with a `LayerMask` instead of component filtering?
- Should edge points be plain `Vector3` offsets computed from bounds, instead of GameObjects?
- Should movement use `Rigidbody.MovePosition` or `linearVelocity` in `FixedUpdate`, or a kinematic character controller (Unity `CharacterController`, or the open-source Kinematic Character Controller)?
- Should extra gravity be scaled by `Time.fixedDeltaTime`, or should the project's gravity or `ConstantForce` be used?
- Would batched `RaycastCommand` (Jobs) matter at the expected enemy counts?

## Verdict
**REPLACE-CANDIDATE** for the GameObject spheres and per-sphere raycasts. **REVISIT** for Translate movement and extra gravity, which are tied to game feel and need playtesting. Both are provisional.
