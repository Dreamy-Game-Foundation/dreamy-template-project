# Changelog

## [1.1.0] - 2026-10-22

- Moved MoveFade/ScaleFade presets out of Runtime into `Samples~/TweenPresets/Resources/Tween` and copied the six sandbox tween presets into the same sample.

- Pause playback before killing it so deferred DOTween removal cannot advance motion/fade after cancellation.
- Added MoveFade and ScaleFade effects for Auto/Manual tween players, importable timing presets in the Tween Presets sample, and input gating during transitions.

## [1.0.6] - 2026-10-16

- In Editor/Device Simulator, fall back to the current Screen bounds when reported safe-area pixels exceed those bounds, preventing UI anchors outside the viewport. Player behavior remains unchanged.
- Existing five PanelPresenterTests failures reproduce with the previous SafeArea implementation; this patch does not change presenter lifecycle.

## [1.0.5] - 2026-10-15

- Resume increasing release versions above the highest existing v1.x tag after the 0.2.0 version regression.
- Synchronize package metadata and installation documentation with v1.0.5; preserve all existing tags and historical changelog entries.
- Update the Dreamy Assets dependency to 1.0.1.

- Added a shared non-panel presenter host and optional ticking contract; activate panels before presenter Show for coroutine-based reveals.

- Added an engine-independent presenter factory/host and automatic UIPanel lifecycle integration, plus prefab Create/Show overloads.

- Removed effect-level stagger timing from `UITweenPlayer`. `TweenDelayControl`
  now owns show/hide group delays through `TweenDelayByIndex`.
- Added `UITweenPlayer`, a single Auto/Manual hybrid player that excludes
  effects owned by nested players and prunes destroyed references safely.
- Removed the duplicate entry-based tween system and the old `TweenPlayer`
  wrapper. Manual authoring now uses explicit target groups.
- Made legacy tween effects cancellation- and destroyed-target-safe, and moved
  post-hide notification before a non-cached panel is destroyed.
- Added per-show/per-hide timing override fields and player-level preset
  inheritance without `Resources.Load` defaults.
- Made tab initialization idempotent and wait for completion before auto-open.
- Added focused editor tests for tween ownership and destroyed effect pruning.
- Added `IPanelTransition` so panels resolve transitions by interface.

## [0.1.1] - 2026-06-15

- Made panel show/hide operations cancellation-safe and killed cancelled tweens.
- Deduplicated panel creation by panel type.
- Replaced persistent transition callbacks with manager-owned transition state.
- Serialized transitions and created full-stretch ordered layer roots.

## [0.1.0] - 2026-06-06

- Added UI package scaffold.
- Added `UIPanel` and `PanelManager`.
- Added button helpers.
- Added tab helpers.
- Added tween helpers.
- Added safe area and scalable pointer helpers.
