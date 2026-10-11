# 06 — Consult: hits, damage and death (audit + recommendation)

Scope: a research-only consult on spike 06 (`06-damage-by-collider-names.md`). It makes no code changes. The target engine is Unity 6.6 (6000.6.5f1). The planned features are a health system, blocking, deflect/counter, air combos and magic.
Legend: **[V]** means the source was fetched and verified during this consult. **[U]** means it was cited from knowledge and is unverified (the URL was not fetched). **[Code]** means it was confirmed by reading the source.

## 1. Audit of current implementation

| # | Finding | Evidence | Severity |
|---|---|---|---|
| A1 | **MustCollide bypass.** In `if (attack.MustCollide && DidCollide(attack)) … else { distance }`, a MustCollide attack that misses falls through to the distance check and can still kill. | `DamageDetector.cs:57-67` [Code] | Bug, high |
| A2 | **Squared-distance mismatch.** `Vector3.SqrMagnitude(...) <= LethalRange` makes the effective range `sqrt(LethalRange)`. For example, LethalRange 4 reaches 2 m, and LethalRange 0.5 reaches about 0.71 m. | `DamageDetector.cs:61-63` [Code] | Bug, medium (tuning values silently wrong) |
| A3 | **Shared ScriptableObject cross-talk (new).** `Attack` is a `StateData` ScriptableObject asset that all characters share. Registration, deregistration and `ClearAttack` match on `attackInfo.AttackAbility == this`. When character B uses the same Attack asset, character A's state can register, finish or remove B's `AttackInfo`. `FinishedAttacks` is also shared mutable state on the asset. | `Attack.cs:21,82-89,97-107,119-130` [Code] | Bug, medium (appears once enemies share moves) |
| A4 | **Stale body part on distance kills.** `DamagedBodyPart` is set only inside `DidCollide`. A distance kill uses whatever body part was set last, or the enum default `Upper`, so the death animation picked can be wrong. | `DamageDetector.cs:11,83,99` [Code] | Minor |
| A5 | **MaxHits counts hits per attack, not per target.** `CurrentHits` lives on the `AttackInfo`, so with MaxHits=1 a sweep can hit only one of two enemies. The code also has no "already hit this target" set. | `AttackInfo.cs:18-19`, `DamageDetector.cs:42,101` [Code] | Design gap |
| A6 | **Polling cost.** Every `DamageDetector` scans the global list each `Update`, using nested loops over body parts, colliders and names, plus string compares. The cost grows as targets × attacks × parts × names. | `DamageDetector.cs:21-93`. Cost estimates are in `performance-bottlenecks.md` and have not been measured. | Perf |
| A7 | **Name-string identity.** Hits match on `collider.name`. Renaming a bone breaks hits silently, with no validation. | `DamageDetector.cs:79` [Code] | Fragility |
| A8 | **Stale contacts.** `CollidingBodyParts` relies on `OnTriggerExit`. Unity does not send exit events when a collider is disabled or destroyed while inside a trigger, so entries can go stale. Death disables colliders, and the pool deactivates objects. | `TriggerDetector.cs:56-76`. Unity docs: [OnTriggerExit](https://docs.unity3d.com/ScriptReference/Collider.OnTriggerExit.html) [U] | Latent bug |
| A9 | **Active window in normalizedTime.** The window works but is frame-rate and transition-blend sensitive. A short window can be skipped entirely at a low frame rate, because the check runs in `Update` while physics runs in `FixedUpdate`. | `Attack.cs:80,95` [Code] | Design |
| A10 | **One hit = death, and death is welded to detection.** `TakeDamage` also swaps the controller, disables colliders and gravity, shakes the camera, logs analytics and respawns. This leaves no seam for health, block, parry or i-frames. | `DamageDetector.cs:95-122` [Code] | Blocks roadmap |
| A11 | **Death by `runtimeAnimatorController` swap.** The swap resets all Animator state and rebuilds the StateMachineBehaviour instances (see decision 01), and `Respawner` must restore the original controller. | `DamageDetector.cs:98-99`, `Respawner.cs:68` | Design |

## 2. Fighting-game conventions (reference model)
- Every attack has three phases: **startup**, **active** and **recovery**. A hitbox exists only during the active frames. Sources: [Capcom SF Seminar](https://game.capcom.com/cfn/sfv/column/131545?lang=en) [U, search-snippet], [Dustloop: Hitboxes](https://dustloop.com/w/Hitboxes) [U, search-snippet], [Amazon/GameMaker: Hitboxes and Hurtboxes](https://developer.amazon.com/docs/gamemaker/hitboxes-hurtboxes.html) [U, search-snippet].
- **Hitbox** means the offensive volume. **Hurtbox** means the vulnerable volume. Attacks often extend the attacker's own hurtbox (same sources).
- **Hitstop** is a freeze applied to both the attacker and the defender on contact. Capcom's seminar gives about 8 frames as a typical value. Hitstun and blockstun, compared against recovery, produce frame advantage, which is what makes combos and counters work (Capcom SF Seminar).
- Implication for Natureous: blocking, parry, air combos and magic all assume these primitives. Hitboxes are separate from hurtboxes, active windows are explicit, and each hit produces a hit event that can be **resolved** in several ways: hit, block, parry or whiff.

## 3. Options

| Option | How | Pros | Cons | Fit | Migration effort |
|---|---|---|---|---|---|
| **(a) Attacker-driven overlap queries** | Each attack defines hitbox shapes (offset, size, bone) in data. During active frames only, the attacker runs `Physics.OverlapBoxNonAlloc`/`OverlapSphereNonAlloc` with a `Hurtbox` layer mask and a preallocated buffer ([OverlapBoxNonAlloc](https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Physics.OverlapBoxNonAlloc.html) [V]: supported, non-allocating, takes a layer mask and `QueryTriggerInteraction`). A per-swing `HashSet<IDamageable>` hit registry stops multi-hits. | Deterministic. Cost scales with active attacks only. No reliance on enter/exit callbacks (fixes A8). Easy to draw with Gizmos. Fits projectiles and magic too (sweep or overlap per tick). | You author hitbox data per move. Fast swings can tunnel, which needs a sweep, a capsule between the previous and current position, or `Physics.CapsuleCast`. | **High.** This is the fighting-game standard. | M: about 2-4 days for the core plus moving existing moves over |
| **(b) Trigger callbacks on hurtbox components** | Hitbox colliders are triggers (kinematic Rigidbody) and are enabled during active frames. `OnTriggerEnter` calls `other.GetComponentInParent<IDamageable>()` or a cached `Hurtbox` component ([OnTriggerEnter](https://docs.unity3d.com/ScriptReference/Collider.OnTriggerEnter.html) [U]). Layer collision matrix: Hitbox × Hurtbox only. | Event-driven, no polling. Minimal code. Reuses the existing ragdoll colliders. | Timing is tied to `FixedUpdate`. Stays in contact never re-fire. Missing exit events on disable (A8). Ordering is less controllable. | Medium. Good stepping stone. | S: about 1-2 days |
| **(c) Damage/health pipeline** | `readonly struct DamageInfo {Attacker, Amount, HitPoint, Normal, BodyPart, Knockback, HitstunFrames, HitstopFrames, Flags (Unblockable, Launcher, Magic…)}`. `IDamageable.ReceiveHit(in DamageInfo) → HitResult {Hit, Blocked, Parried, Ignored}`. A `Health` component owns HP and death. A `Defense` component checks block or parry windows and i-frame flags *before* health. Hit reactions such as hitstun, knockback, launch and hitstop are applied by the reaction layer. | One seam for every planned feature. Testable in EditMode. Fixes A10. | New code paths. Animator states are needed for hit-react and block. | **Required** for any roadmap item | M: about 3-5 days |
| **(d1) Death = Animator state (same controller)** | Put death states in the main controller (a dedicated layer or sub-state machine). Pick the variant via an int parameter and `CrossFade`. | No controller reset. State is preserved for respawn. Works with A3-free logic. | A larger controller. | High | S |
| **(d2) Death = AnimatorOverrideController** | Keep one controller and override the death clip slots per body part or type ([AnimatorOverrideController](https://docs.unity3d.com/ScriptReference/AnimatorOverrideController.html) [U]). | Variety without graph bloat. | Assigning the override still re-binds the Animator, so it shares A11's reset caveat. | Medium | S |
| **(d3) Ragdoll activation** | On death, disable the Animator and enable the non-kinematic ragdoll, adding an impulse from `DamageInfo.Knockback` at `HitPoint`. | Physical, varied, reacts to hit direction. | Less authored control. Needs a ragdoll → get-up path, or a respawn reset. `Rigidbody.velocity` is renamed `linearVelocity` in Unity 6 (spike 06 item 3, [U]). | High as a finisher. The project already has ragdoll parts. | S-M |
| **(e) Events for UI/analytics/camera** | `Health` raises C# events (`Damaged`, `Died`). A small bridge forwards them to UI, analytics, camera shake and a ScriptableObject event channel for cross-scene listeners (Unity e-book, [Create modular game architecture with ScriptableObjects](https://unity.com/resources/create-modular-game-architecture-with-scriptable-objects-ebook) [U]). UnityEvents are only for designer wiring. | Removes the singleton calls from `TakeDamage`. Easier to test. | Subscription hygiene (unsubscribe in `OnDisable`). | High | S |

## 4. Recommendation
1. **Fix A1/A2/A3 first, or in the probe branch.** For A1, use `if (attack.MustCollide) { if (DidCollide) TakeDamage; } else if (sqrDist <= LethalRange*LethalRange) …`. For A3, match on `attackInfo.Attacker == thisCharacter && AttackAbility == this`, and move `FinishedAttacks` off the asset.
2. **Target architecture: (a) + (c) + (e), with (d1) for normal deaths and (d3) for finishers or heavy hits.**
   - `Hitbox` data lives on the move (the `Attack` SO keeps *data only*, per decision 05). A per-character `HitboxRunner` runs overlap queries in `FixedUpdate` only while the move is active. Active frames come from Animation Events or a frame range, not just normalizedTime polling.
   - `Hurtbox` components sit on the existing ragdoll colliders, on a dedicated layer. Each has a `BodyPart` enum and an owner `IDamageable`. This replaces names and `TriggerDetector` bookkeeping.
   - Data flows Hitbox → `DamageInfo` → target `Defense` (parry, block, i-frames) → `Health` → `HitReaction` (hitstop, hitstun, knockback, launch) → events.
   - Delete `AttackManager.CurrentAttacks` polling and the `DamageDetector.Update` loop.
3. **Interim option if time is short:** use (b) with hurtbox components and the layer matrix, keeping the pipeline (c). Move to (a) when air combos land, because they need precise active frames.

Interpretation (not sourced): the work order is (c) and (e) first, because every feature depends on them and they are cheap to test, followed by (a), then death variants.

## 5. Runnable probe (when the Unity 6 editor exists)
PlayMode test `DamageProbeTests`:
1. **A1:** spawn an attacker and a target 1.0 m apart with no trigger overlap (hurtbox layer disabled). Register an `AttackInfo` with `MustCollide=true, LethalRange=4`. Yield one frame and assert `DamageTaken == 0`. Expected on current code: fails (bug confirmed).
2. **A2:** set `MustCollide=false, LethalRange=1.5` with the target at 1.4 m. Assert a hit. Expected on current code: no hit, because the effective range is about 1.22 m.
3. **A3:** two characters run the same Attack asset with staggered timing. Assert that each `AttackInfo` is registered and finished only by its own attacker.
4. **Perf:** wrap `CheckAttack` in `ProfilerMarker("DamageDetector.CheckAttack")` with 10 targets and 5 active attacks, and compare against an OverlapBoxNonAlloc prototype using the same scene.
Budget: under 3 h.

## 6. Questions for you
1. **Health or one-hit kills?** Should players and enemies have HP (needed for combos and blocking), or stay one-hit with optional armor?
2. **Death style:** authored death animations, ragdoll, or both (animation normally, ragdoll for heavy or launcher finishers)?
3. **Precision vs authoring cost:** limb-accurate hitboxes that follow bones (more tuning), or simple per-move boxes in front of the character (classic 2.5D brawler)?
4. **Defense scope for v1:** block only, or block plus parry/deflect windows plus i-frames on dodge? This sets the size of the `Defense` component and the Animator work.

## Knowledge gaps
- The fighting-game sources were seen through search snippets only. The Infil glossary renders with JavaScript and could not be fetched.
- Unity 6.6-specific physics query changes were not checked beyond the 6000.2 OverlapBoxNonAlloc page.
- The Override-controller rebind cost in Unity 6 was not verified.
