# 12 Third-party content and Roundbeargames tutorial base

## Decision
- **Architecture base:** the Animator-as-logic pattern (`CharacterState` StateMachineBehaviour plus `StateData` ScriptableObjects, and the `VirtualInputManager` input pipeline) follows the Roundbeargames tutorial series (CLAUDE.md; where-we-wanted-to-be.md §5).
- `Assets/Roundbeargames_Tutorial/` contains only `RB_VFX/`, `RB_Weapons/` (2 FBX) and `SimpleCameraController.cs`. It holds **assets, not the architecture code** (findings.md: "assets only").
- **Asset Store packs actually present:** `Assets/Joystick Pack/` (Examples, Prefabs, Scripts, Sprites), used by `InputDevices/MobileInput.cs`.
- **Not found in `Assets/`:** no BOXOPHOBIC or "Minimalist" folders or files. The Trello cards (board.md:153,158) mention a sky shader and a minimalist character shader, but the source is missing. See 10.
- **Animations:**
  - `Characters/Animations/MixamoAnimations/`: 13 FBX clips (Idle, Jump, Prejump, Landing, Falling Idle, Lead Jab, Uppercut, Uppercut_Prep, Uppercut_Death, Running Turn 180, Soccer Tackle, Falling Back Death, Flying Back Death - Ending), plus `With Skin/` (10 FBX).
  - `Characters/Animations/uMotionAnimations/`: 4 `.anim` files (Hanging Idle Still, Smash_Prep, Smash_Finish, Smash_Landing), which suggests in-editor authoring with the uMotion tool. Character model: `Characters/YBot/` (1 FBX), the Mixamo Y Bot.
  - Controller: `Characters/Animations/SuitedMan.controller`.
- **Other SDKs:** GameAnalytics, Smartlook, AppCenter, PlayServicesResolver and `Plugins/Android`. See 09.

## Goal behind it
- **Stated (Trello):** "Got a bunch of characters off Unity asset store" (board.md:213).
- **Stated (commit):** "Setup Game from old project" (85bdd52).
- **Inferred:** follow a published tutorial and use free or bought content to reduce design risk and reach a playable 0.2 within April 2020 (where-we-wanted-to-be.md §5, "Roundbeargames Influence").

## What it achieved / where it hurt
- Achieved: the full movement, combat and climbing set was built in about 4 weeks (project-as-built.md Timeline).
- Hurt:
  - Lock-in to Animator-as-logic (where-we-wanted-to-be.md Tradeoff 3, Q5).
  - The uncached `Singleton` is part of the inherited pattern and is the documented root performance problem (performance-bottlenecks.md "Architectural Root").
  - Assets referenced on Trello are missing from the repo.
  - The Mixamo and uMotion licensing and source files are not documented.

## Unity 6 status
(findings.md)
- Joystick Pack: no hits, low risk.
- Roundbeargames materials: Standard shader, may turn pink until converted to URP.
- FBX and `.anim` imports were not flagged. Humanoid animation import is assumed stable (UNVERIFIED).
- AppCenter and GameAnalytics: see 09.

## Probe question
Do all character FBX and `.anim` clips still import as Humanoid and bind to `SuitedMan.controller` without missing-clip references after the Unity 6 import?

Probe method (run later):
- Batchmode EditMode script. For each `AnimatorController` under `Assets/Natureous`, walk the states and assert that every `motion != null`. For each FBX `ModelImporter`, assert `animationType == Human`.
- Write the results to a log.
- Budget: 2 hours.

## Pending research
- Should the tutorial-derived core (Singleton, Animator-as-logic) be refactored in place, or kept only for player locomotion while new systems (AI brain, health, shop) use plain C# services?
- Which current mobile-input option should replace Joystick Pack, if any: the Input System's On-Screen Stick or keeping the pack?
- What are the licence terms for the Mixamo and Asset Store content in a commercial release, and where should provenance be recorded?
- Should the missing sky and character shaders be recovered from the "old project" or rebuilt?

## Verdict
**KEEP-CANDIDATE** for the animation and joystick assets. **REVISIT** for the tutorial-derived core patterns. Both are provisional, pending the probe and the Singleton fix.
