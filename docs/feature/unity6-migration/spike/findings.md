# Spike findings: unity6-migration (Phase A, static)

**Probe question:** What in Natureous (Unity 2019.3.5f1, URP 7.1.8) is deprecated, removed or changed in Unity 6.6 (6000.6.5f1), and what has to change for the project to open, compile and run?
**Mode:** Static only. The editor is not installed. Scan script: `/tmp/spike_unity6-migration/scan.sh <project_root>`.
**Verification caveat:** This session had no web access (no WebSearch or WebFetch). Every "Unity 6 status" entry comes from general knowledge, and each source URL is the official page to check. All are marked **UNVERIFIED** until Phase B or a docs review confirms them.

## Verdict: LIKELY OPENS — no hard compile blockers found; HIGH migration risk in packages and third-party SDKs

None of the 71 own `.cs` files under `Assets/Natureous` uses an API that has been removed. Every hit is either an obsolete API that only produces a warning (`FindObjectOfType`, `Rigidbody.velocity`) or sits in third-party editor code. The real risks are:
1. The URP 7 to URP 17 asset and renderer migration, which affects 5 major versions of serialized data.
2. Cinemachine 2.5 versus Cinemachine 3. Cinemachine is used in `Core.prefab` and `Tutorial Scene.unity` but not in code.
3. Unmaintained SDKs: AppCenter (the service was retired in 2025), GameAnalytics (2019-era), the PlayServicesResolver 1.2.135 DLLs, and Android `.aar` files using support library 26.

## Item table

| # | File:line | What | Unity 6 status | Fix | Source |
|---|---|---|---|---|---|
| 1 | Natureous/Core/Respawner.cs:31,51; Managers/CameraManager.cs:17; Managers/Singleton.cs:15; Objects/Portal/Portal.cs:34 | `FindObjectOfType` | Obsolete (warning) since 2023.1. Use `FindFirstObjectByType` or `FindAnyObjectByType` | Replace with `FindAnyObjectByType<T>()` (`Singleton` uses `FindFirstObjectByType(typeof(T))`) | https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Object.FindObjectOfType.html (UNVERIFIED) |
| 2 | AppCenter/AppCenterBehavior.cs:48, AppCenterBehaviorAdvanced.cs:23, Editor/AppCenterBehaviorEditorAdvanced.cs:31, Plugins/.../UnityCoroutineHelper.cs:20; GameAnalytics/Editor/GA_Menu.cs:33 | `FindObjectsOfType` / `FindObjectOfType` (3rd party) | Obsolete (warning) | Remove the SDK or patch it to `FindObjectsByType(FindObjectsSortMode.None)` | same (UNVERIFIED) |
| 3 | Controllers/CharacterController/CharacterControl.cs:122,130,166,168,171,173; States/.../OffsetOnLedge.cs:18, GroundDetector.cs:37, ToggleGravity.cs:40, ToggleBoxCollider.cs:39 | `Rigidbody.velocity` | Renamed to `linearVelocity` in Unity 6. The old name is obsolete and gets an auto-upgrade prompt | Rename to `linearVelocity`. The API Updater may do this for you | https://docs.unity3d.com/6000.0/Documentation/Manual/UpgradeGuideUnity6.html (UNVERIFIED) |
| 4 | — | `Rigidbody.drag/angularDrag` | Renamed to `linearDamping/angularDamping` | No usage found in code. Physics assets are reserialized | same (UNVERIFIED) |
| 5 | GameAnalytics/Editor/GA_SettingsInspector.cs:1528-2291 (~15 sites); GA_SignUp.cs:1322-1422 | `WWW` class | Obsolete since 2018.3. Still exists in Unity 6 as obsolete (warning) via the `unitywebrequestwww` module. **Removal not confirmed** | Upgrade GameAnalytics to the current UPM package or delete it | https://docs.unity3d.com/6000.0/Documentation/ScriptReference/WWW.html (UNVERIFIED — **possible error-level obsolete**, check in Phase B) |
| 6 | GA_SettingsInspector.cs:1575,1690,1781,1946,2092,2255,2305; GA_SignUp.cs:1336,1436 | `isNetworkError/isHttpError` | Obsolete (warning) since 2020.2. Replaced by `result` | Upgrading the SDK covers this | https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Networking.UnityWebRequest-result.html (UNVERIFIED) |
| 7 | AppCenter/Editor/AppCenterPostBuild.cs:42,88,96 | `PlayerSettings.GetScriptingBackend(BuildTargetGroup)` | Obsolete in favour of `NamedBuildTarget` (warning) | SDK patch or removal | https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PlayerSettings.GetScriptingBackend.html (UNVERIFIED) |
| 8 | GameAnalytics/Plugins/Scripts/Setup/Settings.cs:260; Editor/GA_AssetPostprocessor.cs:20-28,134 | Tizen / StandaloneLinux / OSXIntel enums | Removed, but these sites sit inside `#if` version guards, so they are **not compiled** | None | static read |
| 9 | GameAnalytics/Plugins/Scripts/GameAnalytics.cs:21 | `[ExecuteInEditMode]` | Still supported (`ExecuteAlways` is preferred) | None | UNVERIFIED |
| 10 | Natureous/InputDevices/KeyboardInput.cs:19-24; Scripts/SimpleCameraController.cs:82-148 | Legacy `UnityEngine.Input` | Still supported in Unity 6 ("Input Manager (Old)" or "Both"). New projects default to the Input System. Existing projects keep their setting; `activeInputHandler` is absent from ProjectSettings, which means Old | None to compile. Confirm Active Input Handling = Old | https://docs.unity3d.com/6000.0/Documentation/Manual/class-InputManager.html (UNVERIFIED) |
| 11 | Natureous/UI/PersonalizedPerPlatformText.cs:3 | `using TMPro` | TMP is merged into `com.unity.ugui` 2.0 in Unity 6. The namespace stays `TMPro` | Remove `com.unity.textmeshpro` from the manifest. Re-import TMP Essentials if they are missing (`Assets/TextMesh Pro.meta` exists but the folder looks absent, so check) | https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/TextMeshPro/index.html (UNVERIFIED) |
| 12 | Core.prefab, Tutorial Scene.unity (Cinemachine script GUID 45e653ba…, 2 refs each); no C# usage | Cinemachine 2.5 components | CM 3.x renames the namespace to `Unity.Cinemachine` and replaces `CinemachineVirtualCamera` with `CinemachineCamera`. CM 2.10.x still installs on Unity 6 | Phase 1: move to CM 2.10.x (low risk). Optionally move to 3.x later with the in-editor upgrader | https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineUpgradeFrom2.html (UNVERIFIED) |
| 13 | Assets/ForwardRenderer.asset, Assets/Settings/*, GraphicsSettings m_CustomRenderPipeline guid 19ba41d7… | URP 7.1.8 pipeline asset + Forward renderer | URP 17 (Unity 6) migrates assets automatically on import ("Forward Renderer" becomes "Universal Renderer"). Render Graph is on by default, but there are no custom passes (0 `ScriptableRenderPass` hits) | Upgrade the URP package and let the asset upgrader run. Check that materials use URP/Lit shaders. **No custom shaders were checked in this pass** | https://docs.unity3d.com/6000.0/Documentation/Manual/urp/upgrade-guides.html (UNVERIFIED) |
| 14 | Packages/manifest.json | `com.unity.collab-proxy` 1.2.16 | Collaborate was retired. In Unity 6 the package is "Unity Version Control" (Plastic), 2.x | Remove it | https://docs.unity3d.com/Packages/com.unity.collab-proxy@2.0/manual/index.html (UNVERIFIED) |
| 15 | manifest.json | `com.unity.ide.vscode` 1.1.4 | Deprecated and unsupported. Replaced by `com.unity.ide.visualstudio` | Remove it. Add `ide.visualstudio` if needed | UNVERIFIED |
| 16 | manifest.json | `com.unity.modules.unityanalytics`, `modules.vr`, `modules.umbra` | `unityanalytics` (legacy) and `vr` modules were removed in later versions. Umbra still exists | Remove legacy modules if the Package Manager reports errors | UNVERIFIED |
| 17 | ProjectSettings: `apiCompatibilityLevel: 6` (.NET 4.x), scriptingBackend default Mono, ColorSpace Linear, AndroidMinSdk 19 | Player settings | .NET 4.x maps to .NET Framework (fine). Unity 6 Android **minimum API is 23 or higher**, so 19 will be bumped or flagged | Set the Android min SDK to the Unity 6 minimum. Linear is fine | https://docs.unity3d.com/6000.0/Documentation/Manual/android-requirements-and-compatibility.html (UNVERIFIED) |
| 18 | `RuntimeAnimatorController` usage (Respawner, DamageDetector, DeathAnimationsManager) | Runtime animator API | Unchanged. No `UnityEditor.Animations` usage | None | static read |
| 19 | — | UnityEngine.Experimental, OnLevelWasLoaded, Application.LoadLevel, GUIText, UNet, URP custom features | **0 hits** | — | scan.sh |

## Package upgrade map (target 6000.6.x; exact versions are whatever the editor pins, so treat these as UNVERIFIED)

| Package | Old | New |
|---|---|---|
| com.unity.render-pipelines.universal | 7.1.8 | 17.x (editor-locked core version) |
| com.unity.cinemachine | 2.5.0 | 2.10.x (phase 1) → 3.1.x (optional later) |
| com.unity.collab-proxy | 1.2.16 | **remove** |
| com.unity.textmeshpro | 2.0.1 | **remove** (merged into ugui 2.0) |
| com.unity.ugui | 1.0.0 | 2.0.0 |
| com.unity.timeline | 1.2.12 | 1.8.x |
| com.unity.test-framework | 1.1.11 | 1.4+/1.5 |
| com.unity.ide.rider | 1.1.4 | 3.0.x |
| com.unity.ide.vscode | 1.1.4 | **remove** → com.unity.ide.visualstudio 2.0.x |
| modules.unityanalytics / modules.vr | 1.0.0 | remove if the Package Manager rejects them |

## Recommended upgrade order
1. Work on a git worktree only. Back up `Library/` (or don't: deleting `Library/` forces a clean reimport).
2. Edit the manifest before opening: remove collab-proxy, ide.vscode and textmeshpro. Bump cinemachine to 2.10.x, ide.rider to 3.x, ugui to 2.0.0, and URP to the editor's built-in version.
3. **Go directly to Unity 6**, not through a 2022 LTS step. The own code is small and clean, and the URP upgrader handles 7 to 17. Fall back to a 2022.3 intermediate step only if the URP asset upgrade fails in Phase B.
4. Accept the API Updater on first open (for `velocity` → `linearVelocity`).
5. Third-party SDKs: delete **AppCenter + AppCenterEditorExtensions** (the service was retired on 2025-03-31) and **Smartlook** unless they are actively used. Replace GameAnalytics with its current UPM package, or delete it. Replace PlayServicesResolver 1.2.135 plus the `Plugins/Android/*.aar` files (support library 26, Firebase 16) with External Dependency Manager 1.2.18x, or remove them.
6. Replace `FindObjectOfType` in own code.
7. Run the Render Pipeline Converter / material upgrade and check the Sandbox visuals.

## Third-party SDK risks
- **AppCenter (130 .cs + iOS frameworks):** the service has shut down. It uses obsolete APIs, and its iOS frameworks may fail to link with current Xcode. **Recommend removal.**
- **GameAnalytics (45 .cs, native libs for iOS/tvOS/Tizen/Android):** heavy use of `WWW`. If `WWW` turns out to be removed or error-level in 6.6, this is the main compile blocker (editor assembly).
- **PlayServicesResolver 1.2.135 DLLs:** very old, and might conflict with Gradle 8 / AGP 8 in Unity 6. **Android build risk** only; it does not block opening the project.
- **Plugins/Android `.aar` files (android.support 26, Firebase 16):** AndroidX/Jetifier is required for the Unity 6 Gradle setup. Android build risk.
- **Smartlook (3 .cs):** wraps native calls, so the compile impact is low. The SDK is probably out of date.
- **Joystick Pack (11 .cs):** no hits. Low risk.
- **Roundbeargames_Tutorial:** assets only (VFX and weapons). Standard-shader materials may turn pink until they are converted to URP.

## Phase B (needs editor)
```bash
git -C /Users/Ahmed.Ramy/Developer/Natureous worktree add /tmp/natureous-u6 develop --detach
# (apply manifest edits from step 2 in the worktree only)
/Applications/Unity/Hub/Editor/6000.6.5f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -accept-apiupdate \
  -projectPath /tmp/natureous-u6 \
  -logFile /tmp/spike_unity6-migration/phaseB-open.log
grep -E "error CS|Scripts have compiler errors|obsolete" /tmp/spike_unity6-migration/phaseB-open.log
```
Pass criteria: exit 0, zero `error CS` lines. After that, run the e2e step: play `_preload`, check that Sandbox loads and the player can be controlled (needs a PlayMode test or a manual run).
Run it twice: once with the unmodified manifest (to measure the raw blockers) and once with the step-2 edits.

## Phase B results (editor 6000.6.5f1 arm64, 2026-10-11)

Run on worktree `../Natureous-u6`, branch `spike/unity6-migration`. Logs in `../Natureous-u6-logs/` (not committed).

| Step | Result | Time |
|---|---|---|
| Open untouched project (`-accept-apiupdate`) | Packages auto-upgraded (URP 7.1.8 → 17.6.0, Cinemachine → 6.6.0, Timeline 6.6.0, uGUI 2.6.0, Test Framework 1.8.0; TextMeshPro folded into uGUI). API updater rewrote `CharacterControl.cs` (`velocity` → `linearVelocity`). **3 compile errors, all in GameAnalytics editor code** (`InstanceIDToObject`, `hierarchyWindowItemOnGUI` now errors) | 49 s cold |
| Remove `Assets/GameAnalytics`, stub calls in `AnalyticsManager` with `Debug.Log` | 0 compile errors. Own code: only obsolete warnings (`FindObjectOfType` ×42, `FindObjectsOfType` ×14, `Rigidbody.velocity` ×10) | 21 s warm |
| PlayMode boot smoke (`Assets/Tests/PlayMode/BootSmokeTest.cs`) | Failed: Sandbox held a missing GameAnalytics prefab instance | — |
| Remove missing prefab instances via `Assets/Editor/Migration/RemoveMissingPrefabs.cs` (`-executeMethod`) | Sandbox cleaned through the Unity API (no hand-edited YAML) | — |
| PlayMode boot smoke, strict (any logged error fails) | **PASSED**: `_preload` → Sandbox, 13 `CharacterControl`s | 12.7 s |

**Verdict: WORKS.** The project opens, compiles and boots in Unity 6.6 after removing GameAnalytics.

Not yet checked: rendering correctness (pink materials), gameplay behaviour, Android/iOS builds, Smartlook/AppCenter at runtime, `com.unity.ide.vscode` (deprecated, still resolves), the Cinemachine HDRP sample asmref warning.

**Tooling notes:**
- Headless test runs need the editor binary's `-runTests`. `unity test` (CLI 1.0.0-beta.12) blocked silently on a macOS password prompt the first time; the owner has since allowed it.
- The test asmdef must not list the test runner both in `references` and `optionalUnityReferences` ("duplicate references" aborts compilation without an `error CS` line).
- Game code has no asmdef, so tests reach it by type name via reflection. Adding a `Natureous` asmdef would let tests reference it directly.
