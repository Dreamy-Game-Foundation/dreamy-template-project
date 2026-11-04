# Changelog

## [Unreleased]

## [0.3.0] - 2026-11-03

Source version prepared locally; no v0.3.0 tag or published release created. Existing v0.2.0–v0.2.2 tags are preserved.

### Added

- Add game-owned SettingsLanguageIcons sample data and optional TMP option/caption images; text-only dropdowns remain supported without an icon catalog.

- Optional ISettingsLanguageGateway, ISettingsLanguageService and ISettingsLanguageView contracts for locale options, selection, busy/error state and change notifications.
- SettingsModel/Presenter/Installer support for injected language capability with owner cancellation and reusable presenter lifecycle. Existing ISettingsService/ISettingsView contracts remain source-compatible.
- A real TMP language selector in the Settings Feature sample prefab, 64px rows, bounded scrolling, label wiring and hidden initial state.
- StandaloneSettingsView for feature test scenes using SettingsPresenter without PanelManager/UIPanel lifecycle.

### Changed

- Hide language dropdown, label and error when no gateway/options are available; clear stale option state and keep other settings usable.
- Keep Localization out of Settings Runtime dependencies. The reusable localization adapter is imported separately from Dreamy Localization's Settings Integration sample.
- Document package-owned code versus game-owned imported sample assets, optional integration and upgrade workflow.

### Validation

- Sandbox localization regression: 41 EditMode / 17 PlayMode passed, including icon/text-only options, caption updates, no-gateway behavior on the actual SettingsPanel view and live switching/reopening through the standalone scene.
- Source metadata reflects this feature update. No player/device release validation is claimed.

## Earlier releases

Existing v0.2.0, v0.2.1 and v0.2.2 repository tags predate this changelog. Their history is preserved; this file does not reconstruct unverified historical release notes.
