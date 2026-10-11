# 01 — Animator as the gameplay logic layer

## Decision
Gameplay runs inside Animator states rather than MonoBehaviour `Update` loops.
- `CharacterState : StateMachineBehaviour` holds `List<StateData> ListOfAbilityData` and forwards Enter/Update/Exit to each one (`Assets/Natureous/States/Abilities States Scripts/CharacterState.cs:8-39`).
- `StateData : ScriptableObject` is an abstract ability with `OnEnter/UpdateAbility/OnExit` (`StateData.cs:8-13`). There are 21 subclasses in `States/Abilities States Scripts/` plus AI locomotion scripts. 71 `.asset` instances sit under `States/`.
- `enum TransitionParameter` (`Controllers/CharacterController/CharacterControl.cs:8-19`) is converted with `.ToString()` into Animator parameter names. That happens at 34 call sites, for example `Attack.cs:25,48,60` and `GroundDetector.cs:27`.
- The player and the YBot enemy share one controller: `SuitedMan.controller` is referenced by both `Characters/Prefabs/SuitedMan.prefab` and `YBot.prefab`.

## Goal behind it
- **Inferred:** the pattern was copied from the Roundbeargames tutorial (commit `85bdd52` "Setup Game from old project", `Assets/Roundbeargames_Tutorial/`; `where-we-wanted-to-be.md` §5).
- **Inferred:** keep logic in sync with animation, for example hit windows tied to `normalizedTime` (`Attack.cs:80,95`), and compose new behaviour from assets without code (`where-we-wanted-to-be.md` §1). No record states either goal.
- **Stated (Trello):** cards are framed around animation, such as "character can now jump and land and hang in the air all with animations" and "when they're hit, they die with animations" (`docs/trello/board.md:148,193`). These cards record features, not rationale.

## What it achieved / where it hurt
- **Achieved:** movement, jumping, ledge climbing, 5 attack chains and AI locomotion were all built in about one month (April 2020 commits). Hit windows follow clip time with no separate timeline.
- **Hurt: string parameters.** Each of the 34 `TransitionParameter.X.ToString()` calls produces a string and does a name lookup on every frame. Probe this before assuming a cost. Renaming an enum value silently breaks the matching Animator parameter.
- **Hurt: shared mutable state.** `StateData` assets are shared ScriptableObjects, yet some hold per-use state (`Attack.FinishedAttacks`, `Attack.cs:21`).
- **Hurt: cached character.** `CharacterState.GetCharacterControl` caches the first `CharacterControl` it finds (`CharacterState.cs:41-51`). Because the controller is shared, this is correct only if Unity creates one SMB instance per Animator. `performance-bottlenecks.md` Cascade 4 marks this as SUSPECTED.
- **Hurt: no tests.** There are no tests or asmdefs (CLAUDE.md), and logic only runs when an Animator ticks, which makes EditMode testing hard.

## Unity 6 status
The `StateMachineBehaviour` and `RuntimeAnimatorController` APIs are unchanged (migration findings item 18, static read). Nothing here is deprecated. Status is UNVERIFIED until Phase B.

## Probe question
Does `CharacterState.GetCharacterControl` return the correct `CharacterControl` for each character when the player and YBot share `SuitedMan.controller`?

**Probe method:** a PlayMode test that instantiates `SuitedMan.prefab` and `YBot.prefab`, steps 10 frames, and then, for each Animator, compares `animator.GetBehaviours<CharacterState>()[i].GetCharacterControl(animator)` with `animator.transform.root.GetComponent<CharacterControl>()`. A second check asserts that the two Animators' `CharacterState` instances are different objects (`ReferenceEquals`).
**Budget:** 1 test file, under 2 h including the asmdef setup.

## Pending research
- Should parameter lookups use `Animator.StringToHash` and cached ints? Is there a measurable difference? (Profiler marker around `StateData.UpdateAbility`.)
- Should per-character runtime state move off shared `StateData` assets, for example into a per-character context object?
- Would Animator-agnostic logic be easier to test: a plain C# state machine, with the Animator used only for visuals and the Playables API or animation events supplying timing?
- Unity 6 has newer options, such as Animation Rigging and Playables-based controllers. Do any of them keep the "hit window = clip time" guarantee?

## Verdict
**REVISIT** (provisional, pending probe). The pattern is central and it worked. Its sharp edges (strings, shared SO state, untestable) can be fixed without replacing it.
