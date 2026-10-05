# Settings Feature

`GameInstaller` registers `DreamyAudio.Service` as `IAudioService` and installs `ISettingsService` before Home navigation becomes available.

The Home Settings button opens `Panel/SettingsPanel.prefab` through `PanelManager.Transition`. Settings opens `Panel/RateUsPanel.prefab` the same way. Both prefabs are registered in the Default Local Group. Their launchers bind presenters to `OnPreShow` and release bindings on hide/destroy; `PanelManager` owns initialization, transitions and back navigation. Do not attach `SettingsController` to the same prefab as `SettingsPanelLauncher`.

The project Audio Profile contains persistent Music/SFX buses and uses the imported Audio Library. Assign mixer groups and exposed volume parameters to the buses to apply volume changes to currently playing audio.

Register a real `ISettingsPlatformGateway` before `SettingsInstaller.Install()` to enable platform actions. Without a gateway, GDPR, Restore Purchases and Open Store stay hidden. Native review requests return unavailable. The host adapter is `SettingsPlatformGateway`; its capabilities remain unavailable until DreamySDK is connected.

## Host adapter (2026-10-05)

The imported SimulatedSettingsPlatformGateway has been removed from this host.
GameInstaller now registers the host SettingsPlatformGateway before installing
Settings. Settings and Rate Us share that gateway. DreamySDK hooks, capability
flags and result mapping are documented in
[the adapter README](../../../../Scripts/Settings/README.md).
Until SDK integration is implemented, platform capabilities remain unavailable.
