# Natureous Documentation

**Natureous** is a 2.5D action-adventure game (hack-and-slash preferred) built in Unity 2019.3. This documentation follows DIVIO structure: each file is exactly one type (Tutorial, How-to, Reference, or Explanation) to serve a specific reader need.

---

## Getting Started

**New to the project?** Start here.

- [How to Run the Game](./howto/run-the-game.md) — Play the game in the editor or via headless commands. Covers keyboard, mobile controls, and troubleshooting.

---

## Understanding the Design

**Want to know why things work this way?**

- [Game Vision](./explanation/game-vision.md) — Story, style, progression philosophy, and design decisions (what we agreed on vs. ideas in exploration)
- [Character Architecture](./explanation/character-architecture.md) — Why gameplay is state-machine driven. Explains the StateData/CharacterState pattern, input pipeline, AI system, combat flow, and singletons.

---

## Building and Extending

**Implementing a feature? Extending the system?**

- [StateData Catalog](./reference/state-data-catalog.md) — Complete reference of every ability class, serialized fields, asset instances, and TransitionParameter enum. Use this to find an existing ability or understand how to create a new one.
- [Project Backlog](./reference/backlog.md) — Status of all tasks from Trello. Links code evidence for completed items and AI Intelligence Levels table.

---

## Project Sources

All documentation is traced to two authoritative sources:

1. **Trello Board** — "First Game!" (https://trello.com/b/aawcUoCf/first-game)
   - Snapshot exported 2026-10-11, stored in `docs/trello/board-export.json`
   - Canonical source for game vision, story, style, backlog status, and decisions

2. **Code Repository** — `Assets/Natureous/`
   - Canonical source for implementation details: StateData subclasses, managers, input pipeline, scene setup
   - Every architecture decision is reflected in code

---

## Documentation Quality

Each document includes a validation file (`.md.validation.yaml`) certifying:
- **Type**: Exact DIVIO classification (Tutorial|How-to|Reference|Explanation)
- **Type Purity**: Percentage of content in primary type (target: 80%+)
- **Sources**: Traced to Trello board or code
- **Quality Scores**: Accuracy, completeness, clarity, consistency, correctness, usability

No document mixes types. If a doc spans multiple DIVIO quadrants, it's restructured into separate files.

---

## Key Concepts at a Glance

### Game Loop
1. Player spawns at checkpoint
2. Input (keyboard/mobile) → `VirtualInputManager` → `CharacterControl` → Animator parameter
3. Animator state enters → `CharacterState.OnEnter` calls all `StateData.OnEnter` abilities
4. Each frame → `CharacterState.OnStateUpdate` calls `StateData.UpdateAbility`
5. Animator state exits → `CharacterState.OnExit` calls all `StateData.OnExit`
6. State transitions based on Animator parameters set by abilities

### Key Classes
- **CharacterState**: StateMachineBehaviour on Animator states. Orchestrates StateData abilities.
- **StateData**: Abstract base for abilities. Subclasses in `States/Abilities States Scripts/`.
- **CharacterControl**: Hub for character input, physics, detection, ragdoll.
- **TransitionParameter**: Enum of Animator parameter names (Move, Jump, Attack, Sprint, etc.)

### Scene Setup
- **_preload** (Scene 0): Boot scene. Initializes `ApplicationManager`, all singletons.
- **Sandbox** (Scene 1): Playable level.
- **Game Over Scene** (Scene 2): End-of-level UI.

Always play from `_preload` in the editor.

---

## Common Tasks

### I want to add a new ability (e.g., "double jump")

1. Read [Character Architecture](./explanation/character-architecture.md) to understand StateData pattern
2. Check [StateData Catalog](./reference/state-data-catalog.md) for similar abilities (e.g., Jump, Landing)
3. Create new StateData subclass in `Assets/Natureous/States/Abilities States Scripts/DoubleJump.cs`
4. Serialize it as `.asset` in `Assets/Natureous/States/General/` or `Movement/`
5. Attach `.asset` instance to Animator state in editor
6. Read [Game Vision](./explanation/game-vision.md) to ensure it fits design intent

### I want to change how fast the player moves

1. Open [StateData Catalog](./reference/state-data-catalog.md)
2. Find `MoveForward` ability
3. Open the `.asset` instance you want to modify (e.g., `General_MoveForward.asset`)
4. Change `Speed` field in Inspector
5. Test from [How to Run the Game](./howto/run-the-game.md)

### I want to check what's Done vs. To Do

1. Open [Project Backlog](./reference/backlog.md)
2. Browse "Done (Programming)" for implemented features
3. Check "To Do" or "In Progress" for upcoming work

### I want to understand why we chose hack-and-slash combat

1. Open [Game Vision](./explanation/game-vision.md)
2. Read "Combat Direction" section for rationale

---

## Gaps (Unknown — Not on Board / Not in Code)

- **Health system**: "Health system" card is in To Do; no health code exists (one hit triggers death in `DamageDetector`)
- **Shop Implementation**: Shop concept in Ideas; no UI or inventory system
- **Score**: "Score" card is in To Do; no code
- **Blocking**: "Blocking system" card is in To Do; no code
- **Double Jump**: card is in To Do; no code
- **Pet System**: Pet/follower concept on board; no code
- **Backend**: To Do card "Backend for Game coins, IAP, Shop Items, Skillpoints, Score per level"; no code

For scope questions or decision rationale, consult Trello board directly.

---

## Maintenance

When you:
- **Add a new StateData subclass** → Update [StateData Catalog](./reference/state-data-catalog.md)
- **Complete a Trello card** → Update [Project Backlog](./reference/backlog.md) with code evidence
- **Change design decisions** → Update [Game Vision](./explanation/game-vision.md)
- **Refactor architecture** → Update [Character Architecture](./explanation/character-architecture.md)
- **Add new scenes/controls** → Update [How to Run the Game](./howto/run-the-game.md)

Re-export Trello board periodically (board.md and board-export.json).

---

## Sources

- Trello "First Game!" board snapshot (2026-10-11)
- `CLAUDE.md` project guidance
- Code: `Assets/Natureous/` directory
- Scenes: `ProjectSettings/EditorBuildSettings.asset`

## Added 2026-10-11

- [Project as built (for LLM agents)](reference/project-as-built.md): timeline from git, feature inventory, system wiring, invariants
- [Where we wanted to be](explanation/where-we-wanted-to-be.md): target game vs what got built, techniques and tradeoffs
- [Performance bottlenecks](explanation/performance-bottlenecks.md): suspected causes of the 2020 slowdown, with code evidence
- [Unity 6.6 migration spike](feature/unity6-migration/spike/findings.md): deprecated APIs, package map, SDK risks
- [Claude Code + Unity research](research/unity/claude-code-unity-gamedev-research.md): MCP servers, hooks, Blender, profiling
- [Concept-sheet prompts](concept/concept-sheet-prompts.md): 3 image-model prompts (2D, 2.5D, 3D)
