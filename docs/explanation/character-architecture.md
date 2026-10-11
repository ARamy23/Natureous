# Character Architecture: State Machine Foundation

## Design Rationale

Natureous gameplay is driven by Unity's **Animator state machine**, not MonoBehaviour `Update` loops. This architecture decision addresses three core problems: **compositional ability design**, **animation-first gameplay**, and **visual-logic synchronization**.

### Why State Machine Over Update Loops?

**The Problem**: Traditional Update-based systems decouple animation timing from game logic. An attack animation plays while logic runs at a different rate, creating sync issues, hit detection problems, and animation canceling bugs.

**The Solution**: By anchoring all gameplay to Animator states, we guarantee that logic fires when the Animator state is active. No desynchronization. No frame-rate-dependent bugs.

**Trade-off**: State machine logic is harder to debug (nested state callbacks vs. linear flow). We accept this cost because animation timing is non-negotiable in action games.

### Compositional Ability System

Each Animator state is a container for a list of **StateData** ScriptableObjects — "abilities". When the Animator enters a state, all abilities in that state execute their `OnEnter` callback. On frame updates, their `UpdateAbility` runs. On state exit, `OnExit` fires.

**Why ScriptableObjects?**
- **Data-driven**: Designers create ability instances (`.asset` files) without code changes
- **Reusable**: Same ability (e.g., `MoveForward` at 5 units/sec) can attach to multiple states (walking, running, air recovery)
- **Testable**: Serialized parameters are visible in the Inspector; behavior can be tuned live

**Example**: The "Jumping" state contains three abilities:
1. `Jump` (applies upward force at animation keyframe)
2. `MoveForward` (applies horizontal velocity based on input)
3. `GroundDetector` (polls ground collision, triggers transition to Landing state)

When Jump state activates, all three run together. Animators never query C# state; state queries Animator.

---

## Architecture Components

### CharacterState (Orchestrator)

**File**: `Assets/Natureous/States/Abilities States Scripts/CharacterState.cs`

A `StateMachineBehaviour` attached to every Animator state. Holds a list of `StateData` instances. On state enter/update/exit, it forwards calls to all abilities.

```
CharacterState (on Animator state)
├─ StateData[] ListOfAbilityData
   ├─ Jump (applies force)
   ├─ MoveForward (applies velocity)
   └─ GroundDetector (polls collision, sets Animator parameter)
```

### StateData (Ability Base Class)

**File**: `Assets/Natureous/States/Abilities States Scripts/StateData.cs`

Abstract class defining the ability contract:

```csharp
public abstract class StateData : ScriptableObject {
  public abstract void OnEnter(CharacterState state, Animator animator, AnimatorStateInfo info);
  public abstract void UpdateAbility(CharacterState state, Animator animator, AnimatorStateInfo info);
  public abstract void OnExit(CharacterState state, Animator animator, AnimatorStateInfo info);
}
```

**Why abstract?** Enforces consistent lifecycle. Every ability must implement all three callbacks.

### CharacterControl (Character Hub)

**File**: `Assets/Natureous/Controllers/CharacterController/CharacterControl.cs`

Monobehaviour on the character's root. Holds:
- **Input state**: `bool MoveUp, MoveDown, MoveLeft, MoveRight, Jump, Attack, Sprint`
- **Physics**: `Rigidbody`, `GravityMultiplier`, `PullMultiplier`, extra gravity/pull application
- **Detection**: `LedgeChecker`, `DamageDetector`, procedurally spawned edge-detection spheres
- **Ragdoll parts**: `List<Collider> RagdollParts` for death animation
- **Character manager**: `AIProgress` for AI, `AnimationProgress` for animation state

**Key method**: `GetCharacterControl(Animator)` — StateData calls this to access the character, avoiding direct MonoBehaviour references.

### TransitionParameter Enum

**File**: `Assets/Natureous/Controllers/CharacterController/CharacterControl.cs` (lines 8–19)

```csharp
public enum TransitionParameter {
  Move, Jump, ForceTransition, Landing, Grounded,
  Attack, Sprint, Turn, TransitionIndex,
}
```

These enum names **must exactly match Animator parameter names**. Abilities set parameters via:
```csharp
animator.SetBool(TransitionParameter.Jump.ToString(), true);
```

---

## Input Pipeline

### Flow

```
Keyboard/Mobile Input
     ↓
VirtualInputManager (singleton)
     ↓
ManualInput (per-character buffer)
     ↓
CharacterControl (input flags)
     ↓
StateData Abilities (read flags, set Animator parameters)
```

### KeyboardInput & MobileInput

- **KeyboardInput**: Reads WASD/Shift/Space/J, disables itself on Android/iOS
- **MobileInput**: Reads joystick + attack button (Joystick Pack asset)

Both write to `VirtualInputManager.Instance`.

### VirtualInputManager (Singleton)

**File**: `Assets/Natureous/Managers/VirtualInputManager.cs`

A global input buffer. Every frame, it reads all active input devices and caches the state.

### ManualInput

**File**: `Assets/Natureous/Managers/ManualInput.cs`

Per-character component. Each frame, copies flags from `VirtualInputManager` into the player character's `CharacterControl`.

AI characters skip this; they use `AIProgress` instead.

---

## AI System

### AIProgress

**File**: `Assets/Natureous/States/AI/AIProgress.cs`

Monobehaviour on AI characters. Holds AI-specific state (target, patrol path, detection flags). When an AI character enters certain Animator states, `StateData` abilities query `AIProgress` to make decisions.

### PathFindingAgent

**File**: `Assets/Natureous/Characters/PathFindingAgent/PathFindingAgent.cs`

Computes paths to targets. Abilities like `SendPathFindingAgent` invoke it to populate a waypoint queue.

### PlayerDetector

**File**: `Assets/Natureous/States/AI/PlayerDetector.cs`

Spawns enemies when the player enters a trigger zone. Reads player position; if within range, instantiates AI prefabs.

### AI Intelligence Levels

**Backlog status** (Trello): "Create AI Intelligence Levels"

Proposed four levels with differentiated behaviors:

| Level | Movement | Attacks | Blocking |
|-------|----------|---------|----------|
| **Basic** | Walk, Jump, Climb | 1 attack + long wait | Cannot block |
| **Mid** | Walk, Run, Jump | Moving/walking/running attacks + wait | Block (0.75s debounce) |
| **Aggressive** | Walk, Run, Jump | Moving/walking/running attacks, no wait | Block (0.5s debounce) |
| **Sentient** | All + fluidity | All attacks, perfect combos, no wait | Block (0.3s debounce) |

**No implementation yet** — architecture ready for StatData subclasses per level.

---

## Combat System

### Attack Flow

1. Player presses Attack → `CharacterControl.Attack = true`
2. Animator transitions to Attack state (e.g., "LeadJab_Attack")
3. `CharacterState.OnEnter` fires; `Attack` ability (StateData subclass):
   - Pools an `AttackInfo` from `PoolManager`
   - Registers it in `AttackManager.CurrentAttacks`
4. `Attack.UpdateAbility` runs each frame:
   - Checks animation time against `AttackStartTime` and `AttackEndTime`
   - If in active window, attack hitbox is "live"
   - Checks for combo input; if detected, sets `Animator.SetBool(TransitionParameter.Attack, true)` to chain
5. When animation frame leaves active window, attack is deregistered
6. `Attack.OnExit` clears state

**Files**:
- `Assets/Natureous/States/Abilities States Scripts/Attack.cs` — ability class
- `Assets/Natureous/Managers/AttackManager.cs` — tracks active attacks
- `Assets/Natureous/PooledObjects/AttackInfo/` — pooled attack data

### Damage Detection

When an attack is "live", `DamageDetector` on target characters polls `AttackManager.CurrentAttacks`. For each attack:
1. Checks if target's ragdoll part colliders overlap with attack source
2. Matches against `Attack.ColliderNames` (e.g., "LeftHand", "RightFist")
3. If hit and within `LethalRange`, applies death

**File**: `Assets/Natureous/Characters/DamageDetector.cs`

### Death

On hit:
1. `DamageDetector` loads appropriate death animation via `DeathAnimationsManager`
2. Swaps Animator to death state
3. `CharacterControl.TurnOnRagdoll()` disables normal physics, enables ragdoll
4. Camera shakes via `CameraManager`
5. Ragdoll fades away via `SceneFader`

**Files**:
- `Assets/Natureous/Managers/DeathAnimationsManager.cs`
- `Assets/Natureous/Characters/Death/` (death StateData scripts)
- `Assets/Natureous/Managers/CameraManager.cs`

---

## Level Flow

### Checkpoints

**File**: `Assets/Natureous/Core/Checkpoint.cs`

Trigger zones. When player enters, `CheckpointManager.CurrentCheckpoint` updates. On death, player respawns here.

### Respawn

**File**: `Assets/Natureous/Core/Respawner.cs`

On death or abyss fall, teleports player to `CheckpointManager.CurrentCheckpoint` position. Fades screen via `SceneFader`.

### Portals & Scene Transitions

**File**: `Assets/Natureous/Objects/Portal/Portal.cs`

End-level trigger. Fades to next scene using `SceneFader`.

---

## Analytics & Telemetry

**File**: `Assets/Natureous/Managers/AnalyticsManager.cs`

Wraps GameAnalytics and Smartlook SDKs. Tracks:
- Checkpoint reaches
- Deaths (with location)
- Enemy kills
- Game over
- Survey opens

StateData abilities and managers fire events; AnalyticsManager logs them.

---

## Object Pooling

**Rationale**: Attacks, VFX, and pooled objects are created frequently. Pooling avoids GC pressure.

### PoolManager

**File**: `Assets/Natureous/Managers/PoolManager.cs`

Singleton. On demand, loads a prefab from `Resources` by `PoolObjectType` name, instantiates copies, stores in pool. When requested, deactivates objects instead of destroying them.

### Attack Pooling

`Attack` ability fetches pooled `AttackInfo` each time the attack state enters. After the attack ends, the object is returned to pool (deactivated).

---

## Singletons

**File**: `Assets/Natureous/Managers/Singleton.cs`

Template class. `public static T Instance { get; }` calls `FindObjectOfType` on access. If not found, auto-creates a GameObject with the component.

**Warning**: `FindObjectOfType` every frame is expensive. Cache references in hot paths (abilities, managers, not per-frame queries).

**Classes using Singleton**:
- `VirtualInputManager`
- `PoolManager`
- `AttackManager`
- `CheckpointManager`
- `DeathAnimationsManager`
- `CameraManager`
- `CharacterManager`
- `AnalyticsManager`

---

## Scene Setup

**Boot sequence**:
1. `_preload` scene (index 0) loads first
2. `ApplicationManager` (in `_preload`) sets 60 FPS, loads scene 1
3. Scene 1 is typically `Sandbox` or `Game Over Scene`

**Important**: Always play from `_preload` in editor. Starting from another scene skips singleton initialization.

---

## Sources

- `CLAUDE.md` section "Architecture"
- Code inspection: CharacterControl, StateData, CharacterState, Attack, AIProgress, PoolManager
- Trello board "First Game!": AI Intelligence Levels card (unprioritized)
