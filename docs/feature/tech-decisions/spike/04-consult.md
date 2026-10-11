# 04 — Consult: ground, ledge and wall detection for a 2.5D fighter

Companion to `04-edge-sphere-collision.md`. This is research only. Claims marked **UNVERIFIED** were not confirmed against a source during this spike.

## Audit (what the sphere approach actually does)

| Issue | Evidence in code | Real effect |
|---|---|---|
| 16 GameObjects per character used as ray origins | `CharacterControl.cs:134-193` | Spawn cost plus transform overhead. The same points can be computed from `collider.bounds` as plain `Vector3`. Nothing needs to be a GameObject. |
| Up to 6 down rays + 11 forward rays per Animator update | `GroundDetector.cs:47-49`, `MoveForward.cs:119-138` | One `BoxCast` or `SphereCast` per direction gives the same coverage with a single query. |
| No LayerMask; filtering via `GetComponent` on each hit | `GroundDetector.cs:49`, `Ledge.cs:13-26` | Wasted work and allocation-prone lookups. A `LayerMask` filters inside the physics engine at no extra cost. |
| `transform.Translate` on a non-kinematic Rigidbody | `CharacterControl.cs:197` | Moving the Transform directly teleports the body. The solver does not sweep that motion, so it can tunnel or jitter against walls. Unity's docs say to move Rigidbodies through forces, velocity, or `MovePosition` for kinematic bodies, not through the Transform ([Rigidbody manual](https://docs.unity3d.com/Manual/class-Rigidbody.html), [MovePosition](https://docs.unity3d.com/ScriptReference/Rigidbody.MovePosition.html)). |
| Movement in Animator update (variable dt), gravity in FixedUpdate | `CharacterControl.cs:164-197` | Two clocks drive one body. The jump arc and horizontal speed fall out of sync when frame rate changes. |
| Extra gravity not dt-scaled | `CharacterControl.cs:168,173` | `velocity += down * k` per fixed step makes jump feel depend on `Time.fixedDeltaTime`. The fix is `k * Time.fixedDeltaTime` (acceleration), or `AddForce(..., ForceMode.Acceleration)`. |
| "Grounded if vy in (-0.001, 0]" shortcut | `GroundDetector.cs:37` | The character also counts as grounded at the apex of a jump, and while pinned against a wall. Correctness bug: possible double jumps or false landings (**UNVERIFIED in play**). |
| Ledge = trigger on `LedgeChecker` touching hand-placed `Ledge` | `LedgeChecker.cs`, `Ledge.cs` | Reliable but authored by hand: every grabbable edge needs a Ledge object. `TeleportOnLedge` and `OffsetOnLedge` snap position, which is acceptable for a climb. |

Overall: the design works, but it is expensive and fragile. Its root problem is not the spheres themselves. It mixes Transform movement with a dynamic Rigidbody.

## Options

| Option | How | Pros | Cons | Fit for 2.5D fighter | Migration effort |
|---|---|---|---|---|---|
| (a) Rigidbody + shape casts | Set `linearVelocity` in FixedUpdate. Ground check = one `Physics.BoxCast`/`SphereCast` down with a ground `LayerMask`. Wall check = one cast forward. Use `*NonAlloc` variants or `Physics.SphereCastNonAlloc` buffers ([docs](https://docs.unity3d.com/ScriptReference/Physics.SphereCastNonAlloc.html)). | Smallest change. Keeps ragdoll and knockback forces. One query per direction. | Dynamic bodies still slide on slopes, bounce off steps, and get pushed by other characters. You fight friction and materials to get a crisp feel. | Good | Low (1-3 days) |
| (b) Unity `CharacterController` component | `controller.Move(v * dt)` with your own gravity. Use `isGrounded` plus a cast for ledges ([docs](https://docs.unity3d.com/ScriptReference/CharacterController.html)). | Collide-and-slide is built in, as are steps and slopes. Deterministic feel. | Capsule only and always upright. It does not interact with Rigidbodies, so knockback and ragdoll handoff must be manual. `isGrounded` is only valid after `Move`. | Good for platforming, medium for hit reactions | Medium |
| (c) Kinematic Character Controller (P. St-Amand) | Kinematic capsule with a collide-and-slide solver and a motor interface. | Very controllable, precise, and known in the community. | Asset Store package, free. Maintenance status for Unity 6 is **UNVERIFIED** ([forum thread](https://discussions.unity.com/t/released-kinematic-character-controller/678434)). The open-source alternative is [OpenKCC](https://github.com/nicholas-maltbie/OpenKCC) (MIT). Adds a dependency plus a learning curve. | Good, but overkill for a single plane | Medium-High |
| (d) Physics2D for gameplay, 3D for visuals | 2D colliders on the gameplay plane (`Rigidbody2D`, `Physics2D.BoxCast`, `ContactFilter2D`), with 3D meshes rendered on top. | Gameplay is 2D, so this matches it. Cheaper and simpler queries. The 2D platformer toolset includes one-way platforms (`PlatformEffector2D`). | Every level and character collider must be rebuilt in 2D. Ragdolls are 3D. You lose any depth-lane movement (Trine/Oceanhorn-style depth). | Excellent if movement is strictly planar | High (re-author colliders) |
| (e) Ledge detection: two casts | Cast forward at chest height (hit) and at head height (miss), then cast down from above the hit to find the ledge top. Run this only while airborne or falling. | No hand-placed Ledge objects, so any edge on a `Ledge` layer works. | Needs tuning (distances, wall normal checks). Can grab odd geometry. | Good; combine with (a)/(b) | Low-Medium |
| (e') Ledge triggers (current) | Keep the authored `Ledge` volumes, but match by layer, not `GetComponent`. | Designer-controlled and predictable. | Manual authoring per edge. | Fine for small levels | Very low |

How shipped 2.5D action games and fighters handle this: **unknown / not verified**. No postmortem was found for Trine or Oceanhorn. Common practitioner lore says traditional fighters use custom AABB hitboxes and pushboxes, not engine physics (**UNVERIFIED**).

Unity 6 API notes: `Rigidbody.velocity` was renamed to `linearVelocity` ([docs](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Rigidbody-linearVelocity.html)). `Physics.*NonAlloc` still exists. Batched queries are available through `RaycastCommand` and `SpherecastCommand` ([docs](https://docs.unity3d.com/ScriptReference/RaycastCommand.html)). These only matter with dozens of characters. Single casts at a handful of characters cost well under budget (**UNVERIFIED, measure**). The 6000.6 version-specific docs pages were not checked.

## Recommendation

**Option (a) plus (e) now, keeping (e') where ledges are already authored.**

1. Drop the sphere GameObjects. Compute origins from `bounds`, or better, use one `BoxCast` down and one `CapsuleCast` forward, each with a `LayerMask`.
2. Move all locomotion into FixedUpdate. The Animator writes intent (target speed); FixedUpdate sets `linearVelocity.x/z`. Remove `Translate`.
3. Make extra gravity an acceleration (`AddForce(Vector3.down * g, ForceMode.Acceleration)`).
4. Remove the `vy ≈ 0` grounded shortcut, so the cast alone decides grounded.

Reasoning:
- This is the lowest-effort path that fixes every correctness issue in the audit.
- It keeps the existing Rigidbody, ragdoll and knockback design.
- It does not foreclose (b) or (d) later.

Switch to (d) only if you confirm movement is strictly single-plane and you want fighting-game-precise collision.

## Runnable probe (once the Unity 6.6 editor exists)

The PlayMode test runs a scripted input sequence over `Sandbox`:
- walk off an edge
- jump at the apex
- run into a wall
- jump to a ledge

Each FixedUpdate it records:
- `old IsGrounded` vs a `BoxCast` result
- forward-blocked (11 rays) vs a `CapsuleCast` result
- the position delta

Pass criteria:
- Zero divergent frames, except the apex frames, which are expected and prove bug #7.
- No wall penetration at `Time.fixedDeltaTime` 0.02 and 0.01.
- Jump height within 2% across both timesteps after the gravity fix.

Wrap each variant in a `ProfilerMarker`. Budget: about half a day, shared with probe 03's input driver.

## Questions for you

1. Is movement strictly on one plane, or do you want depth lanes or 3D wandering (Trine/Oceanhorn-style)? This decides whether (d) is on the table.
2. Does combat need frame-exact, fighting-game hit and push boxes, or is platforming feel the priority, with combat on loose triggers?
3. Should ledges be designer-authored (keep `Ledge` volumes) or auto-detected on any edge (two-cast)?
4. Do knockback and ragdolls need to interact physically with other bodies? That favours a Rigidbody (a) over `CharacterController` (b) or KCC (c).
