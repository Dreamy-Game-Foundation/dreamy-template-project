Status: complete (test completed; automatic-save limitation remains)

Scope: Build Report Tool v3.8.2 smoke build and report verification on Unity 6000.4.12f1, Android IL2CPP APK, existing enabled BootstrapScene/MainScene.
Baseline commit: b88e3b39847f55dcca4e6c2e30667dd9e3ff5d5d. Existing QuickNav Preferences change and newly imported Assets/Packages preserved.
Preflight: correct Editor project confirmed; BootstrapScene clean, no prefab stage/play mode, zero Console errors. Imported tool has obsolete API warnings.
Build job: build-ddfcd0158f. Output: Builds/BuildReportSmoke-20261110/DreamyTemplate.apk.
Observed potential failure: tool selects nonexistent /home/tutrinh/Library/Logs/Unity/Editor.log on Linux; actual log is /home/tutrinh/.config/unity3d/Editor.log. Awaiting build/report receipt.
Workflow/lessons degraded: instructed docs/agents/dreamy-workflow.md and installed dreamy CLI unavailable (also recorded by prior upgrade checkpoint); no matching lesson store found. No background model calls.

Confirmed failure: first build produced APK but report callback threw DirectoryNotFoundException. Fixed Linux current/previous log resolution in BRT_Util.cs; recompile passed. Second build build-f610c440f6 succeeded in 38.20 s, 0 errors/13 warnings, generated all four reports and opened window. Deserialize and APK size checks passed. Incremental build did not emit fresh Used Assets section; clean build required to validate fresh asset inventory.

Final clean build: build-e9288b2d62 succeeded in 267.97 s, 0 errors / 29 warnings. CleanBuildCache and DetailedBuildReport confirmed from live BuildReport. MCP temporarily disconnected across Editor reload, then reconnected to the same target and recovered the job receipt.

Verification: APK 54,439,548 bytes (tool displays 51,92 MB), signature passes apksigner; Android manifest and ARM64 libil2cpp.so present. Report round-trip through the package loader passed: correct project/Android/APK, two enabled scenes, 3,173 used entries, 520 unused entries, 9 size categories. Dependencies, texture data, and Unity build report XML all deserialize. Fresh asset section follows clean-build log marker. Final Console: zero errors. No device run performed.

Automatic-save limitation: incremental build saved all reports automatically, but after the clean build the window had IsFinishedGettingValues=true and ShouldSaveGottenBuildReportNow=true without completing its OnInspectorUpdate save. Bringing the window forward programmatically did not complete it in this session. Used the documented public ReportGenerator.CreateReport(enabledScenes, outputPath, platform) API to complete and verify the clean report; pending save is now false. Do not claim fully reliable automatic saving or visual/native-window verification from this receipt. This is an observed unresolved limitation, not proven fixed.

Incremental asset-inventory limitation: Unity logs no fresh Used Assets list when player data is unchanged; the tool can reuse the earlier section. Use clean builds for a fresh asset inventory.

Changes: Linux log resolution in the imported Editor-only BRT_Util.cs; tool-created ProjectSettings/BuildReportToolOptions.xml; this evidence directory. Existing user QuickNav changes preserved, no manifest/lock/scene/meta edits by this task. Unity-generated PerformanceTestRun JSON resources were cleaned up by their build callback.

Reports: /home/tutrinh/UnityBuildReports/dreamy-template-project-Android-2026Thg 1110-111818.xml and DEP-/TextureData-/UBR- companions. APK: Builds/BuildReportSmoke-20261110/DreamyTemplate.apk.
