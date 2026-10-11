# 09 Analytics-first playtesting (GameAnalytics + Smartlook + survey; AppCenter present)

## Decision
- `Managers/AnalyticsManager.cs:12-16`: `GameAnalytics.Initialize()` and `Smartlook.SetupAndStartRecording("<hard-coded key>")` run unconditionally in `Awake`.
- `AnalyticsManager.cs:18-56` logs these events:
  - progression: checkpoint Complete, abyss Fail, Game Over
  - design: player killed, enemy killed, Survey Opened, Survey Ignored
  - The level names are hard-coded: "First Platform" / "Demo Level".
- `UI/GameOverScreenUIHandler.cs:15-19`: `Application.OpenURL` opens a Google Forms survey. `ExitApplication` (:9-13) logs "Survey Ignored".
- AppCenter SDK and editor extensions are in `Assets/AppCenter*`. They are not referenced from `Assets/Natureous` (no `AppCenter` usage in own code; findings.md counts 130 .cs files in third-party code).

## Goal behind it
- **Stated (Trello):** "Analytics" ✅, "Smartlook" ✅, "Open browser on Survey when game ends" ✅ (board.md:63-73).
- **Stated (commits):** "Add Game Analytics SDK" (a0613ff), "Install more analytics and personalize the hints based on the platform" (fcfe7d1), "Add smartlook" (4cefffd).
- **Inferred:** gather playtest evidence (where players die or fall, whether they finish) for Release 0.2 (where-we-wanted-to-be.md §4).
- **Inferred:** AppCenter was meant for crash reporting or distribution. No commit or card states this.

## What it achieved / where it hurt
- Achieved: a level-progress funnel and session recordings for 0.2. Whether that data was ever reviewed is not recorded.
- Hurt:
  - Smartlook recording is always on, including in the editor and on desktop. Its cost is **Unknown (suspected MEDIUM)** (performance-bottlenecks.md Cascade 7).
  - Every log call goes through the uncached `Singleton` (Root Cause).
  - The recording key is hard-coded (where-we-wanted-to-be.md §4).
  - No consent or opt-out flow is visible in code.
  - Event taxonomy is hard-coded to a single level.

## Unity 6 status
(findings.md)
- AppCenter: the service was retired on 2025-03-31, and the SDK uses obsolete APIs. **Recommend removal.**
- GameAnalytics: the 2019-era SDK uses `WWW` and `isNetworkError`. A possible compile blocker if `WWW` is error-level in 6.6 (items 5, 6; UNVERIFIED).
- Smartlook: compile impact is low, but the SDK is probably out of date.
- PlayServicesResolver and the support-library 26 .aar files are an Android build risk.

## Probe question
How much frame time and memory does always-on Smartlook recording cost on a target mobile device?

Probe method (run later):
- Development build on one Android device.
- Run two identical Sandbox runs, with recording on and off (guard the call behind a flag in a throwaway branch).
- Capture Profiler CPU main-thread ms, GC alloc, and memory.
- Budget: half a day, assuming the SDK still builds. If it does not, the probe answers itself.

## Pending research
- Which analytics SDK (current GameAnalytics UPM package, Unity Analytics/UGS, Firebase, or none) is still maintained for Unity 6 and offline-friendly?
- Is session recording needed at all, or would heatmap-style custom events give enough insight?
- What consent and privacy flow do current store policies require for session recording?
- Should the analytics call sites go behind an interface (port) so that the SDK choice is swappable and the calls can be tested?

## Verdict
**REPLACE-CANDIDATE** for the SDKs: AppCenter is dead, and GameAnalytics and Smartlook are stale. **KEEP-CANDIDATE** for the practice of instrumenting playtests. Both are provisional.
