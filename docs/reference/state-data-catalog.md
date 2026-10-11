# StateData Catalog

## TransitionParameter Enum

**Location**: `Assets/Natureous/Controllers/CharacterController/CharacterControl.cs` (lines 8–19)

Animator parameter names that StateData abilities can set. Names must match exactly in the Animator Controller.

| Parameter | Type | Purpose |
|-----------|------|---------|
| `Move` | bool | Player moving left/right/up/down |
| `Jump` | bool | Jump input pressed |
| `ForceTransition` | bool | Force state exit (used by transition indexers) |
| `Landing` | bool | Just landed from jump/fall |
| `Grounded` | bool | Character in contact with ground |
| `Attack` | bool | Attack button pressed or combo ready |
| `Sprint` | bool | Sprinting |
| `Turn` | bool | Turning 180 degrees |
| `TransitionIndex` | int | Indexed transition (for multi-way branches) |

---

## General Abilities

### Idle

**Class**: `Assets/Natureous/States/Abilities States Scripts/Idle.cs`

**Instance**: `Assets/Natureous/States/General/General_Idle.asset`

**Behavior**: Do nothing. Placeholder ability for states with no active logic.

**Serialized Fields**: None

---

### MoveForward

**Class**: `Assets/Natureous/States/Abilities States Scripts/MoveForward.cs`

**Instances**: 
- `Assets/Natureous/States/General/General_MoveForward.asset`
- `Assets/Natureous/States/Movement/Air Movements/Player_Fall_MoveForward.asset`
- `Assets/Natureous/States/Movement/Air Movements/Player_Jump_MoveForward.asset`

**Behavior**: Applies horizontal velocity based on movement input.

**Serialized Fields**:
- `float Speed` — units per second in movement direction
- `float TurnSpeed` — rotation speed when changing direction (0 = instant)

---

### GroundDetector

**Class**: `Assets/Natureous/States/Abilities States Scripts/GroundDetector.cs`

**Instance**: `Assets/Natureous/States/General/Genera_GroundDetector.asset`

**Behavior**: Polls ground collision via procedural edge-sphere colliders. Sets `TransitionParameter.Grounded` when floor is detected.

**Serialized Fields**:
- `float GroundDragDistance` — distance to poll downward
- `LayerMask GroundLayers` — which layers count as ground

---

### Turn180

**Class**: `Assets/Natureous/States/Abilities States Scripts/Turn180.cs`

**Instance**: `Assets/Natureous/States/General/General_Turn180.asset`

**Behavior**: Rotates character Y-axis 180 degrees (flips facing).

**Serialized Fields**:
- `float TurnSpeed` — rotation speed (degrees per second)

---

### CheckMovement

**Class**: `Assets/Natureous/States/Abilities States Scripts/CheckMovement.cs`

**Instance**: `Assets/Natureous/States/General/General_CheckMovement.asset`

**Behavior**: Reads `CharacterControl.Move*` flags; sets `TransitionParameter.Move` bool.

**Serialized Fields**: None

---

### CheckAttack

**Class**: `Assets/Natureous/States/Abilities States Scripts/CheckAttack.cs`

**Instance**: `Assets/Natureous/States/General/General_CheckAttack.asset`

**Behavior**: Reads `CharacterControl.Attack` flag; sets `TransitionParameter.Attack` bool.

**Serialized Fields**: None

---

### CheckSprint

**Class**: `Assets/Natureous/States/Abilities States Scripts/CheckSprint.cs`

**Instances**:
- `Assets/Natureous/States/General/General_CheckSprint.asset`
- `Assets/Natureous/States/General/General_CheckSprint_RequireMovement.asset`

**Behavior**: Reads `CharacterControl.Sprint` flag; may require movement input. Sets `TransitionParameter.Sprint`.

**Serialized Fields**:
- `bool RequireMovement` — if true, only set Sprint when moving

---

### CheckSprintAndMovement

**Class**: `Assets/Natureous/States/Abilities States Scripts/CheckSprintAndMovement.cs`

**Instance**: `Assets/Natureous/States/General/General_CheckSprintAndMovement.asset`

**Behavior**: Combines sprint and movement checks.

**Serialized Fields**: None

---

## Movement Abilities

### Jump

**Class**: `Assets/Natureous/States/Abilities States Scripts/Jump.cs`

**Instance**: `Assets/Natureous/States/Movement/Air Movements/Player_Jump.asset`

**Behavior**: Applies upward Rigidbody force at specified animation timing. Tracks jump state in `AnimationProgress.hasPlayerJumped`.

**Serialized Fields**:
- `float JumpTiming` — normalized animation time (0–1) when to apply force
- `float JumpForce` — force magnitude (units in Rigidbody mass scale)
- `AnimationCurve Pull` — gravity multiplier over animation duration

---

### Landing

**Class**: `Assets/Natureous/States/Abilities States Scripts/Landing.cs`

**Instance**: `Assets/Natureous/States/Movement/Player_Landing.asset`

**Behavior**: Clears jump state on landing.

**Serialized Fields**: None

---

### CheckRunningTurn

**Class**: `Assets/Natureous/States/Abilities States Scripts/CheckRunningTurn.cs`

**Instance**: `Assets/Natureous/States/Movement/Running/Player_CheckRunningTurn.asset`

**Behavior**: Detects if player should turn while running (movement direction opposes facing). Sets `TransitionParameter.Turn`.

**Serialized Fields**: None

---

## Attack Abilities

### Attack

**Class**: `Assets/Natureous/States/Abilities States Scripts/Attack.cs`

**Instances**: Multiple per attack type:
- `Player_LeadJab_Attack.asset`
- `Player_Uppercut_Attack.asset`
- `Player_HammerDown_Attack.asset`
- `Player_RunningHammerDown_Attack.asset`
- `Player_Smash_Landing_Attack.asset`

**Behavior**: Pools an `AttackInfo`, registers with `AttackManager`, polls hitbox active window, detects combos.

**Serialized Fields**:
- `float AttackStartTime` — normalized animation start of hitbox active window
- `float AttackEndTime` — normalized animation end of hitbox active window
- `List<string> ColliderNames` — ragdoll part names to check collisions ("LeftHand", "RightFist", etc.)
- `DeathType DeathType` — death animation to play on hit target
- `bool MustCollide` — if true, attack fails if no collision
- `bool MustFaceTheAttacker` — if true, only hit if target faces attacker
- `float LethalRange` — maximum distance to apply damage
- `int MaxHits` — how many targets can this attack hit

---

### ForceTransition

**Class**: `Assets/Natureous/States/Abilities States Scripts/ForceTransition.cs`

**Instances**:
- `Player_LeadJab_ForceTransition.asset`
- `Player_Uppercut_ForceTransition.asset`
- `Player_RunningHammerDown_ForceTransition.asset`

**Behavior**: Forces animator to exit current state (sets `TransitionParameter.ForceTransition`) after a delay.

**Serialized Fields**:
- `float TimeToForceTransition` — seconds before forcing exit

---

### MoveForward (Attack Variant)

**Instances**:
- `Player_LeadJab_MoveForward.asset`
- `Player_Uppercut_MoveForward.asset`
- `Player_HammerDown_MoveForward.asset`

**Behavior**: Same as general MoveForward; used to add directional momentum during attacks.

**Serialized Fields**: `float Speed`, `float TurnSpeed`

---

### ShakeCamera

**Class**: `Assets/Natureous/States/Abilities States Scripts/ShakeCamera.cs`

**Instances**:
- `Player_HammerDown_ShakeCamera.asset`
- `Player_RunningHammerDown_ShakeCamera.asset`
- `Player_Smash_Landing_ShakeCamera.asset`

**Behavior**: Triggers camera shake when attack connects.

**Serialized Fields**:
- `float ShakeDuration` — seconds
- `float ShakeMagnitude` — intensity

---

### SpawnObject

**Class**: `Assets/Natureous/States/Abilities States Scripts/SpawnObject.cs`

**Instances**:
- `Player_HammerDown_SpawnObject.asset`
- `Player_HammerDown_SpawnObject_VFX.asset`
- `Player_RunningHammerDown_SpawnObject.asset`
- `Player_RunningHammerDown_SpawnObject_VFX.asset`

**Behavior**: Spawns a VFX or impact object during attack.

**Serialized Fields**:
- `float SpawnTime` — normalized animation time when to spawn
- `string ObjectName` — prefab name to pool
- `Vector3 SpawnOffset` — relative position from character

---

### GravityPull

**Class**: `Assets/Natureous/States/Abilities States Scripts/GravityPull.cs`

**Instance**: `Assets/Natureous/States/Attacks/Player_Smash_Prep_GravityPull.asset`

**Behavior**: Applies extra downward force (increased gravity) during prep phases.

**Serialized Fields**:
- `float PullMultiplier` — gravity increase factor

---

## Cliff Climbing Abilities

### ToggleBoxCollider

**Class**: `Assets/Natureous/States/Abilities States Scripts/ToggleBoxCollider.cs`

**Instances**:
- `Player_LedgeGrab_ToggleBoxCollider_Off.asset` (disables during ledge grab)
- `Player_CrouchIdle_TriggerBoxCollider_On.asset` (enables during crouch)

**Behavior**: Enables or disables character's main BoxCollider.

**Serialized Fields**:
- `bool EnableCollider` — true to enable, false to disable

---

### ToggleGravity

**Class**: `Assets/Natureous/States/Abilities States Scripts/ToggleGravity.cs`

**Instances**:
- `Player_LedgeGrab_ToggleGravity_Off.asset`
- `Player_CrouchIdle_TriggerGravity_On.asset`

**Behavior**: Toggles Rigidbody.useGravity.

**Serialized Fields**:
- `bool UseGravity` — true to enable gravity, false to disable

---

### OffsetOnLedge

**Class**: `Assets/Natureous/States/Abilities States Scripts/OffsetOnLedge.cs`

**Instance**: `Player_LedgeGrab_OffsetOnLedge.asset`

**Behavior**: Adjusts character position to align with ledge surface.

**Serialized Fields**:
- `Vector3 OffsetPosition` — adjustment relative to ledge

---

### TeleportOnLedge

**Class**: `Assets/Natureous/States/Abilities States Scripts/TeleportOnLedge.cs`

**Instance**: `Player_HangToCrouch_TeleportOnLedge.asset`

**Behavior**: Teleports character to ledge position when transitioning from hang to crouch.

**Serialized Fields**:
- `Vector3 TeleportOffset` — position adjustment

---

### TransitionIndexer

**Class**: `Assets/Natureous/States/Abilities States Scripts/TransitionIndexer.cs`

**Instances**: Multiple per state (numbered 1–4):
- `Player_LedgeGrab_TransitionIndexer_1.asset`
- `Player_HangingIdle_TransitionIndexer_1.asset`
- `Player_CrouchIdle_TransitionIndexer_[1-4].asset`
- `Player_Running_TransitionIndexer_1.asset`
- `Player_Walk_TransitionIndexer_1.asset`

**Behavior**: Sets `TransitionParameter.TransitionIndex` to a specific int value based on input direction or condition. Used for multi-way branches.

**Serialized Fields**:
- `int TransitionIndexValue` — value to set
- Condition logic (varies by instance)

---

## AI Abilities

### StartWalking

**Class**: `Assets/Natureous/States/AI/Locomotion/Locomotion State Scripts/StartWalking.cs`

**Instance**: `Assets/Natureous/States/AI/Locomotion/StartWalking.asset`

**Behavior**: Initiates walking by reading `AIProgress` state and setting movement direction.

**Serialized Fields**:
- `float WalkSpeed` — movement speed

---

### SendPathFindingAgent

**Class**: `Assets/Natureous/States/AI/Locomotion/Locomotion State Scripts/SendPathFindingAgent.cs`

**Instance**: `Assets/Natureous/States/AI/Locomotion/SendPathFindingAgent.asset`

**Behavior**: Invokes `PathFindingAgent.FindPath()` to generate waypoint queue to target.

**Serialized Fields**: None

---

### JumpPlatform

**Class**: `Assets/Natureous/States/AI/Locomotion/Locomotion State Scripts/JumpPlatform.cs`

**Instance**: `Assets/Natureous/States/AI/Locomotion/JumpPlatform.asset`

**Behavior**: Applies jump force for platform crossing.

**Serialized Fields**:
- `float JumpForce` — force magnitude

---

### FallPlatform

**Class**: `Assets/Natureous/States/AI/Locomotion/Locomotion State Scripts/FallPlatform.cs`

**Instance**: `Assets/Natureous/States/AI/Locomotion/FallPlatform.asset`

**Behavior**: Tracks falling state during platform fall.

**Serialized Fields**: None

---

## Death Abilities

### ForceTransition (Death Variant)

**Instance**: `Player_SoccerTackleDeath_ForceTranstion.asset`

**Behavior**: Same as attack ForceTransition; used to exit death state.

**Serialized Fields**: `float TimeToForceTransition`

---

## StateData Asset Folders

Instances are organized by functional group:

| Folder | Purpose | Count |
|--------|---------|-------|
| `Assets/Natureous/States/General/` | Idle, movement checks, detection | 8 assets |
| `Assets/Natureous/States/Movement/` | Jumping, landing, running, air moves | 14 assets |
| `Assets/Natureous/States/Movement/Air Movements/` | Airborne-specific abilities | 7 assets |
| `Assets/Natureous/States/Movement/Running/` | Running turns, running jumps | 6 assets |
| `Assets/Natureous/States/Attacks/` | Attack definitions and FX | 30+ assets |
| `Assets/Natureous/States/Cliff Climbing/` | Ledge grab, hang, crouch transitions | 10 assets |
| `Assets/Natureous/States/Death/` | Death animations and cleanup | 1 asset |
| `Assets/Natureous/States/AI/Locomotion/` | AI pathfinding and jumping | 4 assets |

---

## Sources

- Code: `Assets/Natureous/States/Abilities States Scripts/` directory (all StateData subclasses)
- Code: `Assets/Natureous/States/AI/Locomotion/` directory
- Assets: `.asset` instances in `Assets/Natureous/States/` folders
- CLAUDE.md project section
