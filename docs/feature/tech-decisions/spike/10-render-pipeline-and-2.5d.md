# 10 Render pipeline (URP 7.1.8) and 2.5D presentation

## Decision
- **URP:**
  - `ProjectSettings/GraphicsSettings.asset:46` points to `UniversalRP-HighQuality` (guid 19ba41d7…).
  - `QualitySettings.asset` has three levels, Low, Medium and High (:10,47,84), each with its own URP asset (:43,80,117).
  - Per-platform defaults: Android, iPhone and WebGL use Medium (index 1). Standalone uses High (index 2).
- **High asset** (`Assets/Settings/UniversalRP-HighQuality.asset`):
  - HDR on (:25), MSAA 2 (:26), render scale 1 (:27)
  - main light shadows on (:29), additional lights per-pixel (:31)
  - shadow distance 50 (:35), 1 cascade (:36), soft shadows on (:41)
  - dynamic batching off (:43), depth and opaque textures off (:22-23)
  - Renderer: `Assets/ForwardRenderer.asset` (Forward). No custom passes (findings.md item 13).
- **Camera:** Cinemachine 2.5 components appear in `Core/Core.prefab`, `Sandbox.unity` and `Tutorial Scene.unity`, but nowhere in C#. Shake is done by an Animator trigger on `Camera/CameraController.cs:29-32` through `CameraManager`.
- **Sky and lighting:**
  - All 4 build-related scenes use Unity's built-in default skybox (`m_SkyboxMaterial: fileID 10304`, e.g. `Sandbox.unity:29`). `Assets/Materials/Skybox_Mat.mat` exists and uses a built-in skybox shader (fileID 106).
  - Baked and realtime lightmaps are disabled (`Sandbox.unity:54-55`). Sandbox has `LightingData.asset` and one reflection probe.
  - **No BOXOPHOBIC or "Minimalist" asset folder or file was found in `Assets/`.** Trello mentions "a pre-made shader from unity" for the rotating sky and "a different minimalist shader" for the main character (board.md:153,158). Either they lived in the "old project" (85bdd52) or they were removed. Not confirmed.
- **2.5D movement:**
  - `CharacterControl.cs:195-198`: `transform.Translate(Vector3.forward * …)`, so motion is along local forward.
  - `CharacterControl.cs:200-209`: facing is Y-rotation 0 or 180, and `IsFacingRightDirection` is `forward.z > 0`. World travel is therefore along Z.
  - `Respawner.cs:40,61` pins X to 0.

## Goal behind it
- **Inferred:** the URP template defaults (three quality assets, SampleScene, Readme) plus per-platform tiers target desktop and mobile from one project (CLAUDE.md).
- **Stated (Trello):** "sky is now rotating with some stars…", "minimalist shader to make him stand out from dummy characters", "Adjust camera" (board.md:33,153,158).
- **Inferred:** a 3D-rendered side view with Z-axis gameplay was chosen for the 2.5D Oceanhorn-like look (where-we-wanted-to-be.md §Vision).

## What it achieved / where it hurt
- Achieved: one pipeline for desktop and mobile, and Cinemachine follow without custom code.
- Hurt (inferred from the asset settings, not measured):
  - High uses HDR, soft shadows, per-pixel additional lights and MSAA. These are costly on mobile if the High tier is ever selected there.
  - Dynamic batching is off.
  - The 2.5D constraint is enforced only by convention (Translate on local forward, X pinned on respawn), not by Rigidbody constraints. Rigidbody constraints were not read in this probe.
  - performance-bottlenecks.md attributes the slowdown to CPU-side Singleton cascades, not rendering. Rendering cost is not covered there.

## Unity 6 status
(findings.md items 12, 13)
- URP 7 → 17: auto-upgrade on import. Forward Renderer becomes Universal Renderer, and Render Graph is on by default. No custom passes, so risk is low. Material and shader conversion was not checked.
- Cinemachine 2.5 → 2.10.x in phase 1. CM 3.x renames the namespace and the vcam type, and has an upgrader.
- Roundbeargames Standard-shader materials may render pink until converted.
- All UNVERIFIED.

## Probe question
After the URP 17 upgrade, does Sandbox render without magenta materials, and does the Medium (mobile) tier hold 60 fps on a reference device?

Probe method (run later):
- Batchmode editor script. Load each build scene, iterate the renderers, and list materials whose shader name is "Hidden/InternalErrorShader" or not "Universal Render Pipeline/*". This is EditMode and needs no device.
- Then a Profiler capture of Sandbox at the Medium tier on device.
- Budget: 2 hours for the material scan, half a day for the device capture.

## Pending research
- Which URP 17 renderer path suits a 2.5D mobile target: Forward, Forward+, or the 2D Renderer with 3D characters?
- Should the 2.5D plane be enforced in physics (Rigidbody X-position constraint) instead of by convention?
- Do the sky and character shaders mentioned on Trello need recreating in Shader Graph, since the source assets are absent?
- Cinemachine 2.10 versus 3.x: what does it cost to adopt 3.x now rather than later?

## Verdict
**KEEP-CANDIDATE** for URP and Cinemachine, which follow the upgrade path. **REVISIT** for the quality-tier settings and the 2.5D constraint enforcement. Both are provisional.
