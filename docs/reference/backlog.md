# Project Backlog

Current status as of 2026-10-11 from Trello board "First Game!" (https://trello.com/b/aawcUoCf/first-game).

---

## Done (Programming)

| Task | Code Evidence | Note |
|------|----------------|------|
| Learn Unity and build a prototype | `Assets/Natureous/Scenes/Sandbox.unity` | Foundational work; prototype assets in place |
| Made a character and made it move | `Assets/Natureous/Controllers/CharacterController/CharacterControl.cs` | Input → movement pipeline functional |
| Character moving animated | `Assets/Natureous/Scenes/Sandbox.unity` | Animator setup with movement states |
| Character can now jump | `Assets/Natureous/States/Abilities States Scripts/Jump.cs` | Jump ability implemented |
| Character can now jump and land | `Assets/Natureous/States/Movement/Player_Landing.asset` | Landing transition working |
| Gravity pull more dependent on hold time | `Assets/Natureous/States/Abilities States Scripts/GravityPull.cs` | Animation curve driving gravity multiplier |
| Falling or coming off edges animation | `Assets/Natureous/States/Movement/Air Movements/` | Fall state and detection in place |
| Rag doll parts and death animation | `Assets/Natureous/Characters/Death/` | RagdollParts list, death animator swap implemented |
| Character can now punch | `Assets/Natureous/States/Abilities States Scripts/Attack.cs` | Attack state data and pooling working |
| Punch animation dash forward | `Assets/Natureous/States/Abilities States Scripts/MoveForward.cs` (attack variant) | Attack momentum implemented |
| Sprinting | `Assets/Natureous/States/Abilities States Scripts/CheckSprint.cs` | Sprint transition and state machine |
| Sprint jump | Multiple sprint jump assets in `Assets/Natureous/States/Movement/Running/` | Running jump states |
| Special Attack | `Player_Uppercut_*.asset`, `Player_Smash_*.asset` | Multiple attack types with unique timing |
| AI - Enemies attack | `Assets/Natureous/States/Abilities States Scripts/Attack.cs` (used by AI states) | AI reuses player attack system |
| AI - Enemies move | `Assets/Natureous/States/AI/Locomotion/` | PathFindingAgent and AIProgress in place |
| Camera - Shake on hit | `Assets/Natureous/States/Abilities States Scripts/ShakeCamera.cs` | ShakeCamera ability and CameraManager |
| Respawn | `Assets/Natureous/Core/Respawner.cs` | Respawn-to-checkpoint system |
| Checkpoints and Japanese Gate | `Assets/Natureous/Core/Checkpoint.cs`, CheckpointManager.cs | Checkpoint system and Portal.cs for gates |
| Level Complete UI | `Assets/Natureous/UI/GameOverScreenUIHandler.cs` | Game over screen (no success state UI) |
| Fader when respawning | `Assets/Natureous/Scenes/Scenes Utilities/SceneFader.cs` | Scene fade transitions |
| Fader for next scene | `SceneFader.cs` used by Portal | Portal fade + scene load |
| Open browser on survey | `Assets/Natureous/Managers/AnalyticsManager.cs` | Survey launch via Smartlook/GameAnalytics |
| Analytics | `Assets/Natureous/Managers/AnalyticsManager.cs` | GameAnalytics and Smartlook integration |
| Smartlook | `AnalyticsManager.cs` | Smartlook SDK initialized |
| Add CI/CD | Not in code repo | Infrastructure task; no artifact |
| Add sprint to mobile control | `Assets/Natureous/InputDevices/MobileInput.cs` | Mobile input sprint button added |
| Fix sprint + attack | `Assets/Natureous/States/Abilities States Scripts/CheckSprintAndMovement.cs` | Combined check logic |
| Fix enemy spawn on level restart | `Assets/Natureous/States/AI/PlayerDetector.cs` | Respawn triggers enemy spawning correctly |
| Adjust camera | `Assets/Natureous/Camera/CameraController.cs` | Camera positioning and following |
| Auto detect ledge and climb | `Assets/Natureous/Characters/LedgeChecker.cs`, CliffClimbing states | Ledge detection and climbing states |
| Add cliff climbing | Cliff Climbing StateData folder | Ledge grab, hang, crouch transitions |
| Release 0.2 | Shipped (2026-09-24 approx) | First playable release |

---

## In Progress

| Task | Status | Note |
|------|--------|------|
| Add an intro scene | Conceptual | No scene implemented yet |
| Character selection | Backlog | No selection UI |
| Melee Attacks | Variants exist | Additional attack types planned |
| Magic attacks | Backlog | No implementation |
| Blocking system | Backlog | No blocks or parry mechanics |
| Deflecting and counter attack | Backlog | No riposte system |
| Pet or object follower system | Backlog | Conceptual pet system; no code |
| Double Jump | Backlog | No mid-air jump |
| Create Melee Weapon Combos | Partial | Attack chaining exists; weapon variety TBD |
| Add puzzles | Backlog | No puzzle mechanics |
| Health system | Done (partial) | No visible health UI; damage kills instantly |
| Create air combo | Backlog | No sequenced air attacks |
| Improve AI Attacks | Backlog | Current: reuse player attack system |
| Allow AI to do Combos | Backlog | Combo detection only for player |
| Improve AI Movement | Backlog | Basic pathfinding; no flocking/formation |
| Allow AI to Sprint | Backlog | No sprint state for AI |

---

## To Do (Unstarted)

| Task | Backlog Category | Priority Note |
|------|------------------|---------------|
| Release 0.3 | Version milestone | No scope defined |
| Game Coins | Monetization | Backend required |
| In-app purchases | Monetization | Backend required |
| Shop | Monetization | No shop UI or inventory |
| Skill points | Progression | No skill allocation UI |
| Score | Game flow | No per-level scoring |
| Backend for Game coins, IAP, Shop Items, Skillpoints, Score | Monetization | Major infrastructure |
| See if we can utilize Remote Config | Infrastructure | Config management via Firebase/AppCenter |

---

## Blocked

| Task | Reason | Note |
|------|--------|------|
| Integrate Combat system with Game Creator Combat Module | Not Possible | Game Creator is third-party; custom system preferred |

---

## AI Intelligence Levels

**Trello Card**: "Create AI Intelligence Levels" (To Do, unprioritized)

Proposed four tiers of AI difficulty. No implementation in codebase yet.

| Level | Movement | Attacks | Blocking | Behavior |
|-------|----------|---------|----------|----------|
| **Basic** | Walk, Jump, Climb | 1 standing/walking attack; long wait between | Cannot block | Simplest opponent; pacing dependent on designer |
| **Mid** | Walk, Run, Jump | Moving, walking, running attacks with wait | Blocks with 0.75s debounce | Intermediate; reactionary |
| **Aggressive** | Walk, Run, Jump | Moving, walking, running attacks, no wait | Blocks with 0.5s debounce | Fast-paced; reduced reaction time |
| **Sentient** | All movement + fluidity | All attacks, perfect combos, no delay | Blocks with 0.3s debounce | Hardest; relentless; mastery-level challenge |

**Architecture ready for**: Each level would map to a different configuration of StateData abilities and behavior thresholds in AIProgress.

---

## Design Decisions (Agreed)

These items are on board but implemented through architecture decisions, not discrete tasks:

- **2.5D Dimension**: Implemented via isometric camera, Z-layering, 2D movement plane
- **State Machine Architecture**: Animator-driven via CharacterState + StateData system
- **Oceanhorn-style Progression**: Monument-restoration-based power growth
- **Hack-and-slash Combat** (preferred): Fast attack chains, momentum-based
- **Keyboard + Mobile Controls**: KeyboardInput and MobileInput both active; Mobile disables on non-mobile
- **Analytics**: Integrated GameAnalytics and Smartlook from start
- **Pooling Strategy**: AttackInfo, VFX objects pooled to reduce GC pressure

---

## Sources

- Trello board "First Game!" snapshot (2026-10-11)
- Code in `Assets/Natureous/` directory
- Scene configurations: `ProjectSettings/EditorBuildSettings.asset`
