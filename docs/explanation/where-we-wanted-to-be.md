# Where We Wanted to Be: Natureous Design Vision & Tradeoffs

**Type**: DIVIO Explanation | **Audience**: Future developers, design leads, architecture reviewers | **Updated**: 2026-10-11

This document explains the intended design for Natureous, the techniques chosen to build it, why those choices were made, and their tradeoffs. It captures the reasoning behind decisions so future work can build on or critique those foundations.

---


> **Provenance:** all the work happened in April 2020 (67 commits, one developer). No design notes from that time exist, so every "why" below is inferred from the code, commit messages and the Trello board, not recorded. Read "we chose X because Y" as "X fits Y".

## The Target Game vs. What Got Built

### Vision: Oceanhorn-Inspired Hack-and-Slash Adventure

The target was an **action-adventure game in the style of Oceanhorn**: a 2.5D, exploration-driven experience where the protagonist (Mino) grows progressively stronger by discovering and restoring nature monuments. Combat should feel visceral and fast-paced (hack-and-slash, not beat-em-up). The game world should transform visually as progress is made.

**Target Scope** (from Trello Board, "What We Agreed On"):
- 2.5D isometric perspective
- Hack-and-slash combat (preferred over beat-em-up)
- Story: Mino awakens powerless; restores monuments to unlock abilities
- A few major landmark maps (board: "Maybe another 3 maps will suffice") (each symbolizing real-world locations; Hiroshima Gate as example)
- Character progression through monument discovery
- Map transformation after completion (nature restored, humans removed)
- Minimalist protagonist shader; NPC dummies use different shader for readability

**Built Deliverable** (from code and Trello "Done Programming"):
- ✅ 2.5D playable level with platforming and combat
- ✅ Player character with movement, jump, sprint, melee attacks (jab, uppercut, hammer, running-hammer, smash)
- ✅ Cliff climbing (auto-detect ledge, hang, crouch transition)
- ✅ AI enemies that patrol, detect, chase, and attack
- ✅ Checkpoints and respawn system
- ✅ Portal to next level
- ✅ Camera follow and shake on impact
- ✅ Death animation with ragdoll
- ✅ Analytics tracking (progression, deaths, kills) + session recording
- ✅ Mobile controls (joystick + attack button)
- ✅ Release 0.2 shipped

**Not Yet Built** (Backlog, To Do / Ideas):
- ❌ Multiple landmark maps (1 level built; the board suggests about 3 more)
- ❌ Health system (one-hit death currently)
- ❌ Blocking, deflect, counter-attack
- ❌ Elemental combat (air/water/fire bending)
- ❌ Shop, skill points, in-app purchases, coin system
- ❌ Pet/follower system
- ❌ Character selection
- ❌ Intro story scene
- ❌ AI intelligence levels (design complete; no code)
- ❌ Melee weapon combos, double jump, air combos

**Gap Analysis**: The project built a feature-complete **first level proof-of-concept** (Release 0.2) with all core systems (input, state machine, combat, AI, analytics). It did not yet build the scope-expanding features (multiple maps, progression unlocks, advanced combat, economy). The architecture is designed to scale to these, but the backlog remains.

---

## Techniques Chosen & Design Decisions

### 1. Animator-Driven State Machine (Not Update Loops)

**What was chosen**: All gameplay logic is anchored to the Animator state machine. Abilities are `StateData` ScriptableObjects that execute callbacks on `OnStateEnter`, `OnStateUpdate`, `OnStateExit`. Character behavior emerges from composing abilities on Animator states.

**Alternative not chosen**: Traditional MonoBehaviour `Update` loops, where separate systems poll flags and drive state independently of animation.

**Why This Choice**: 

The core insight: **animation timing is non-negotiable in action games.** When an attack animation plays while logic runs independently, sync breaks—the visual punch connects before the hitbox appears, or the cancel window closes before the player can react.

By anchoring logic to Animator states, the team guaranteed that game logic fires when the animation is active. No desynchronization. No frame-rate-dependent bugs. A punch animation and its hitbox are locked together.

**Secondary benefit: Composition**. Instead of a monolithic character controller with nested `if` trees, each ability is a separate concern. `MoveForward` doesn't know about jumping; `Jump` doesn't know about attacking. They compose on shared states. Adding a new ability (e.g., "double jump") means:
1. Write a new `DoubleJump` StateData subclass
2. Serialize it as `.asset`
3. Attach to an Animator state
4. Done—no modifying other systems

This is how Roundbeargames Tutorial series structured it (the architectural reference).

**Tradeoff: Complexity**. State machine logic is harder to debug than linear flow. With Update loops, you read top-to-bottom. With state callbacks, you jump between OnEnter/Update/Exit. Debuggers show fragments, not the full picture. The team accepted this cost.

**Tradeoff: Animator Dependency**. All systems are bound to Unity Animator behavior. Porting to a different engine requires rewriting the architecture. The team chose deep Unity integration over portability.

### 2. Pooling for Combat Objects

**What was chosen**: Attack hitboxes are pooled. `PoolManager` maintains a pool of `AttackInfo` prefabs per `PoolObjectType`. When an attack state fires, it fetches a pooled object, reuses it, returns it. No garbage collection spikes.

**Alternative not chosen**: Instantiate/Destroy each attack, letting GC clean up.

**Why This Choice**:

Action games spawn hitboxes frequently (combo attacks, repeated strikes, VFX particles). Garbage collection pauses break frame rate and responsiveness. Pooling is the standard solution—reuse objects, avoid allocation.

The team integrated pooling into `PoolManager` (Singleton), accessible from any StateData ability. The cost is upfront: design `PoolObjectType` enum, create prefabs, load from Resources by name. The benefit is smooth, 60-FPS gameplay.

**Tradeoff: Setup Overhead**. Before an attack can pool, someone must:
1. Create the prefab
2. Add to `PoolObjectType` enum
3. Load from Resources/
4. Register with PoolManager

This is why the first level has 40+ attack `.asset` instances—each attack type was carefully pooled and tuned.

**Tradeoff: Resources Loading**. Pooled prefabs load from `Resources/`, which is bundled into the build. If an attack prefab is missing, fallback is instantiate-on-demand (expensive). No error warning.

### 3. Singletons with Auto-Creation

**What was chosen**: Core managers (`PoolManager`, `AttackManager`, `CheckpointManager`, `AnalyticsManager`, etc.) inherit `Singleton<T>`. Accessing `.Instance` calls `FindObjectOfType()` and auto-creates a GameObject if missing.

**Alternative not chosen**: Manual instantiation in scene; dependency injection.

**Why This Choice**:

Singletons allow systems to talk without scene setup. `Attack.cs` doesn't need a reference to `PoolManager`—it calls `PoolManager.Instance.GetObject()`. No wiring required. This was crucial for a small team with rapid iteration.

**Tradeoff: Performance**. `FindObjectOfType()` is O(n) and called on every access. Commit history suggests it was not fine even in the first level: "Build first level (still need optimization)", "try optimization technique" and "Fix AI and optimize the game even more" (2020-04-18/19). See `performance-bottlenecks.md`. As scope grows (multiple maps, more managers), this becomes a bottleneck. The recommendation: cache references in hot paths.

**Tradeoff: Global State**. Singletons are global state—hard to test in isolation, easy to create hidden dependencies. The codebase works but isn't unit-test friendly. Refactoring to dependency injection would require rewriting manager initialization.

### 4. Analytics-First Playtesting

**What was chosen**: Core gameplay events (checkpoint reach, death, enemy kill, abyss fall, survey open) are logged to GameAnalytics and Smartlook (session recording). These are integrated into every level, shipped with Release 0.2.

**Alternative not chosen**: Analytics as an afterthought; ship first, measure later.

**Why This Choice**:

The team wanted data-driven design from day one. With player session recordings + progression analytics, you can see:
- Where do players get stuck? (Deaths at checkpoint N)
- How long do levels take? (Progression events)
- Are they engaging the intended difficulty? (Combo usage, AI encounters)

This informs the next level design, combat balancing, and pacing.

**Tradeoff: Privacy & Overhead**. Session recording stores video. Player trust is required. The team accepted this for the sake of learning.

**Tradeoff: Hardcoded Keys**. Smartlook recording key is hardcoded in `AnalyticsManager`. Production deployment requires key rotation (currently not automated).

### 5. Roundbeargames Tutorial-Driven Architecture

**What was chosen**: The core architecture (Animator state machine + StateData abilities) is based directly on a Roundbeargames tutorial series (stored in `Assets/Roundbeargames_Tutorial/`—do not modify).

**Why This Choice**:

Rather than invent a bespoke architecture, the project followed a published tutorial series. This reduced design risk and gave the team a reference implementation to lean on. The architecture scales well for action games and is well-documented in the tutorial.

**Tradeoff: Lock-In**. The codebase is now committed to the Roundbeargames paradigm. Refactoring away from Animator-as-state-machine would be a massive undertaking.

---

## Design Tradeoffs: What We Accepted

### Tradeoff 1: Animation-First Gameplay vs. Physics-First Gameplay

**Choice**: Animation-first. Animator states drive logic.

**Alternative**: Physics-first. Rigidbody checks drive state.

**Why we chose animation-first**: Action games prioritize responsiveness and visual clarity. Players should see what the game is doing. Animation communicates intent. Binding logic to animation guarantees the visual is "correct."

**Cost**: Physics simulation is secondary. Collision detection for attacks is raycast + distance check, not true physics overlap. Ledge detection uses procedural sphere colliders, not physics queries. The trade is precision for performance and control.

---

### Tradeoff 2: Pooling Efficiency vs. Memory Simplicity

**Choice**: Aggressive pooling (attacks, VFX, AI spawning).

**Alternative**: Allocate/destroy freely; let GC handle it.

**Likely reason for pooling**: avoid instantiate/destroy cost and garbage during combat. The original reason isn't recorded anywhere.

**Cost**: Memory footprint is higher (pools pre-allocate). If a pool size is wrong, you either run out (stall on wait) or waste RAM. Manual tuning per pool type is required.

---

### Tradeoff 3: Tight Animator Coupling vs. Portable Architecture

**Choice**: Deeply integrated with Animator state machine.

**Alternative**: Decouple gameplay from Animator; use Animator purely for visuals.

**Why we chose coupling**: Sync guarantees. No other engine feature locks logic and animation tighter.

**Cost**: Porting to a different engine (Unreal, Godot) requires rewriting. The architecture is Unity-specific.

---

### Tradeoff 4: One-Hit Death vs. Health System

**Choice**: Current implementation: any damage = instant death (no health pool).

**Alternative**: Track health; trigger death at 0 HP.

**Why we're staying here for now**: MVP speed. Building a health UI, damage scaling, etc. adds scope. The team shipped without health to get feedback faster.

**Backlog**: "Health system" is in To Do. Next iteration will likely implement proper health.

---

### Tradeoff 5: Player Feedback via Analytics vs. Guesswork

**Choice**: Every gameplay event is logged and recorded.

**Alternative**: Assume we know what players need; skip telemetry.

**Why we chose telemetry**: Intuition fails. Analytics answer: "Is this attack too strong?" (kill rate), "Do players understand the jump mechanic?" (deaths at jump section). 

**Cost**: Privacy considerations; hardcoded API keys; SDK bloat.

---

## Lessons Carried Forward (Questions for Next Wave)

These questions frame the tradeoffs. They're not prescriptions to change code—they're hypotheses for the next design phase.

### Q1: Animator as Logic Layer — Still the Right Call?

**Current state**: Animator state machine is the source of truth. Works well for a tight, fast-paced combat loop.

**Question for next time**: As we add more systems (health, inventory, dialogue, map transitions), does Animator still scale? Or do we end up with a "kitchen sink" where every feature is a StateData subclass?

**What to watch**: If StateData classes start having 10+ fields and complex conditional logic, it's a sign the abstraction is breaking. Might be time to split concerns (e.g., health as a separate component).

### Q2: Pooling Granularity — Per Attack or Global?

**Current state**: Each attack type has its own pool. Reduces allocations.

**Question for next time**: As combat gets richer (combos, special effects, particles), will per-type pooling still be manageable? Or should we move to a global object pool with duck typing?

**What to watch**: If adding a new attack requires touching `PoolObjectType` enum and PoolManager setup, that's friction. Consider auto-pooling or a factory pattern.

### Q3: Singletons vs. Scene-Based Managers?

**Current state**: Managers are Singletons; they persist and auto-create.

**Question for next time**: As we add multiple maps and scene transitions, will Singleton lifetime management stay sane? Currently `DontDestroyOnLoad` persists across scenes—do we want managers to reset per level, or truly global?

**What to watch**: If managers need scene-specific state (e.g., current checkpoint changes per level), Singletons might conflict. Might need to split global (AnalyticsManager) from per-level (CheckpointManager).

### Q4: Analytics-First Design — Proving Value?

**Current state**: GameAnalytics + Smartlook integrated. We have session recordings and progression data.

**Question for next time**: Is the data actually driving decisions? Or is it just logged with no action? If it's the latter, maybe the team's research process doesn't align with telemetry.

**What to watch**: If no one is reviewing Smartlook videos or running GameAnalytics queries, telemetry is overhead, not insight. Might want to set up a review cadence (weekly playtest debrief) to make data actionable.

### Q5: Animator Coupling — Porting Risk?

**Current state**: Everything is tied to Animator. Tight integration.

**Question for next time**: If we ever need to port (Unreal, custom engine), how much rework? Could we extract a "state machine abstraction" that Animator implements, leaving room for other implementations?

**What to watch**: If porting becomes a real possibility, consider writing an adapter layer. Don't rewrite yet, but add seams for future extraction.

### Q6: One-Hit Death — When to Add Health?

**Current state**: No health system. Any damage = death.

**Question for next time**: When difficulty increases (more enemies, longer levels, advanced AI), will one-hit death feel punishing or exciting? If punishing, health system needs priority.

**What to watch**: Playtesting data. If players complain about "cheap kills" or feel underpowered, health + damage scaling become critical.

### Q7: AI Simplicity — Ready to Scale?

**Current state**: AI uses Animator same as player. Basic patrol + detect + attack.

**Question for next time**: Trello board proposes "AI Intelligence Levels" (Basic, Mid, Aggressive, Sentient). The design is documented but not coded. When implementing, does the StateData pattern handle level-based tuning, or do we need a separate AI config layer?

**What to watch**: If every AI level difference requires a new StateData subclass, that's explosion. Consider an AI config asset (difficulty, timings, ability set) separate from logic.

---

## How This Shaped the Build

### The Roundbeargames Influence

Roundbeargames tutorial (stored as reference in `Assets/Roundbeargames_Tutorial/`) demonstrated:
- StateMachineBehaviour (`CharacterState`) on every Animator state
- Scriptable ability pattern (StateData as data-driven "abilities")
- Input → VirtualInputManager → Character state
- Physics via Rigidbody, not Animator

The Natureous team adopted this wholesale. It accelerated development and provided a stable foundation.

### The First Level as Proof-of-Concept

Rather than building every map at once, the team focused on one level (Sandbox) with every core system (combat, AI, platforming, analytics). This let them:
- Ship something (Release 0.2)
- Gather data (session recordings, analytics events)
- Iterate on mechanics (attack timing, enemy behavior)
- Validate the architecture at real scope

The backlog (multiple maps, health, shop) is the next phase.

### Why Analytics-First Matters

By embedding telemetry from the start, the team can replay any player session and see exactly where they died, which attacks they used, how long they spent at each checkpoint. This data informs level design for maps 2–4. Without it, the team would guess about pacing and difficulty.

---

## Architectural Debt & Opportunities

### Technical Debt

1. **Animator Parameter Validation**: TransitionParameter enum names must match Animator parameter names. No compile-time check. A typo breaks silently.
   - **Mitigation**: Add Editor script to verify at build time.

2. **FindObjectOfType Performance**: Singleton.Instance calls FindObjectOfType every access. Fine for ~8 managers; breaks with many.
   - **Mitigation**: Cache Instance in hot paths. Consider manager pooling registry instead of FindObjectOfType.

3. **Hardcoded Smartlook Key**: API key in source code. Risky for production.
   - **Mitigation**: Move to app config or environment variable.

4. **StateData Asset Refactoring Friction**: Changing StateData field types requires re-attaching `.asset` instances.
   - **Mitigation**: Consider a serialization strategy (e.g., JSON config) with looser binding.

### Opportunities for Next Wave

1. **Ability Library**: Abstract common ability patterns (timers, animation-frame triggers, collision checks). Reduce StateData boilerplate.

2. **AI Config System**: Instead of per-level StateData, load AI difficulty from asset config (times, ability set, detection range).

3. **Combo System**: Build a "combo registry" so complex attack chains (jab → uppercut → smash) don't require manual Animator transitions.

4. **Map Progression**: Design a "checkpoint → portal → next map" system that's testable in isolation (currently tested manually in editor).

5. **Health & Damage Model**: Separate damage sources (melee, environmental) from health pool. Allow tuning without code changes.

---

## Design Philosophy Summary

**Natureous is built on three pillars**:

1. **Animation First**: Logic is anchored to visual state. This guarantees responsiveness.

2. **Composition Over Inheritance**: Abilities are small, single-concern pieces composed on states. Easy to test, easy to add.

3. **Data-Driven Iteration**: Analytics and playtest feedback drive design. Architecture is flexible enough to iterate on mechanics (speed, damage, combo timing) without code recompilation.

This philosophy worked for the first level. As scope expands to multiple maps, the team may revisit it—not to discard these pillars, but to refine how they scale.

---

## Sources

- **Trello Board "First Game!"**: Story, style, combat, maps, backlog
- **Code**: StateData pattern, Animator integration, manager structure
- **CLAUDE.md**: Architecture rationale
- **Roundbeargames Tutorial**: Reference architecture (do not modify)
- **Existing Docs**: game-vision.md, character-architecture.md
