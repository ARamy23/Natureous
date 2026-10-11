# Performance Bottlenecks: The Singleton Pattern Cascade

## Why This Matters

Natureous experiences severe performance degradation even on powerful machines because of a fundamental architectural decision: the Singleton pattern as implemented does not cache retrieved instances. Every single access to a manager (`VirtualInputManager.Instance`, `AttackManager.Instance`, `CharacterManager.Instance`) triggers a full scene-wide object search via `FindObjectOfType`. Across a frame processing input, physics, state updates, and attack detection—this compounds into thousands of redundant searches per second.

This document explains the root cause, traces its impact through the codebase with evidence, and provides a framework for profiling to confirm bottleneck severity.

---

## Root Cause: The Singleton Implementation

### The Problem

**File**: `Assets/Natureous/Managers/Singleton.cs`  
**Lines 12-26**: The `Instance` property

```csharp
public static T Instance
{
    get
    {
        _instance = (T)FindObjectOfType(typeof(T));
        if(_instance == null)
        {
            GameObject obj = new GameObject();
            _instance = obj.AddComponent<T>();
            obj.name = typeof(T).ToString();
        }
        DontDestroyOnLoad(_instance);
        return _instance;
    }
}
```

**The issue**: Line 15 calls `FindObjectOfType` on **every single access**, even when `_instance` is already valid. The `_instance` field is set, but the next call still searches the entire scene. This is an architectural flaw where caching logic is inverted: the code *should* check if `_instance != null` first, then only search if it's null.

**Why this is expensive**:
- `FindObjectOfType<T>` is O(n) where n = total objects in scene
- Each call traverses the entire object hierarchy via `Object.FindObjectOfType` (unsafe C++ code)
- Unity's documentation (docs.unity3d.com) classifies it as a "heavy" operation; typical cost is 0.1–1ms per call on moderately populated scenes
- Cost scales with scene complexity (more objects = slower searches)

---

## Cascade 1: Input Processing (High Impact)

### How Often This Happens

**File**: `Assets/Natureous/InputDevices/KeyboardInput.cs`  
**Lines 17–26**: Update method

```csharp
void Update()
{
    VirtualInputManager.Instance.MoveUp = Input.GetKey(KeyCode.W);
    VirtualInputManager.Instance.MoveDown = Input.GetKey(KeyCode.S);
    VirtualInputManager.Instance.Sprint = Input.GetKey(KeyCode.LeftShift);
    VirtualInputManager.Instance.MoveRight = Input.GetKey(KeyCode.D);
    VirtualInputManager.Instance.MoveLeft = Input.GetKey(KeyCode.A);
    VirtualInputManager.Instance.Jump = Input.GetKey(KeyCode.Space);
    VirtualInputManager.Instance.Attack = Input.GetKey(KeyCode.J);
}
```

**Seven property accesses, each one finds the singleton**:  
7 × FindObjectOfType calls per frame, every frame. On a 60 FPS target, that's **420 searches per second** just for keyboard input.

**Impact**: **HIGH**  
**Type**: CONFIRMED in code | Per-frame cost
**Architectural issue**: Input is a frame-critical hot path; accessing it should be a property lookup (O(1)), not a scene search (O(n)).

---

### Compounded by MobileInput

**File**: `Assets/Natureous/InputDevices/MobileInput.cs`  
**Lines 39–47**: Update method

```csharp
void Update()
{
    VirtualInputManager.Instance.MoveUp = joystick.Vertical >= upSensitivity;
    VirtualInputManager.Instance.MoveDown = joystick.Vertical <= downSensitivty;
    VirtualInputManager.Instance.MoveRight = joystick.Horizontal >= sensitivty;
    VirtualInputManager.Instance.MoveLeft = joystick.Horizontal <= -sensitivty;
    VirtualInputManager.Instance.Jump = joystick.Vertical >= jumpSensitivity;
    VirtualInputManager.Instance.Sprint = Mathf.Abs(joystick.Horizontal) >= sprintSensitivty;
}
```

**Six more property accesses**. Both input systems are active conditionally (keyboard on desktop, mobile on mobile), but each duplicates the pattern.

---

### Replicated by ManualInput

**File**: `Assets/Natureous/Managers/ManualInput.cs`  
**Lines 17–27**: Update method

```csharp
void Update()
{
    characterControl.MoveRight = VirtualInputManager.Instance.MoveRight;
    characterControl.MoveLeft = VirtualInputManager.Instance.MoveLeft;
    characterControl.Jump = VirtualInputManager.Instance.Jump;
    characterControl.Attack = VirtualInputManager.Instance.Attack;
    characterControl.Sprint = VirtualInputManager.Instance.Sprint;
    characterControl.MoveUp = VirtualInputManager.Instance.MoveUp;
    characterControl.MoveDown = VirtualInputManager.Instance.MoveDown;
}
```

**Seven more reads** from `VirtualInputManager.Instance`. This is a relay that copies input to the player character.

**Cascade subtotal**: ≥ 420 FindObjectOfType calls per second just to read and relay input.

---

## Cascade 2: Attack Detection and Damage (High Impact)

### Nested Loop Multiplication

**File**: `Assets/Natureous/Characters/DamageDetector.cs`  
**Lines 21–27 and 29–69**: Update and CheckAttack methods

```csharp
private void Update()
{
    if (AttackManager.Instance.CurrentAttacks.Count > 0)
    {
        CheckAttack();
    }
}

private void CheckAttack()
{
    foreach(AttackInfo attack in AttackManager.Instance.CurrentAttacks)
    {
        // ... filter checks ...
        
        if (attack.MustCollide && DidCollide(attack))
            TakeDamage(attack);
        else
        {
            // distance check
        }
    }
} 

private bool DidCollide(AttackInfo attackInfo) 
{
    foreach (var collidingBodyPart in character.CollidingBodyParts)
    {
        foreach (var collider in collidingBodyPart.Value)
        {
            foreach (string name in attackInfo.ColliderNames)
            {
                if (name.Equals(collider.name))
                {
                    // ...
                }
            }
        }
    }
    return false;
}
```

**The structure**: 3–4 nested foreach loops for every active attack on every character.

**Per-frame cost breakdown**:
- Outer loop: number of active attacks (1–10+ during combat)
- Middle loop: body parts per character (varies, typically 5–20+ due to ragdoll setup)
- Inner loop: colliders per body part (2–5+)
- Innermost loop: collider names per attack definition (2–4+)

**Estimated worst case**: 10 attacks × 15 body parts × 3 colliders × 3 names = **1,350 string comparisons** per Update call, per character.

**With 3 active characters** (player + 2 enemies): **4,050 comparisons per frame**.

**Impact**: **HIGH**  
**Type**: CONFIRMED in code | Per-frame cost, algorithmic inefficiency
**GC allocation**: Each `name.Equals(collider.name)` call might allocate if collider names are stored as strings and compared repeatedly.

---

### AttackManager.Instance Accesses

Lines 23 and 31 call `AttackManager.Instance.CurrentAttacks` twice per DamageDetector frame.

With 3 characters × 2 checks = **6 FindObjectOfType calls per frame** for attack manager alone.

---

## Cascade 3: Ground Detection Raycasts (Medium Impact)

### Raycasts Per Character

**File**: `Assets/Natureous/States/Abilities States Scripts/GroundDetector.cs`  
**Lines 35–61**: IsGrounded method

```csharp
bool IsGrounded(CharacterControl character)
{
    float yVelocity = character.Rigidbody.velocity.y;
    if (yVelocity > -0.001f && yVelocity <= 0f)
    {
        return true;
    }

    if (yVelocity < 0f)
    {
        foreach (GameObject sphere in character.BottomSpheres)
        {
            RaycastHit hit;
            if (Physics.Raycast(sphere.transform.position, -Vector3.up, out hit, Distance))
            {
                if (!character.RagdollParts.Contains(hit.collider)
                    && !Ledge.IsLedge(hit.collider.gameObject)
                    && !Ledge.IsLedgeChecker(hit.collider.gameObject)
                    && !Ledge.IsCharacter(hit.collider.gameObject))
                    return true;
            }
        }
    }
    return false;
}
```

**The pattern**: 
- **Line 44**: `foreach (GameObject sphere in character.BottomSpheres)` — iterates over 6 bottom spheres (set up in `CharacterControl.SetupColliderSpheres`, lines 134–162)
- **Line 49**: Each sphere casts a raycast downward
- **Lines 51–54**: Each raycast hit is checked against multiple categories (ragdoll parts, ledges, other characters)

**Per-character cost when falling**:
- 10 raycasts per GroundDetector Update (when yVelocity < 0)
- Each raycast: O(n) physics query
- Each hit check: O(m) List.Contains calls on RagdollParts, plus three static method calls to Ledge

**Architectural issue**: Raycasting is expensive; doing 10+ per frame per character is high. A single capsule raycast or a single sphere overlap would be more efficient.

**Impact**: **MEDIUM–HIGH**  
**Type**: CONFIRMED in code | Per-frame physics cost
**Worst case**: 3 characters falling = 30 raycasts/frame. At 60 FPS, 1,800 raycasts/second.

---

## Cascade 4: Character State Caching (Low–Medium Impact, Correctness Risk)

### Potential Shared Instance Problem

**File**: `Assets/Natureous/States/Abilities States Scripts/CharacterState.cs`  
**Lines 41–51**: GetCharacterControl method

```csharp
private CharacterControl characterController;

public CharacterControl GetCharacterControl(Animator animator)
{
    if(characterController == null)
    {
        characterController = animator.transform.root.GetComponent<CharacterControl>();
    }
    
    return characterController;
}
```

**Observation**: CharacterState is a `StateMachineBehaviour`, which is typically a **shared asset instance** across all animators using the same state machine asset.

**Correctness risk**: If multiple characters share the same animator controller asset (which is likely for efficiency), then `characterController` in the first character to call `GetCharacterControl` will be cached, and subsequent characters will retrieve the wrong CharacterControl reference.

**Impact**: **MEDIUM (correctness) / LOW (performance)**  
**Type**: SUSPECTED | Needs verification: check if animator assets are shared or per-instance.

**Performance note**: If the code is *intended* to cache per-instance (i.e., each character has its own animator controller copy), then this is harmless. If animator assets are shared, this is a silent correctness bug where attacks, ground detection, and state transitions could affect the wrong character.

---

## Cascade 5: Edge-Sphere Collider Instantiation (Medium Impact)

### Instantiation Overhead

**File**: `Assets/Natureous/Controllers/CharacterController/CharacterControl.cs`  
**Lines 134–162**: SetupColliderSpheres and CreateEdgeSphere

```csharp
private void SetupColliderSpheres()
{
    // ... calculate positions ...
    
    GameObject bottomFront = CreateEdgeSphere(new Vector3(0f, bottom, front));
    GameObject bottomBack = CreateEdgeSphere(new Vector3(0f, bottom, back));
    GameObject topFront = CreateEdgeSphere(new Vector3(0f, top, front));
    
    // ... add to lists ...
    
    float horizontalSectionLength = (bottomFront.transform.position - bottomBack.transform.position).magnitude / 5f;
    CreateMiddleSpheres(bottomFront, -Vector3.forward, horizontalSectionLength, 4, BottomSpheres);
    
    float verticalSectionLength = (bottomFront.transform.position - topFront.transform.position).magnitude / 10f;
    CreateMiddleSpheres(bottomFront, Vector3.up, verticalSectionLength, 9, FrontSpheres);
}

public GameObject CreateEdgeSphere(Vector3 pos)
{
    GameObject obj = Instantiate(ColliderEdgePrefab, pos, Quaternion.identity);
    return obj;
}
```

**The structure**:
- 3 corner spheres + 4 horizontal middle + 9 vertical middle = **16 GameObject instantiations per character** (6 in BottomSpheres, 11 in FrontSpheres, bottom-front shared) during Awake
- Each instantiation: Instantiate, AddComponent (collider), parenting, list add
- **Per-scene cost**: player (16) + N enemies (16 × N)

**Impact**: **MEDIUM (on startup/enemy spawn)**  
**Type**: CONFIRMED in code | Initialization and enemy spawn overhead (not per-frame, but significant at load time and when enemies spawn)

---

## Cascade 6: Respawner Check Every Frame (Low Impact)

**File**: `Assets/Natureous/Core/Respawner.cs`  
**Lines 80–84**: LateUpdate method

```csharp
private void LateUpdate()
{
    if (IsFallingIntoTheAbyss())
        Respawn();
}

bool IsFallingIntoTheAbyss()
{
    var isFallingIntoAbyss = this.gameObject.transform.position.y < -20;
    if (isFallingIntoAbyss)
        AnalyticsManager.Instance.LogPlayerHasFallenIntoAbyss();
    return isFallingIntoAbyss;
}
```

**Issue**: LateUpdate runs on every frame, even if the character is not near the abyss boundary. The check is cheap (a single float comparison), but:
- **Line 15**: If the player falls into the abyss, it calls `AnalyticsManager.Instance` (another FindObjectOfType)
- Unnecessary every frame for most gameplay

**Impact**: **LOW**  
**Type**: CONFIRMED in code | Per-frame, negligible CPU cost but adds one more Singleton access in rare cases

---

## Cascade 7: Smartlook Session Recording SDK (Medium–High Impact)

**File**: `Assets/Natureous/Managers/AnalyticsManager.cs`  
**Lines 12–16**: Awake method

```csharp
private void Awake()
{
    GameAnalytics.Initialize();
    Smartlook.SetupAndStartRecording("7c105a30a5e7c648c52013fd05fed03a5d2f3f0d");
}
```

**Suspected performance impact**: session-recording SDKs capture the screen and input continuously and upload them, which costs CPU, memory and network. No Smartlook source was consulted and nothing was measured, so the size of this cost is **Unknown**. Recording starts unconditionally in `Awake`, including in editor and desktop sessions.

**Impact**: **Unknown (suspected MEDIUM)**  
**Type**: Recording call CONFIRMED in code; cost SUSPECTED (needs Profiler)
**Where**: Initialized once in Awake, runs globally throughout session

---

## Summary: Per-Frame Cost Estimate

| Bottleneck | Cost (per frame @ 60 FPS) | Type | Severity |
|---|---|---|---|
| Singleton FindObjectOfType (input + relays) | ≥420 searches | O(n) scene traversal | HIGH |
| Singleton FindObjectOfType (attacks + other managers) | ≥6 searches | O(n) scene traversal | MEDIUM |
| DamageDetector nested loops + string comparisons | 1,350–4,050 comparisons | Algorithmic | HIGH |
| GroundDetector raycasts (3 chars falling) | 30 raycasts | Physics query | MEDIUM |
| Respawner abyss check | 1 float comparison | Negligible | LOW |
| **Subtotal framework overhead** | **~500 scene searches + 4,000+ string ops + 30 raycasts** | Cascading | **VERY HIGH** |
| Smartlook recording (global) | Unknown (not measured) | SDK overhead | MEDIUM |

---

## Architectural Root: "How We Approached the Game"

The Natureous codebase treats **managers as globally-accessed singletons**, which is a common pattern in game development but was implemented without proper instance caching. This led to:

1. **Manager-centric architecture**: Input, attacks, characters, physics state are all mediated through singleton managers.
2. **No inversion of caching logic**: The Singleton pattern should check if the instance is already cached before searching. The current implementation searches every time.
3. **Compounding across layers**:
   - Input → reads from VirtualInputManager
   - Controllers → write to VirtualInputManager via ManualInput
   - State machines → read from AttackManager, CharacterManager
   - Damage → reads from AttackManager
   - Each read is independent and doesn't know about the others, so redundancy accumulates

This is not a bug per se—the code works correctly. It's an architectural decision that trades convenience (global access) for performance (repeated searches).

---

## How to Confirm with the Profiler

### Step 1: Enable Unity Profiler

1. Open `Window > Analysis > Profiler` in the Unity Editor
2. Select **CPU Usage** in the left panel
3. In the **Profiler** window, expand the call tree
4. Look for:
   - `FindObjectOfType` calls (should appear under **YourScript.Update**)
   - `Physics.Raycast` calls (under **GroundDetector.IsGrounded**)
   - String operations (under **DamageDetector.CheckAttack**)

### Step 2: Profile Input Reads

1. Enter Play mode
2. Press any movement key (W, A, S, D)
3. In the Profiler, expand **KeyboardInput.Update**
4. **Look for**: A deep chain of `FindObjectOfType` calls, one for each `VirtualInputManager.Instance` access
5. **Expected**: ≥7 FindObjectOfType calls per Update frame

### Step 3: Profile Attack Detection

1. Have the player attack an enemy
2. In the Profiler, expand **DamageDetector.Update** → **CheckAttack**
3. **Look for**: 
   - Loop iterations (foreach over CurrentAttacks)
   - Loop iterations inside `DidCollide` (nested foreach over body parts, colliders, names)
   - String.Equals calls
4. **Expected**: 1,000–4,000 operations depending on active attacks and character count

### Step 4: Profile Ground Detection

1. Have a character jump or fall
2. In the Profiler, expand **GroundDetector.UpdateAbility**
3. **Look for**: Physics.Raycast calls
4. **Expected**: 6 raycasts per character per frame when falling, 0 when grounded

### Step 5: Profile Total Scene

1. Load a typical gameplay scene with player + 2–3 enemies
2. **Profiler > CPU Usage**: Note the total **Scripts** time (no baseline exists; record the number as the first measurement)
3. **Breakdown**: if this analysis is right, `FindObjectOfType` should show up prominently under the input and manager `Update` calls

### Key Markers to Watch

| Marker | Expected Appearance | What It Tells You |
|---|---|---|
| `FindObjectOfType` | Under Input, Manager, and State Update methods | Singleton pattern overhead |
| `Physics.Raycast` | Under GroundDetector, likely 0.1–0.5ms total | Raycast query cost |
| `String.Equals` or `String.Compare` | Under DamageDetector, likely 0.5–2ms total | String allocation and comparison |
| `List.Contains` | Under DamageDetector, GroundDetector | Linear list searches |
| **Frame Time** | Typically 16–20ms for 60 FPS target | Total overhead; should be <16ms |

---

## Why Powerful Machines Still Struggle

Even a high-end desktop or console can experience frame rate dips because:

1. **O(n) searches are O(n)**: A scene with 100+ objects (player, enemies, collider spheres, UI, lights) makes every FindObjectOfType call slower
2. **Cascading redundancy**: 500+ searches per frame × O(n) per search = significant overhead even at high clock speeds
3. **Memory bandwidth**: Each search traverses the entire object hierarchy, potentially causing cache misses
4. **Physics queries stack**: 30+ raycasts per frame, even on a fast CPU, consume GPU compute and CPU-GPU sync time
5. **String allocation patterns**: Repeated string comparisons in DamageDetector can trigger garbage collection pauses, especially on lower-end hardware or long play sessions

The bottleneck is **not** raw CPU power; it's **algorithmic efficiency**. A more optimized design (cached singletons, spatial partitioning for damage, single capsule raycast) would run smoothly on the same hardware.

---

## No Fixes Applied

This document provides diagnosis and profiling guidance. See the separate how-to guide for remediation strategies (caching singleton instances, optimizing damage detection with spatial queries, consolidating raycasts).

---

**Document confidence**: Codebase analysis is based on evidence from source files. Profiler metrics are estimated; actual numbers depend on scene complexity and target platform. See profiling steps above to measure actual impact.


## Not covered

Rendering (URP asset settings, lights, shadows, skybox shaders, post-processing) and physics settings (`TimeManager.asset`, `DynamicsManager.asset`) were not analysed. They are open candidates for the bottleneck, especially on a 2.5D scene with many realtime lights. Check them in the Profiler's Rendering module and Frame Debugger.
