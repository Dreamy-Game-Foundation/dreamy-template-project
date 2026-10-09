# Settings Feature

GameInstaller installs SettingsFeatureInstaller into the shared PanelPresenterFactory after audio/config/save readiness. SettingsPanel and RateUsPanel are passive views. PanelManager creates one presenter per opening and disposes it on hide, disable, destroy or failed show. Cached reopen creates a fresh presenter.

Home opens `Address.SettingsPanel`; the Settings presenter calls the root callback to open `Address.RateUsPanel`. GameInstaller attaches the factory to the current scene and newly loaded scenes before their Start methods run. SettingsPanelLauncher and RateUsPanelLauncher remain as empty compatibility components to preserve existing prefab GUID references; they do not bind presenters.

The host SettingsPlatformGateway keeps all platform capabilities unavailable until an SDK is connected. Inject ISettingsHapticsGateway and ISettingsReviewGateway before installation if the game supports those features. The optional haptics toggle is unassigned on existing prefabs. The legacy store button reference is preserved and hidden; store actions are handled by Rate Us.

See [the adapter README](../../../../Scripts/Settings/README.md) for SDK ownership and result mapping. Keep Runtime assemblies free of Editor references.
