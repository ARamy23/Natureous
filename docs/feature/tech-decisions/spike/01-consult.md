# 01 — Consult: Is "gameplay inside Animator states and abilities as ScriptableObjects" the right approach?

Companion to `01-animator-as-logic.md`. This is research only, with no code changes. Date: 2026-10-11. Target: Unity 6000.6.5f1.

Evidence labels:
- **[V]** I fetched and confirmed the source this session.
- **[S]** I saw the source in search results but did not read it in full.
- **[U]** Unverified. This is from my knowledge or from a URL I did not fetch. Check it before relying on it.
- **[I]** My interpretation.

## TL;DR
The pattern is a good *prototype* architecture, and it is a weak *combat* architecture. The plan is to keep the Animator for visuals and blending, and to move gameplay authority into a code-owned, frame-counted state machine driven by move-data ScriptableObjects (options b + d). Do this incrementally, starting with combat and leaving locomotion and ledge states alone for now. Decide on Animancer (c) once you know whether you want to drop the Animator graph entirely.

## 1. Audit

### Strengths (real, keep them)
- **Designer-friendly composition.** Behaviour is assembled by dropping `StateData` assets into a state's `ListOfAbilityData` (`CharacterState.cs:10`). That is low-code iteration, and it delivered locomotion, ledge climbing, 5 attack chains and AI locomotion in about a month (`01-animator-as-logic.md`).
- **Data-driven abilities.** Tunables such as `AttackStartTime`, `AttackEndTime`, `MaxHits` and `LethalRange` live in assets (`Attack.cs:10-19`). This idea is worth keeping in any option.
- **Animation sync for free.** Hit windows read `stateInfo.normalizedTime` (`Attack.cs:54,80,95`), so the logic cannot drift from the clip.

### Problems
| # | Problem | Evidence | Severity for a fighter |
|---|---|---|---|
| P1 | **Shared mutable state in ScriptableObjects.** `Attack.FinishedAttacks` is a private list on an asset (`Attack.cs:21`). Every character using that asset shares it. `AttackInfo.AttackAbility == this` (`Attack.cs:87,102,121`) matches by *asset*. If the player and YBot run the same Attack asset at the same time, one character's `OnExit`/`ClearAttack` can remove or finish the other character's `AttackInfo`. | Code read [V]. `SuitedMan.controller` is shared by both prefabs (`01-animator-as-logic.md`). | **High.** This becomes a live bug as soon as enemies use the same moves. |
| P2 | **SMB per-instance cache.** `CharacterState.characterController` caches the first owner (`CharacterState.cs:41-51`). Unity *instantiates* StateMachineBehaviours per Animator at runtime, so this is probably fine. I could not confirm that from the Manual or `GetBehaviours` pages this session [U]. ScriptableObject `StateData` is **not** instantiated, which is the root of P1. | Unity docs fetched; they are silent on this point. | Medium. The existing probe covers it. |
| P3 | **String parameters.** `TransitionParameter.X.ToString()` appears at 34 call sites. Unity provides `Animator.StringToHash`, which "generates a parameter id from a string. Ids are used for optimized setters and getters" [V]. Renaming an enum value silently breaks the binding. | [Animator.StringToHash](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Animator.StringToHash.html) | Low for performance, medium for correctness. Cheap to fix. |
| P4 | **Hard to unit test.** Logic only runs when an Animator ticks, and it depends on singletons (`AttackManager.Instance`, `PoolManager.Instance`). EditMode tests are effectively impossible, and PlayMode tests are slow and flaky. | Code read [V]. CLAUDE.md says there are no tests or asmdefs. | High once cancels and parry exist. |
| P5 | **Logic hidden in editor graphs.** Transitions, conditions, exit times and ability lists live in `.controller` YAML. They cannot be reviewed in diffs, and merges conflict badly. [I] | `SuitedMan.controller` | Medium, and grows with the move count. |
| P6 | **Frame precision.** Windows are *normalized float time*, sampled on whichever frame the Animator updates (variable `Update`, or the physics step if Update Mode = Animate Physics [U]). There is no integer frame count, so "active frames 5-7" or "cancel on frames 10-14" cannot be expressed exactly. Results depend on the frame rate. Combo windows are derived by formula (`Attack.cs:54`, start + ⅓·len … end + ½·len) instead of being authored. Transition blending also means a state's `OnStateEnter`/`OnStateExit` overlap during crossfades. | [I], based on code | **High** for blocking, parry, counters and hitstop. |
| P7 | **Hitstop.** Freezing two characters for N frames means setting `animator.speed = 0`, which also freezes the logic, because the logic *is* the Animator. Then the hitstop counter itself must live somewhere outside the Animator. [I] | — | High for planned features. |
| P8 | **Input buffering and cancels as bools.** `SetBool(Attack, true)` inside a window (`Attack.cs:60`) is a 1-slot buffer with no priority. Deflect, block, special and air-combo cancels need a priority-ordered cancel table. [I] | — | High for planned features. |

## 2. Options

| Option | How | Pros | Cons | Fit (2.5D action/fighter) | Migration effort |
|---|---|---|---|---|---|
| **(a) Keep Animator-as-logic, but fix it** | Move per-use state off the SO into a per-character `AbilityRuntime` context (dictionary keyed by `StateData` on `CharacterControl`, or SMB-held state). Cache hashed params as `static readonly int`. Add asmdefs and PlayMode tests. | Smallest change. Keeps the designer workflow. Fixes P1-P3. | P4-P8 remain: still float time, crossfade overlap, hitstop entangled with logic, no cancel tables. | OK for a brawler-lite (Devil-May-Cry-lite without precise parry). Poor for a fighter. | **S**: about 1-3 days |
| **(b) Code-owned hierarchical FSM; Animator for visuals only** | A plain C# HFSM (Grounded/Airborne/Ledge/Hurt → Idle/Move/Attack/Block…) ticks in a fixed step (`FixedUpdate` or a custom 60 Hz accumulator). States call `animator.CrossFadeInFixedTime(hash, …)` / `Play` [U: [CrossFade](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Animator.CrossFadeInFixedTime.html)]. The Animator graph collapses to mostly transition-less states. | Testable in EditMode (pure C#). Logic is visible in diffs. One authority. Hitstop is just "don't tick the FSM, set `animator.speed=0`". | Loses some visual graph authoring. Must keep animation length and logic duration in sync by hand or with tooling. | **Good**, and the standard approach for action games [I]. | **M**: about 1-2 weeks for combat. Locomotion and ledge can migrate later. |
| **(c) Playables API / Animancer** | Drop the AnimatorController and play clips from code. The Playables API is built in [U: [Playables manual](https://docs.unity3d.com/6000.0/Documentation/Manual/Playables.html)]. Animancer (Kybernetik) is actively maintained: v8.2.x shipped Sept-Oct 2025 with Unity 6.2 fixes, and v8.3.0/8.3.1 shipped Mar-Apr 2026 with Unity 6.3 entity-ID updates [S: [v8.2](https://kybernetik.com.au/animancer/docs/changes/animancer-v8-2), [v8.3](https://kybernetik.com.au/animancer/docs/changes/animancer-v8-3), [GitHub issues](https://github.com/KybernetikGames/animancer/issues/463)]. Not confirmed on 6000.6 specifically. Animancer has built-in FSM and events on normalized time. | Animation becomes fully code-driven and pairs naturally with (b). Clip-time events per transition. Hitstop via state speed. No graph YAML. | Third-party dependency. Pro is paid; the Lite tier has limits [U]. Raw Playables is verbose. Must rebuild the 71-asset setup. | Very good as the *animation backend* for (b)+(d). | **M-L**: about 2-3 weeks, plus licence |
| **(d) Frame-data-driven combat** | `MoveData` SO: clip, total frames, startup/active/recovery ranges, hitbox/hurtbox per active frame, damage, hitstun, blockstun, hitstop frames, cancel windows `{fromFrame,toFrame, allowed: [Normal, Special, Jump, Parry], onHitOnly}`. The FSM counts **integer frames** at a fixed tick. Animation playback is normalized to the move's frame count. | This is how fighting games specify moves: startup/active/recovery, frame advantage, and cancel/confirm windows defined in frames [S: [SRK SFV glossary](https://srk.shib.live/w/Street_Fighter_V/Glossary), [Dustloop frame chart doc](https://dustloop.com/w/Module:FrameChart-Test/doc)]. Deterministic and testable ("a parry on frame 3 beats a move on active frame 6"). Keeps the data-driven SO idea. | Needs an authoring tool (an inspector timeline or scrubber) or designers will suffer. More upfront design. | **Best fit** for combos, cancels, block/parry and hitstop. | **M** on top of (b). The data model is about 3-5 days. Tooling is extra. |
| **(e) Unity 6 extras: Animation Rigging, DOTS/ECS** | Animation Rigging handles procedural IK constraints [U: [package docs](https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/index.html)]. ECS is a data-oriented runtime. | Rigging is nice for hand IK on ledges and aim/look-at. | Neither solves gameplay-state authority. ECS has no first-class skinned-animation pipeline for this use case [U], and it is overkill for a few characters. | Rigging is optional polish later. DOTS is **not relevant**. | n/a |

## 3. Recommendation

**Use (b) + (d): a code-owned, fixed-tick hierarchical state machine driven by frame-data `MoveData` ScriptableObjects, with the existing Animator kept as a visual-only player (`CrossFadeInFixedTime` with hashed IDs). Before anything else, ship (a)'s P1/P3 fixes as a hotfix.** Re-evaluate Animancer (c) as the playback backend once the Animator graph is mostly empty.

Reasoning [I]:
1. Every planned feature (block, deflect/counter, air combos, hitstop, magic cancels) needs **integer frame windows, priority cancel tables and a pausable logic clock**. P6-P8 cannot be fixed while the Animator *is* the clock.
2. This keeps what worked: assets stay data-driven (`MoveData` replaces `Attack` tunables) and designers still author in the inspector.
3. It unlocks EditMode tests of combat rules, which is the biggest risk reducer for a fighter.
4. It is incremental. `CharacterState`/`StateData` can coexist: locomotion and ledge states stay Animator-driven while an `ActionStateMachine` owns Attack/Block/Hurt. Migrate further only if it pays off.
5. Hold off on Animancer for now. It is a paid dependency, 6000.6 compatibility is unverified, and (b) works with the built-in Animator.

Hotfix (do regardless of the option chosen): make `Attack` stateless (move `FinishedAttacks` and the match key to the `AttackInfo` owner + instance ID), and replace `.ToString()` with cached `StringToHash` ints.

## 4. Runnable probe (once the Unity 6 editor exists)
Budget: 1 asmdef, 2 test files, about 3 h.
1. **P1 shared-SO bug (PlayMode).** Spawn `SuitedMan` and `YBot`. Force both into the same attack state on the same frame (`animator.Play(hash)`). Step until player `OnExit` fires while YBot is still active. Assert that YBot's `AttackInfo` is still in `AttackManager.CurrentAttacks` and not finished. Expected: **fails today**.
2. **P2 SMB instancing.** Assert that `!ReferenceEquals(player.GetBehaviours<CharacterState>()[0], ybot.GetBehaviours<CharacterState>()[0])` and that each `GetCharacterControl` returns its own root. Assert that the `StateData` entries **are** reference-equal, which documents P1.
3. **P6 frame jitter.** Run one attack at `Application.targetFrameRate` = 30, 60 and 144. Log the frame indices (relative to a fixed 60 Hz counter) where `RegisterAttack` first fires. Expected: different active-frame starts, which shows the windows depend on frame rate.
4. **(b)+(d) spike.** Write a 100-line `ActionStateMachine` + `MoveData` (startup 5 / active 3 / recovery 12, cancel window 10-14) ticking in `FixedUpdate` (`Time.fixedDeltaTime = 1/60`). Add an EditMode test: input on frame 11 → cancels, input on frame 15 → doesn't, hitstop of 8 freezes the frame counter. Proves testability without an Animator.

## 5. Knowledge gaps
- I could not confirm in the Unity 6 docs that StateMachineBehaviours are instantiated per Animator. The Manual and `GetBehaviours` pages are silent [V]. Probe 2 settles it.
- Animancer compatibility with 6000.6 specifically is unverified (the latest seen is v8.3.1 for 6.3). The kybernetik.com.au domain is not on the trusted list.
- I fetched no Unity-authored guidance recommending code-driven FSMs over the Animator for combat. Fit judgements are interpretation [I].
- The fighting-game frame-data sources are community wikis outside the trusted list (medium trust). The terminology is industry-standard, though.

## 6. Questions for you
1. **Precision target:** frame-exact, fighter-style (fixed 60 Hz, parry windows in frames, frame advantage matters), or action-game feel (DMC/Hollow Knight-ish, where roughly correct timing is fine)? This decides whether (d) is required or whether (a)+(b) is enough.
2. **Who authors moves:** you in code/inspector, or a non-programmer designer? This decides how much timeline/hitbox tooling to build, and whether Animancer's editor workflow is worth the licence.
3. **Do enemies use the same moveset assets as the player** (the shared `SuitedMan.controller` suggests yes)? If so, the P1 hotfix is urgent.
4. **Any online or replay ambitions?** If there is any chance of rollback netcode or replays, logic must be deterministic and fixed-tick from day one, which strongly favours (b)+(d).
