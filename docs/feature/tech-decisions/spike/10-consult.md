# 10 Consult: render pipeline, 2.5D constraint, camera (Unity 6.6)

Inputs: `10-render-pipeline-and-2.5d.md`, `unity6-migration/spike/findings.md` items 12-13, `GraphicsSettings.asset:46`, `Packages/manifest.json:3`. Checked on 2026-10-11.

## 1. Fact-check: "URP is deprecated"
**Verdict: FALSE.** The Built-in Render Pipeline is the one being deprecated, not URP. URP is Unity's main pipeline going forward.

| Claim | Status | Evidence |
|---|---|---|
| Built-in RP deprecated | **Shipped** in 6.5 | "marking the Built-In Render Pipeline as deprecated in Unity 6.5". It stays available through "Unity 6.7 LTS", with support "at least until the end of 2028". [unity.com strategy](https://unity.com/topics/render-pipelines-strategy-for-2026), [forum post](https://discussions.unity.com/t/render-pipelines-strategy-for-2026/1710004), [6000.6 manual](https://docs.unity3d.com/6000.6/Documentation/Manual/built-in-render-pipeline.html) |
| URP is the focus | **Announced** | Unity is investing in URP for "faster performance, build time, enhanced stability and extensibility" and new 3D runtime lighting. Secondary reports say SCGI GI and SSR are planned for 6.7 (UNVERIFIED against a first-party release note). |
| HDRP | **Announced**: maintenance only | "no new features are planned for HDRP". Work is limited to Switch 2 support and stability. |
| Unified renderer (URP and HDRP merged) | **Announced as a direction (2024), not shipped** | Discussed at Unite 2024 for the release after Unity 6 ([thread](https://discussions.unity.com/t/unified-renderer-coming-to-unity-next-gen-after-6/1522190), [CG Channel](https://www.cgchannel.com/2024/09/unity-previews-its-roadmap-for-unity-6-1-and-beyond/)). The 2026 strategy page does not mention it. |

The likely source of the confusion is the Built-in RP deprecation in 6.5, together with HDRP losing new features. Natureous already uses URP, so it is on the supported path.

## 2. Audit of the current setup
- URP 7.1.8. The default asset is HighQuality (`GraphicsSettings.asset:46`). There are three quality tiers, and mobile uses Medium.
- High tier: HDR, MSAA 2x, soft shadows, per-pixel additional lights, shadow distance 50. That is too heavy if mobile ever selects it.
- **Stray asset:** a second URP asset and renderer at `Assets/Natureous/Characters/YBot/UniversalRenderPipelineAsset*.asset`. It is not referenced by Graphics or Quality as far as was read, but it is a confusion risk. Verify, then delete or move it.
- Cinemachine 2.5.0 (`manifest.json:3`) is used only through scene and prefab components. There is no Cinemachine C# API usage, so the 3.x upgrade is easy.
- The 2.5D plane is kept by convention: Translate along local forward on Z, and X is pinned only on respawn. No Rigidbody constraints were found by grep.

## 3. Options

### Renderer
| Option | How | Pros | Cons | Fit | Migration effort |
|---|---|---|---|---|---|
| A. URP Universal Renderer, **Forward+**, Render Graph | Auto-upgrade from ForwardRenderer, then set Rendering Path = Forward+ | No per-object light limit. Render Graph is the default and the future in Unity 6. SRP Batcher and GPU Resident Drawer are available. | Slight base cost on very low-end GLES devices | High: 3D characters and environments, side view | Low (automatic) |
| B. URP Universal Renderer, plain **Forward** | Same as A, keep Forward | Cheapest per pixel on low-end mobile when there are few lights | Additional-light cap per object | High if the art uses few lights | Lowest |
| C. URP **2D Renderer** | Swap the renderer to Renderer2D | Sprite lighting, 2D tooling | 3D lit meshes and shadows are poorly supported, and the art is 3D (YBot, Oceanhorn-like look) | Low | High (re-light everything) |
| D. HDRP | Switch pipeline | High fidelity | No mobile support, no new features | None | Very high |

### Quality tiers (all on option A or B)
- Mobile Low/Medium: HDR off (or the 6.x low-precision HDR format), MSAA off or 2x, render scale 0.8-1, 1 cascade, shadow distance 20-30, hard shadows, additional lights per-vertex or off, SRP Batcher on.
- Desktop High: HDR, MSAA 4x, 2 cascades, soft shadows.
- A stylized look benefits from unlit or simple-lit Shader Graph shaders plus baked light probes. These are cheaper than realtime soft shadows.

### 2.5D constraint
| Option | How | Pros | Cons | Effort |
|---|---|---|---|---|
| Convention (current) | Translate along Z, pin X on respawn | No change | Physics knock-backs or collisions can push the character off-plane | 0 |
| **Rigidbody constraints** | `FreezePositionX`, plus `FreezeRotationX/Z` (keep Y for facing) | Physics enforces the plane | Constraints apply only to physics. Kinematic or Translate code is not clamped. | ~1 h |
| Constraints + clamp in code | The above, plus `pos.x = planeX` in FixedUpdate | Guaranteed for all movement paths | Small amount of code | ~2 h |

### Camera
| Option | Pros | Cons | Effort |
|---|---|---|---|
| CM 2.10.x (phase 1) | Drop-in on Unity 6 | Legacy line that will be removed eventually | Low |
| **CM 3.1.x** (`Unity.Cinemachine`, `CinemachineCamera` + Position Composer + Confiner 2D/3D) | Current line. Unity recommends it for Unity 6, with "3.1.5 released for 6000.2". Upgrader exists. | Data format changes, so Timeline and prefab references need the upgrader | Low-medium, because there are no API scripts ([upgrade guide](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineUpgradeFrom2.html), [3.1.5 post](https://discussions.unity.com/t/cinemachine-3-1-5-is-now-available/1691770)) |

Note: one [issue tracker report](https://issuetracker.unity3d.com/issues/cinemachine-installs-the-outdated-version-by-default-in-the-unity-6-dot-3) says Unity 6.3 installed an older Cinemachine by default. Pin 3.1.x explicitly in `manifest.json`. The exact Cinemachine version validated for 6000.6 is UNVERIFIED, so check the Package Manager once the editor is installed.

## 4. Recommendation
1. Stay on **URP**. Use the Universal Renderer with **Forward+** and Render Graph on (option A). Fall back to Forward (B) on the Low tier only if profiling shows a cost.
2. Rebuild the tiers: Mobile Low, Mobile Med, Desktop High. Turn on the SRP Batcher. Default Android/iOS to Medium, as today.
3. Do not use the 2D Renderer.
4. Freeze X position and X/Z rotation on the player and enemy Rigidbodies, and add an X clamp for Translate paths.
5. Go straight to **Cinemachine 3.1.x** during the Unity 6 migration rather than 2.10, because there is no API code to port. Set up `CinemachineCamera` with Follow on the player, a Position Composer (dead zone and lookahead), and a Confiner per level. Move shake to an Impulse Source, replacing the Animator trigger.
6. Remove the stray YBot URP asset after confirming it is unreferenced.

## 5. Runnable probe (when the 6000.6 editor exists)
- Batchmode: `Unity -batchmode -projectPath . -executeMethod Probes.RenderAudit.Run -quit -logFile -`
  - Log `GraphicsSettings.defaultRenderPipeline`, each `QualitySettings` tier's pipeline asset, and the renderer type and rendering path.
  - List materials whose shader is `Hidden/InternalErrorShader` or not `Universal Render Pipeline/*` or Shader Graph.
  - Use `AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset")` to flag unreferenced assets.
  - Pass if there are zero error shaders and exactly 3 referenced assets.
- Cinemachine: after `Window > Cinemachine > Upgrade Project to CM3`, grep scenes for the old `CinemachineVirtualCamera` GUID. Expect 0.
- Device: a Profiler capture of Sandbox at the Medium tier on the min-spec device. Target 60 fps (16.6 ms), with GPU time under 12 ms.
- Physics: a PlayMode test that applies a lateral force to the player for 5 s. Assert |x − planeX| < 0.001.

## 6. Questions for you
1. **Min-spec devices:** which is the lowest Android/iOS device you will support (for example a 2019-era Mali device or iPhone 8)? Is the frame target 30 or 60 fps? This decides Forward vs Forward+ and the shadow budget.
2. **Art style:** is the look toon/unlit stylized (cheap) or lit with realtime shadows (costly on mobile)? Are the "rotating star sky" and "minimalist character shader" worth recreating in Shader Graph?
3. **Camera:** fixed side view only, or also zoom and depth reveals (Oceanhorn-like angles)? This decides whether the confiner is 2D or 3D.
4. **Physics on enemies:** should knock-back ever leave the plane on purpose (for example falling into the background)? If yes, constrain only the player.
