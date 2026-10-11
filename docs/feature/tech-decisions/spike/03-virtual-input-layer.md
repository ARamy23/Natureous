# 03 — Virtual input layer

## Decision
Input flows in four steps: devices, then a shared virtual pad, then a relay, then the character.
- **Devices.** `KeyboardInput.Update` writes 7 bools into `VirtualInputManager.Instance` (`InputDevices/KeyboardInput.cs:17-26`). It disables itself on Android and iOS (`:12-14`). `MobileInput.Update` maps the Joystick Pack axes to 6 bools using thresholds (`InputDevices/MobileInput.cs:39-47`). Attack is set through UI button callbacks (`:49-57`). `MobileInput` disables itself under `UNITY_STANDALONE` (`:32-36`).
- **Virtual pad.** `VirtualInputManager` is a singleton holding 7 public bools (`Managers/VirtualInputManager.cs:7-16`).
- **Relay.** `ManualInput.Update` copies the 7 bools into `CharacterControl` (`Managers/ManualInput.cs:18-27`).
- **Character.** `CharacterControl` holds the same 7 bools (`CharacterControl.cs:29-35`). AI writes those same fields, which makes `CharacterControl` the common seam for player and AI.

## Goal behind it
- **Stated (commits and Trello):** support desktop and mobile with a single character pipeline. Evidence: commit `69b7ec6` "Extend mobile input to sprint and climb", and the Trello card "Add Sprint to mobile control system" (`docs/trello/board.md:143`).
- **Inferred:** the device-agnostic abstraction and the shared AI/player seam. Nothing records those.

## What it achieved / where it hurt
- **Achieved:** the mobile port needed no changes to character logic. AI and player share every ability.
- **Hurt: singleton lookups.** Every hop goes through the singleton. That means 14 `Instance` accesses per frame on desktop: 7 writes plus 7 relay reads (`performance-bottlenecks.md` Cascade 1, where the impact depends on decision 02).
- **Hurt: level-only input.** Bools are levels, not edges, so jump-hold and attack-press semantics are left to each ability. "Hold Space = higher" (CLAUDE.md) relies on the `Jump` level in `CharacterControl.FixedUpdate` (`CharacterControl.cs:171`).
- **Hurt: frame ordering.** The order of the `Update` calls between the device, `ManualInput` and the Animator is not defined, so input may arrive a frame late. This is unverified.
- **Hurt: no rebinding.** Keys are hard-coded (`KeyboardInput.cs:19-25`).

## Unity 6 status
The legacy `UnityEngine.Input` still works when Active Input Handling is set to Old or Both. `activeInputHandler` is absent from ProjectSettings, which means Old (migration findings item 10, UNVERIFIED). Nothing is removed.

## Probe question
Does player input reach `CharacterControl` in the same frame it is read? In other words, is there zero frames of latency added between `KeyboardInput.Update` and `ManualInput.Update`?

**Probe method:** a PlayMode test that sets `VirtualInputManager.Instance.MoveRight = true` from a component with a forced-early `[DefaultExecutionOrder(-100)]`, then asserts `CharacterControl.MoveRight` within the same frame. A second test replaces `KeyboardInput` with the Input System `InputTestFixture` after a later migration.
**Budget:** under 2 h.

## Pending research
- Should the project move to the Unity Input System package: action maps, a rebinding UI, on-screen stick and button controls (which would replace Joystick Pack), and the `InputTestFixture` for tests?
- Should input be modelled as an `ICharacterIntent` (or a struct) that is pushed once per frame, instead of 7 public bools copied twice?
- Should press and release edges (`WasPressedThisFrame`) be explicit for jump and attack?
- Would `PlayerInput` with behaviour callbacks make `ManualInput` unnecessary?

## Verdict
**KEEP-CANDIDATE** for the abstraction, which is a single seam for AI, keyboard and mobile. **REVISIT** for the transport (singleton hops and legacy Input). Both are provisional.
