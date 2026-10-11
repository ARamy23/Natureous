# How to Run the Game

## Prerequisites

- Unity 2019.3.5f1 (required; other versions untested)
- Project cloned at `/Users/Ahmed.Ramy/Developer/Natureous` (or adjust paths accordingly)
- All dependencies installed (URP 7.1.8, Cinemachine, TextMeshPro, Joystick Pack)

## In the Editor

### Play from the Correct Scene

1. Open the Project in Unity Editor (2019.3.5f1)
2. Open `Assets/Natureous/Scenes/_preload.unity`
3. Press **Play** (or Ctrl+P / Cmd+P)

**Why _preload?** This scene initializes singletons (`ApplicationManager`, `AnalyticsManager`, `VirtualInputManager`, etc.). Starting from other scenes skips initialization and breaks gameplay.

### Controls

#### Keyboard

| Key | Action |
|-----|--------|
| **A / D** | Move left / right |
| **W / S** | Move up / down |
| **Shift** | Sprint |
| **Space** | Jump (hold for higher jump) |
| **J** | Attack |

#### Mobile (Touch Devices)

| Input | Action |
|-------|--------|
| **Left Joystick** | Move (directional) |
| **Attack Button** | Attack |
| **Sprint Button** | Sprint |
| **Jump Button** (hold) | Jump (higher if held longer) |

**Note**: `KeyboardInput` auto-disables on Android/iOS. `MobileInput` activates instead. Both write to `VirtualInputManager`, which feeds input to the player character.

### Verify Gameplay

1. Player spawns in Sandbox scene
2. Walk around: press A/D or WASD
3. Jump: press Space (hold for height)
4. Attack: press J; combo by pressing J again during attack window
5. Sprint: hold Shift + A/D to run
6. Die: touch an enemy or fall off the map → respawn at last checkpoint

**Success**: Game responds to all input without errors. Player reaches a checkpoint or defeats an enemy.

---

## Headless Batch Mode (Testing & CI/CD)

### Compile Check

Verify project compiles without errors:

```bash
/Applications/Unity/Hub/Editor/2019.3.5f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -projectPath /Users/Ahmed.Ramy/Developer/Natureous \
  -quit \
  -logFile -
```

**What it does**: Opens project, compiles all scripts, exits. Prints compile errors to stdout. Useful for CI/CD pre-commit hooks.

### Run Tests (EditMode)

If tests are added to `Assets/Editor/` with `[Test]` attribute:

```bash
/Applications/Unity/Hub/Editor/2019.3.5f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -projectPath /Users/Ahmed.Ramy/Developer/Natureous \
  -runTests \
  -testPlatform EditMode \
  -testResults /tmp/results.xml
```

**What it does**: Runs all EditMode tests, writes XML report to `/tmp/results.xml`.

**Single Test**:

```bash
/Applications/Unity/Hub/Editor/2019.3.5f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -projectPath /Users/Ahmed.Ramy/Developer/Natureous \
  -runTests \
  -testPlatform EditMode \
  -testFilter YourNamespace.YourTestClass.YourTestMethod \
  -testResults /tmp/results.xml
```

### Build to Executable

**Note**: No build scripts exist in repo. Manual build process:

1. **File → Build Settings**
2. **Add Open Scenes** (click "Add Current" for scenes to include)
3. **Build** (select output folder, e.g., `Builds/`)
4. **Run executable**

**Scenes in build order** (`ProjectSettings/EditorBuildSettings.asset`):
1. `_preload` (Scene 0)
2. `Sandbox` (Scene 1)
3. `Game Over Scene` (Scene 2)

The build will run `_preload` first, which loads Scene 1 (Sandbox) automatically.

---

## Troubleshooting

### "Singletons not initialized" / "NullReferenceException on Managers"

**Cause**: Started from wrong scene (not `_preload`)

**Fix**: Always play from `Assets/Natureous/Scenes/_preload.unity`

### Mobile input doesn't respond

**Cause**: Running on desktop with keyboard. Mobile input only activates on Android/iOS builds.

**Fix**: Use keyboard. Mobile input is tested only on mobile devices.

### Game freezes or crashes immediately

**Cause**: Missing dependencies (URP, Cinemachine, TextMeshPro, Joystick Pack)

**Fix**: 
1. Open `Window → Rendering → Universal Render Pipeline`
2. Open `Window → Cinemachine`
3. Import TextMeshPro (usually prompts on first load)
4. Verify `Assets/Joystick Pack/` exists

### Enemies don't spawn

**Cause**: Respawn sequence broke. Check `PlayerDetector` on enemy spawn trigger.

**Fix**: 
1. Ensure `Respawner` set `CheckpointManager.CurrentCheckpoint` on spawn
2. Check `PlayerDetector.OnTriggerEnter` is detecting player
3. Verify enemy prefab in Resources/

---

## Done

Game is running when:
- Player spawns in playable area
- Keyboard/Mobile input works
- Character moves, jumps, attacks
- Enemies spawn and respond to player
- No console errors

## Sources

- `CLAUDE.md` Toolchain section
- `ProjectSettings/EditorBuildSettings.asset` (scene order)
- `Assets/Natureous/Scenes/_preload.unity` (boot entry point)
