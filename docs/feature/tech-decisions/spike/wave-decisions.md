# SPIKE Decisions -- tech-decisions (consultations 01, 04, 06, 10)

Decided by the owner on 2026-10-11 after the four consultations. Probes still need the Unity 6.6 editor.

## Owner decisions

| Question | Answer |
|---|---|
| Combat precision | **Action-game feel** (Oceanhorn/Trine-like). Combos and blocking, timing "roughly right" is fine |
| Movement | **Plane + depth moments**: side-on play, but knockbacks, cutscenes and enemies may leave the plane |
| Health and death | **Health + hit reactions; animated deaths, ragdoll for heavy finishers** |
| Target | **Mid-range phones at 60 fps** (~2021+ Android, iPhone 12 class); desktop High tier |

## Resulting direction (provisional until probes run)

- **01 Animator as logic → REPLACE (combat first).** Code state machine owns gameplay; Animator plays clips via hashed IDs. Moves as data assets with startup/active/recovery windows and cancel windows. No fixed-tick determinism or rollback requirement, so windows may be time-based rather than frame-counted. Locomotion and ledge climbing migrate later.
- **04 Edge spheres → REPLACE.** Keep 3D Rigidbody (depth moments rule out 2D physics). One downward `BoxCast` + one forward `CapsuleCast` with LayerMasks; movement via `linearVelocity` in `FixedUpdate`; gravity as acceleration; ledges via chest/head casts; cast alone decides grounded.
- **06 Hits and death → REPLACE.** `DamageInfo` → `IDamageable` → defence (block first; parry/i-frames later) → health → hit reaction. Attacker-owned overlap queries during active windows, hurtboxes on a dedicated layer, per-swing hit set. Death as states in the main controller; ragdoll on heavy finishers. Fix bugs A1–A3 first if the old system must keep running during migration.
- **10 Render and camera → KEEP URP.** Forward+ with Render Graph; tiers Mobile Medium (60 fps target, hard shadows, short shadow distance, HDR off) and Desktop High. Lock the **player** Rigidbody to the play plane; let enemies unlock the plane per state (knockback into depth). Cinemachine 3.1.x pinned; camera bounds must allow depth moments.

## Probe status

All probes defined in `01/04/06/10-consult.md`; none run. Blocked on installing editor 6000.6.5f1.

## Next wave

DESIGN (nw-solution-architect) for the combat core: state machine + move data + damage pipeline, using these decisions as constraints.
