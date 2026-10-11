# 06 — Damage detection by collider names; death by swapping the controller

## Decision
- **Attack registration.** `Attack.OnEnter` takes a pooled `AttackInfo`, `ResetInfo`s it and adds it to `AttackManager.Instance.CurrentAttacks`, a global `List<AttackInfo>` (`States/Abilities States Scripts/Attack.cs:23-38`; `Managers/AttackManager.cs:9`). `Register` copies `ColliderNames` and other settings once `normalizedTime` enters the window [`AttackStartTime`, `AttackEndTime`) (`Attack.cs:78-91`; `PooledObjects/AttackInfo/Resources/AttackInfo.cs:34-46`).
- **Contact tracking.** Ragdoll colliders are set as triggers, and `TriggerDetector` components are added in the editor (`CharacterControl.cs:84-102`, called from `CharacterControlEditor.cs:19`). `TriggerDetector.OnTriggerEnter/Exit` keeps `CharacterControl.CollidingBodyParts` up to date (`Characters/TriggerDetector.cs:29-76`).
- **Damage check.** On every frame, `DamageDetector.Update` loops over all current attacks (`Characters/DamageDetector.cs:21-69`). `DidCollide` matches `attackInfo.ColliderNames` against the colliding collider's `name` with `string.Equals`, and requires that collider's root to be the attacker (`:71-93`).
- **Death.** `TakeDamage` swaps in a death controller, `character.SkinnedMeshAnimator.runtimeAnimatorController = DeathAnimationsManager.Instance.GetAnimator(...)` (`DamageDetector.cs:98-99`), chosen randomly by `DeathType` and body part (`Managers/DeathAnimationsManager.cs:21-55`). It also disables colliders and gravity. `Respawner` restores the previous controller afterwards (`Core/Respawner.cs:68`). One hit kills (`DamageTaken > 0` guard, `DamageDetector.cs:97`).

## Goal behind it
- **Stated (Trello):** "characters now have damage detectors, and when they're hit, they die with animations" (`docs/trello/board.md:148`).
- **Inferred:** hits come from the attacker's real limb (for example a fist) touching the target's ragdoll part, with a death animation that depends on which body part was hit (`GeneralBodyPart`). The tutorial lineage is commit `85bdd52`. One-hit death is inferred to be MVP scope (`where-we-wanted-to-be.md` "Tradeoff 4").

## What it achieved / where it hurt
- **Achieved:** limb-accurate hits, body-part-specific deaths and a working combat loop against AI.
- **Hurt: per-frame cost.** Every character polls the global list on every frame, with nested loops over attacks, body parts, colliders and names (`performance-bottlenecks.md` Cascade 2). That doc's comparison counts are estimates, not measurements.
- **Hurt: fragile names.** Renaming a bone or collider in the rig silently breaks hits, and no validation exists.
- **Hurt: possible logic bug (static read, unverified).** The check is `if (attack.MustCollide && DidCollide(attack)) ... else { distance check }` (`DamageDetector.cs:57-67`). A `MustCollide` attack that did **not** collide falls through to the `LethalRange` distance check, so it can still kill by proximity.
- **Hurt: swapping the whole controller.** Death replaces the entire `RuntimeAnimatorController`, which resets Animator state, and then restores it on respawn. The `CharacterState` instances and the `GetCharacterControl` cache are re-created when that happens (see decision 01).

## Unity 6 status
The `RuntimeAnimatorController` swap API is unchanged (migration findings item 18). The `Rigidbody.velocity` rename affects `TurnOnRagdoll` (`CharacterControl.cs:122,130`; item 3). `FindObjectOfType` inside the singletons is obsolete (item 1). All of these are UNVERIFIED.

## Probe question
Can a `MustCollide` attack kill a target that is within `LethalRange` but has no colliding body parts?

**Probe method:** a PlayMode test that places two characters at a distance where `SqrMagnitude` ≤ `LethalRange` with no trigger overlap, registers a `MustCollide = true` `AttackInfo`, steps one frame, and asserts `DamageTaken == 0`. If the assertion fails, the bug is confirmed. Add a `ProfilerMarker("DamageDetector.CheckAttack")` to measure the cost while doing this.
**Budget:** under 2 h.

## Pending research
- Should hits be pushed by physics callbacks instead of polled by every target? For example, the attacker's hitbox `OnTriggerEnter` could notify the target's hurtbox component.
- Should hit and hurt matching use identity rather than names: layers or `LayerMask`, a `Hitbox`/`Hurtbox` component reference, or `Physics.OverlapSphere`/`OverlapBoxNonAlloc` during the attack window?
- Should attack data stay a pooled GameObject or become a plain data object (see decision 05)?
- Should deaths use Animator layers, override controllers (`AnimatorOverrideController`) or a Playables graph instead of swapping the whole controller?
- Is a health component the right moment to split damage out of the Animator layer (`where-we-wanted-to-be.md` Q1)?

## Verdict
**REPLACE-CANDIDATE** for the name-string matching and the per-frame polling. **REVISIT** for death by controller swap. Both are provisional, pending the probe.

## Addendum (orchestrator review, 2026-10-11)

Both bugs were confirmed by reading `Assets/Natureous/Characters/DamageDetector.cs:57-67`:
- `if (attack.MustCollide && DidCollide(attack)) … else { distance check }`: a `MustCollide` attack that misses falls into the `else` branch and can still kill by distance.
- The distance check compares `Vector3.SqrMagnitude(...)` against `attack.LethalRange`, so the effective range is `sqrt(LethalRange)`, not `LethalRange`. Add this to the probe.
