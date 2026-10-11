# Natureous: Project Architecture Reference

**Type**: DIVIO Reference | **Audience**: LLM agents, architecture reviewers | **Updated**: 2026-10-11

This document captures the Natureous game as built: systems, wiring, asset structure, and invariants. Use this for understanding current implementation details, system dependencies, and code-location lookups.

---

## Timeline

From `git log origin/develop` (all 67 commits by Ahmed Ramy). All game work happened in **April 2020**; nothing after 2020-04-30 until docs on 2026-10-11.

| Date | Work (commit subjects) |
|------|------------------------|
| 2020-04-05 | Initial commit; "Setup Game from old project" |
| 2020-04-07 → 04-10 | Sprint, turn, stop, running jump, run/walk turn animations, hang from ledge |
| 2020-04-12 | Cliff climbing; ledge climb fix; delete redundant things |
| 2020-04-13 → 04-15 | Special attack foundation, hammer spawn + VFX; mobile input for sprint/climb; attack system improvements |
| 2020-04-17 → 04-18 | AI: dummies walk to start points, follow-player foundation, platform movement recording, navigation, falling, jump/climb, attack when close; player detector; **"Build first level (still need optimization)"**, **"try optimization technique"** |
| 2020-04-19 | Gate as last checkpoint, checkpoint system, fader, Game Over scene, **"Fix AI and optimize the game even more"** |
| 2020-04-20 | Fader when respawning |
| 2020-04-23 → 04-24 | GameAnalytics, Smartlook, platform-personalised hints, version bump to **0.2** |
| 2020-04-25 → 04-28 | Bug fixes |
| 2020-04-30 | Layer changer automator |

The `master`/`release` history ends 2020-04-28 ("Fix bugs"); `develop` adds 2020-04-30.

---

## Feature Inventory

| Feature | Status | Code Location | Scenes | Notes |
|---------|--------|---------------|--------|-------|
| **Movement (Walk/Run)** | Shipped | `States/Abilities States Scripts/{MoveForward,CheckMovement}` | Sandbox, Tutorial | Keyboard (WASD), Mobile (Joystick Pack) |
| **Jump & Fall** | Shipped | `States/Abilities States Scripts/{Jump,Landing,GroundDetector}` | Sandbox, Tutorial | Gravity pull mechanic in `Jump.cs` |
| **Sprint** | Shipped | `States/Abilities States Scripts/CheckSprint*` | Sandbox, Tutorial | Mobile sprint toggle in `MobileInput.cs` |
| **Melee Attack (Jab)** | Shipped | `States/Abilities States Scripts/Attack.cs` + `PooledObjects/AttackInfo/` | Sandbox, Tutorial | Pooled hitbox, combo-ready |
| **Charged Attack (Smash)** | Shipped | `States/Attacks/Player_Smash_*` (8 assets) | Sandbox, Tutorial | Gravity pull prep + multi-stage animation |
| **Special Attack (Running Hammer)** | Shipped | `States/Attacks/Player_RunningHammerDown_*` (10 assets) | Sandbox, Tutorial | Sprint+Attack combo |
| **Uppercut Attack** | Shipped | `States/Attacks/Player_Uppercut_*` (4 assets) | Sandbox, Tutorial | Jump+Attack combo |
| **Cliff Climbing** | Shipped | `States/Cliff Climbing/` (11 assets) + `LedgeChecker.cs` | Sandbox, Tutorial | Auto-detect ledge; hang → crouch transition |
| **Ledge Detection** | Shipped | `Characters/LedgeChecker.cs`, `OffsetOnLedge.cs`, `TeleportOnLedge.cs` | Sandbox, Tutorial | Edge-sphere raycast system |
| **Death & Ragdoll** | Shipped | `Characters/Death/`, `DeathAnimationsManager.cs`, `DamageDetector.cs` | Sandbox, Tutorial | Swap Animator + enable ragdoll colliders |
| **Damage Detection** | Shipped | `Characters/DamageDetector.cs` | Sandbox, Tutorial | Polls `AttackManager.CurrentAttacks`; checks ragdoll-part overlap |
| **AI Enemies** | Shipped | `States/AI/AIProgress.cs`, `PathFindingAgent.cs`, `PlayerDetector.cs` | Sandbox | Patrol, detect, chase, attack |
| **AI Attack** | Shipped | `States/Abilities States Scripts/Attack.cs` (shared) | Sandbox | Shares attack pooling with player |
| **Checkpoint & Respawn** | Shipped | `Core/{Checkpoint,Respawner}.cs`, `CheckpointManager.cs` | Sandbox, Tutorial | Last checkpoint triggers respawn on death |
| **Portal & Level End** | Shipped | `Objects/Portal/Portal.cs`, `SceneFader.cs` | Sandbox, Tutorial | Fade + load next scene |
| **Camera Follow** | Shipped | `Camera/CameraController.cs`, Cinemachine | Sandbox, Tutorial | Virtual camera + post-processing |
| **Camera Shake** | Shipped | `States/Abilities States Scripts/ShakeCamera.cs`, `CameraManager.cs` | Sandbox, Tutorial | Triggered on attack hit |
| **Analytics (Game)** | Shipped | `Managers/AnalyticsManager.cs` | All | GameAnalytics SDK integration |
| **Session Recording** | Shipped | `Managers/AnalyticsManager.cs` | All | Smartlook session recorder (Trello: "Smartlook" ✅) |
| **Survey Open** | Shipped | `Managers/AnalyticsManager.cs` | Game Over Scene | Opens browser on level complete (Trello: "Open browser on Survey when game ends" ✅) |
| **Mobile Input** | Shipped | `InputDevices/MobileInput.cs`, Joystick Pack asset | All | Joystick + button; disables KeyboardInput on Android/iOS |
| **Scene Fade** | Shipped | `Scenes/Scenes Utilities/SceneFader.cs` | All scenes | Cross-fade on transition |
| **Health System** | Board Only | — | — | Trello: "Health system" (To Do); currently one-hit death |
| **Shop** | Board Only | — | — | Trello: "Shop" (To Do); no inventory/UI |
| **Score** | Board Only | — | — | Trello: "Score" (To Do); no counter |
| **Blocking** | Board Only | — | — | Trello: "Blocking system" (To Do) |
| **Double Jump** | Board Only | — | — | Trello: "Double Jump" (To Do) |
| **Magic Attacks** | Idea | — | — | Trello: "Magic attacks" (To Do); vision: air/water/fire bending |
| **Pet/Follower** | Idea | — | — | Trello: "Pet System / Followers System" (Ideas); not prioritized |
| **Skill Points** | Idea | — | — | Trello: "Skill points" (To Do) |
| **In-App Purchase** | Idea | — | — | Trello: "In-app purchases" (To Do) |
| **AI Intelligence Levels** | Idea | — | — | Trello: "Create AI Intelligence Levels" (Blocked); design ready, no code |
| **Elemental Abilities** | Idea | — | — | Trello: "Combat" card; not prioritized |
| **Intro Story Scene** | Idea | — | — | Trello: "Add an intro scene to the game which talks about the story a bit" (To Do) |

---

## System Map

### Input Pipeline

```
Hardware (Keyboard | Mobile Joystick)
    ↓
KeyboardInput.cs  OR  MobileInput.cs
    ↓
VirtualInputManager (Singleton)
  - Caches input state every frame
    ↓
ManualInput.cs (per-player) | AIProgress.cs (per-AI)
    ↓
CharacterControl.cs (input flags: MoveUp, MoveDown, MoveLeft, MoveRight, Jump, Attack, Sprint)
    ↓
StateData Abilities (read flags → set Animator parameters)
    ↓
Animator (state transitions)
```

**Files**:
- `InputDevices/KeyboardInput.cs` — Reads keyboard (WASD/Shift/Space/J); disables on Android/iOS
- `InputDevices/MobileInput.cs` — Reads Joystick Pack; attack button via `VirtualInputManager`
- `Managers/VirtualInputManager.cs` — Singleton; global input buffer
- `Managers/ManualInput.cs` — Per-character; copies VirtualInputManager to CharacterControl

### Character State Machine

```
Animator (Visual State)
    ↓
CharacterState (StateMachineBehaviour on state)
    ├─ OnStateEnter() → call StateData[].OnEnter()
    ├─ OnStateUpdate() → call StateData[].UpdateAbility()
    └─ OnStateExit() → call StateData[].OnExit()
    ↓
StateData[] (Ability instances)
    ├─ Movement: MoveForward, Jump, Landing
    ├─ Combat: Attack, ForceTransition
    ├─ Detection: GroundDetector, LedgeChecker
    ├─ Physics: ToggleGravity, ToggleBoxCollider
    └─ Transitions: TransitionIndexer, Turn180
    ↓
CharacterControl.cs (hub for Rigidbody, edges, ragdoll)
    ├─ input flags
    ├─ Rigidbody + Gravity/Pull multipliers
    ├─ procedural edge-spheres (ground/edge detection)
    ├─ ragdoll parts (Collider list)
    └─ AnimationProgress + LedgeChecker + DamageDetector
```

**Files**:
- `States/Abilities States Scripts/CharacterState.cs` — StateMachineBehaviour
- `States/Abilities States Scripts/StateData.cs` — Abstract ability base
- `Controllers/CharacterController/CharacterControl.cs` — Character hub
- `Controllers/CharacterController/CharacterControl.cs` lines 8–19 — `TransitionParameter` enum

### Combat System

```
Player Input: Attack → CharacterControl.Attack = true
    ↓
Animator: transition to Attack state (e.g., LeadJab_Attack)
    ↓
Attack StateData (e.g., Player_LeadJab_Attack.asset):
    ├─ OnEnter() → Pool AttackInfo via PoolManager
    ├─ UpdateAbility() → Poll hitbox active window (AttackStartTime–AttackEndTime)
    ├─ Combo detection → Set Animator.SetBool(TransitionParameter.Attack, true)
    └─ OnExit() → Return AttackInfo to pool
    ↓
AttackManager.CurrentAttacks (active hitboxes)
    ↓
DamageDetector (per-target):
    ├─ Polls AttackManager.CurrentAttacks each frame
    ├─ Checks ragdoll-part colliders vs. attack colliders
    ├─ Matches against Attack.ColliderNames (e.g., "LeftHand")
    └─ On hit within LethalRange → Trigger death
    ↓
DeathAnimationsManager:
    ├─ Load death animator from Resources by DeathType
    ├─ Swap character's Animator
    ├─ CharacterControl.TurnOnRagdoll() → disable physics, enable ragdoll colliders
    ├─ CameraManager.ShakeCamera()
    └─ SceneFader.FadeOutCharacter() + Destroy
```

**Files**:
- `States/Abilities States Scripts/Attack.cs` — Ability class
- `Managers/AttackManager.cs` — Tracks active attacks
- `PooledObjects/AttackInfo/` — Pooled hitbox objects
- `Characters/DamageDetector.cs` — Damage polling
- `Managers/DeathAnimationsManager.cs` — Death animator swap
- `Managers/CameraManager.cs` — Camera shake
- `Scenes/Scenes Utilities/SceneFader.cs` — Death fade + destroy

### AI System

```
PlayerDetector (on trigger zone):
    ├─ Polls player position
    ├─ If within range → Instantiate AI prefab
    └─ Sets AIProgress.target
    ↓
AIProgress (per AI character):
    ├─ Tracks target, patrol waypoints, state flags
    └─ Accessed by StateData abilities
    ↓
Animator (same state machine as player)
    ↓
StateData Abilities:
    ├─ Movement: SendPathFindingAgent, StartWalking, JumpPlatform, FallPlatform
    ├─ Combat: Attack (pooled, same as player)
    └─ Detection: GroundDetector, LedgeChecker (same as player)
    ↓
PathFindingAgent:
    ├─ FindPath(target) → Populate waypoint queue
    └─ Abilities poll waypoints to move AI
```

**Files**:
- `States/AI/PlayerDetector.cs` — Spawns enemies
- `States/AI/AIProgress.cs` — AI state hub
- `Characters/PathFindingAgent/PathFindingAgent.cs` — Pathfinding
- `States/AI/Locomotion/` — AI-specific StateData subclasses

### Level Flow

```
Checkpoint (trigger zone on level):
    └─ Sets CheckpointManager.CurrentCheckpoint
    ↓
Player dies OR falls into abyss:
    ├─ Trigger death (above)
    ├─ Respawner teleports to CheckpointManager.CurrentCheckpoint
    ├─ AnalyticsManager logs death
    └─ SceneFader fades back in
    ↓
Player reaches Portal:
    ├─ Trigger Portal.cs
    ├─ SceneFader fades out
    ├─ Respawn at next level's first checkpoint
    └─ AnalyticsManager logs level complete + open survey
```

**Files**:
- `Core/Checkpoint.cs` — Checkpoint trigger
- `Managers/CheckpointManager.cs` — Singleton; stores current checkpoint
- `Core/Respawner.cs` — Teleport + fade on death
- `Objects/Portal/Portal.cs` — Level end trigger
- `Scenes/Scenes Utilities/SceneFader.cs` — Fade animation

### Analytics & Telemetry

```
AnalyticsManager (Singleton):
    ├─ Initialize GameAnalytics SDK (Awake)
    ├─ Initialize Smartlook session recorder (hardcoded Smartlook key)
    └─ On gameplay events:
        ├─ LogReachedCheckpoint(checkpoint) → GameAnalytics progression event
        ├─ LogPlayerDied() → GameAnalytics design event
        ├─ LogPlayerHasFallenIntoAbyss() → Progression fail event
        ├─ LogPlayerKilledAnEnemy() → Design event
        └─ LogGameOver() → Opens browser on survey link
```

**Files**:
- `Managers/AnalyticsManager.cs` — Telemetry hub
- Third-party: `GameAnalytics/`, `Smartlook/` (SDKs)

---

## Scenes & Build Order

**Build Index** (`ProjectSettings/EditorBuildSettings.asset`):

| Index | Name | Purpose | Content |
|-------|------|---------|---------|
| 0 | `_preload` | Boot scene | `ApplicationManager` (sets 60 FPS, loads scene 1) |
| 1 | `Sandbox` | Main level | Player, AI enemies, checkpoints, portal |
| 2 | `Game Over Scene` | End-of-level UI | Level complete display, survey button |

**Boot Sequence**:
1. Unity loads scene 0 (`_preload`)
2. `ApplicationManager.Awake()` sets `Application.targetFrameRate = 60` and calls `SceneManager.LoadScene(1)` (`Sandbox`)
3. `Tutorial Scene.unity` exists in `Scenes/` but is **not** in the build list
4. All singletons auto-initialize on first `.Instance` access

**Critical**: Always play from `_preload` in editor. Starting from another scene skips singleton init, breaking input and managers.

**Scene Files**:
- `Assets/Natureous/Scenes/_preload.unity` (index 0)
- `Assets/Natureous/Scenes/Sandbox.unity` (index 1)
- `Assets/Natureous/Scenes/Tutorial Scene.unity` (index 2)
- `Assets/Natureous/Scenes/Game Over Scene.unity` (index 3)

---

## Asset Conventions

### StateData Asset Naming

Pattern: `{Context}_{State}_{Ability}[_{Variant}].asset`

**Examples**:
- `General_Idle.asset` — Idle ability in general states
- `General_MoveForward.asset` — Move in general states
- `Player_LeadJab_Attack.asset` — Lead jab attack on player
- `Player_LeadJab_ForceTransition.asset` — Force exit after lead jab
- `Player_Uppercut_MoveForward.asset` — Move during uppercut (momentum)
- `Player_Smash_Prep_GravityPull.asset` — Extra gravity during prep
- `Player_RunningHammerDown_ShakeCamera.asset` — Shake on running hammer impact
- `Player_Jump_GroundDetector.asset` — Ground check during jump
- `Player_CrouchIdle_TransitionIndexer_1.asset` — Multi-way branch, option 1

**Folder Structure**:
```
Assets/Natureous/States/
├── General/ (8 assets)
│   ├── General_Idle.asset
│   ├── General_MoveForward.asset
│   ├── General_CheckMovement.asset
│   ├── General_CheckAttack.asset
│   ├── General_CheckSprint*.asset (2)
│   ├── General_Turn180.asset
│   └── Genera_GroundDetector.asset (typo in folder)
├── Movement/ (20+ assets)
│   ├── Player_Landing.asset
│   ├── Running/
│   │   ├── Player_CheckRunningTurn.asset
│   │   ├── Player_Running_TransitionIndexer_1.asset
│   │   ├── Player_RunToStop_*.asset (2)
│   │   ├── Player_RunToTurn180_*.asset (2)
│   │   ├── Player_Sprint_MoveForward.asset
│   │   ├── Player_Running_Jump.asset
│   │   └── Player_Jump_Running_*.asset (2)
│   └── Air Movements/
│       ├── Player_Jump.asset
│       ├── Player_Jump_*.asset (3 variants)
│       ├── Player_Fall_MoveForward.asset
│       └── Player_Jump_Landing_ForceTransition.asset
├── Attacks/ (40+ assets)
│   ├── Player_LeadJab_*.asset (3)
│   ├── Player_Uppercut_*.asset (5)
│   ├── Player_HammerDown_*.asset (4)
│   ├── Player_RunningHammerDown_*.asset (10)
│   ├── Player_Smash_*.asset (8)
│   └── Player_Walk_TransitionIndexer_1.asset
├── Cliff Climbing/ (11 assets)
│   ├── Player_LedgeGrab_*.asset (3)
│   ├── Player_HangingIdle_*.asset (1)
│   ├── Player_HangToCrouch_*.asset (1)
│   ├── Player_CrouchIdle_*.asset (5)
│   └── Player_*_TransitionIndexer_*.asset (1+)
├── AI/
│   └── Locomotion/ (4 assets)
│       ├── StartWalking.asset
│       ├── SendPathFindingAgent.asset
│       ├── JumpPlatform.asset
│       └── FallPlatform.asset
└── Death/ (1 asset)
    └── Player_SoccerTackleDeath_ForceTranstion.asset (typo: "Transtion")
```

**Total**: ~90 StateData `.asset` instances.

### Resources Folder

Pooled objects loaded by name via `PoolObjectLoader.InstantiatePrefab(PoolObjectType)`:

```
Assets/Resources/
├── AttackInfo/ (projectiles/hitbox prefabs)
├── Particles/ (VFX prefabs)
└── [Unknown others — check PoolObjectType enum]
```

**Note**: `PoolObjectType` is defined in `PooledObjects/PoolObjectLoader.cs` with values `AttackInfo`, `ThorHammer`, `HammerDownVFX`. `Resources/` folders: `PooledObjects/AttackInfo/Resources`, `Characters/Death/Resources`, `Characters/PathFindingAgent/Resources`.

### Prefabs

Character prefabs stored in `Assets/Natureous/Characters/` (names unknown; verify in Sandbox scene hierarchy).

AI enemy prefabs instantiated by `PlayerDetector.cs`; prefab names unknown.

---

## Third-Party Dependencies & SDKs

| Dependency | Version | Purpose | Code Integration | License |
|------------|---------|---------|-------------------|---------|
| **Unity URP** | 7.1.8 | Rendering pipeline | `Packages/manifest.json` | Built-in |
| **Cinemachine** | 2.5.0 | Virtual camera follow | `Camera/CameraController.cs` | Built-in |
| **TextMeshPro** | 2.0.1 | UI text rendering | `UI/GameOverScreenUIHandler.cs` | Built-in |
| **GameAnalytics SDK** | Unknown | Event tracking (progression, design events) | `Managers/AnalyticsManager.cs` | Third-party (proprietary) |
| **Smartlook SDK** | Unknown | Session video recording | `Managers/AnalyticsManager.cs` | Third-party (proprietary) |
| **Joystick Pack** | Unknown | Mobile joystick input | `InputDevices/MobileInput.cs` | Asset Store (likely free) |
| **PlayServicesResolver** | Unknown | Android API resolution | `Plugins/`, auto-linked | Google (built-in for Android) |
| **Roundbeargames Tutorial** | Unknown | Tutorial architecture reference | `Roundbeargames_Tutorial/` folder (do not modify) | Educational (third-party) |

**Location in project**: `Assets/` folders: `GameAnalytics/`, `Smartlook/`, `Joystick Pack/`, `PlayServicesResolver/`, `Plugins/`, `Roundbeargames_Tutorial/`. Do not modify third-party SDKs.

---

## Editor Tooling

### Custom Editors

| Script | Location | Purpose |
|--------|----------|---------|
| `CharacterControlEditor` | `Assets/Natureous/Controllers/CharacterController/Editor/CharacterControlEditor.cs` | Inspector customization for CharacterControl (unknown specifics) |
| `PathFindingAgentEditor` | `Assets/Natureous/Characters/PathFindingAgent/Editor/PathFindingAgentEditor.cs` | Gizmo debugging for AI pathfinding |
| `MaterialChangerEditor` | `Assets/Natureous/Utilities/Editor/MaterialChangerEditor.cs` | Material swap tool |
| `LayerChangerEditor` | `Assets/Natureous/Utilities/Editor/LayerChangerEditor.cs` | Layer management tool |
| `LayerAdderEditor` | `Assets/Natureous/Utilities/Editor/LayerAdderEditor.cs` | Layer auto-create tool |

**Usage**: Double-click a StateData `.asset` in Inspector to open its custom editor (if one exists). Run custom editors from Tools menu or right-click context.

### MaterialChanger & LayerChanger

**MaterialChanger.cs**: Swaps materials on skinned meshes. Used for protagonist minimalist shader vs. dummy NPC shader (Trello: "main character (the suited man) is now using a different minimalist shader").

**LayerChanger.cs** + **LayerAdderEditor.cs**: Auto-create and assign layers. Likely used for ground/edge detection layering.

---

## Known Hacks, TODOs, and Commented Code

### Found TODOs

| Location | Text | Status |
|----------|------|--------|
| `States/Abilities States Scripts/MoveForward.cs` line 42 | `// TODO: - remove comment when character has animationProgress` | Pending; animation progress partially implemented |
| `States/Abilities States Scripts/MoveForward.cs` line 114 | `// TODO: - remove comment when character has animationProgress` | Duplicate of above |

### Potential Issues

1. **Animator Parameter String Conversion**: `TransitionParameter` enum names must exactly match Animator controller parameter names. Mismatch = silent failure. No validation at compile time.

2. **Singleton FindObjectOfType Every Frame**: `Singleton<T>.Instance` calls `FindObjectOfType()` on every access. Expensive in hot paths (abilities, managers). Cache references:
   ```csharp
   PoolManager pool = PoolManager.Instance; // Cache this
   for(int i = 0; i < 100; i++) pool.GetObject(type); // Reuse
   ```

3. **DeathAnimationsManager Resources Lookup**: Death animation loaded by `DeathType` enum name from `Resources/`. If prefab name doesn't match enum, falls back to default (behavior unknown).

4. **Hardcoded Smartlook Key**: `AnalyticsManager.cs` Awake() contains hardcoded Smartlook recording key. Change when deploying to production.

5. **DamageDetector One-Hit Death**: Currently all damage triggers instant death. Health system not implemented.

6. **StateData Asset Serialization**: Changing StateData field types requires re-attaching `.asset` instances to Animator states. No migration tool.

### Commented Code

No widespread commented-out code found in main source (spot checks: `CharacterControl.cs`, `Attack.cs`, `AnalyticsManager.cs`). Likely clean codebase with removals committed.

---

## Invariants (Must Not Break)

### Enum-Animator Synchronization

**TransitionParameter enum** (`CharacterControl.cs` lines 8–19) must exactly match Animator controller parameter names:
- `Move` ↔ Animator bool "Move"
- `Jump` ↔ Animator bool "Jump"
- `ForceTransition` ↔ Animator bool "ForceTransition"
- `Landing` ↔ Animator bool "Landing"
- `Grounded` ↔ Animator bool "Grounded"
- `Attack` ↔ Animator bool "Attack"
- `Sprint` ↔ Animator bool "Sprint"
- `Turn` ↔ Animator bool "Turn"
- `TransitionIndex` ↔ Animator int "TransitionIndex"

Renaming either breaks state transitions silently.

### StateData Asset Naming & Paths

Changing `.asset` file names or moving folders breaks Animator state references. Editor shows "Missing (Script)" if `.asset` moves without updating Animator state references.

### Scene Build Index

`_preload` must remain at index 0; `ApplicationManager` loads scene 1. Reordering breaks boot sequence.

### Singleton Auto-Creation

All Managers inherit `Singleton<T>` and auto-create GameObject if missing. Explicit scene setup not required, but `DontDestroyOnLoad` persists across scenes. Test carefully on scene transitions.

### PoolObjectType Enum

`PoolObjectType` enum (`PooledObjects/PoolObjectLoader.cs`) names must match Resources/ prefab names exactly. Mismatch = instantiate from prefab instead of pooling.

### Ragdoll Parts Colliders

`DamageDetector` checks ragdoll-part collider names (e.g., "LeftHand", "RightFist") against `Attack.ColliderNames`. Renaming colliders breaks hit detection silently.

### Layer Assignments

Ground detection (`GroundDetector.cs`) and edge detection (procedural spheres) rely on LayerMask assignments. Changing layers without updating state data breaks collision.

---

## Meta Files & GUIDs

All `.meta` files for assets must be committed alongside `.asset` files. GUIDs in `.meta` files are immutable once created.

**Critical**: Never hand-edit `.meta` GUIDs. If `.meta` conflicts during merge:
1. Keep one version (either local or remote)
2. Let Unity re-import the asset
3. Reimport folder if GUID lookup fails

---

## Unity Version & Platform Target

- **Unity**: 2019.3.5f1 (specified in `ProjectSettings/ProjectVersion.txt`)
- **Render Pipeline**: Universal (URP 7.1.8)
- **Target Platforms**: Desktop (keyboard) + Mobile (iOS/Android via Joystick Pack)
- **Scripting Runtime**: .NET 4.x (default for 2019.3)

---

## Sources

- **Code**: `Assets/Natureous/` directory (commit history: `git log --stat`)
- **Scenes**: `Assets/Natureous/Scenes/`
- **Settings**: `ProjectSettings/`, `Packages/manifest.json`
- **Build Settings**: `ProjectSettings/EditorBuildSettings.asset`
- **Trello Board**: "First Game!" snapshot (2026-10-11) in `docs/trello/board.md`
- **Configuration Guide**: `CLAUDE.md` Architecture section
