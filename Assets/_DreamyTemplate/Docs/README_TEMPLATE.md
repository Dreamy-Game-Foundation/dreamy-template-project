# Dreamy Template Project

This Unity project is the composition root for Dreamy internal packages.

## Current Local Package Setup

This template uses Git URL package references with `?path=` so it can consume packages from the shared repository:

```json
"com.dreamy.core": "https://gitlab.com/trinhtuu.05/unitybaseproject.git?path=/Packages/com.dreamy.core#dev",
"com.dreamy.datasave": "https://gitlab.com/trinhtuu.05/unitybaseproject.git?path=/Packages/com.dreamy.datasave#dev",
"com.dreamy.assets": "https://gitlab.com/trinhtuu.05/unitybaseproject.git?path=/Packages/com.dreamy.assets#dev",
"com.dreamy.ui": "https://gitlab.com/trinhtuu.05/unitybaseproject.git?path=/Packages/com.dreamy.ui#dev",
"com.dreamy.editor-tools": "https://gitlab.com/trinhtuu.05/unitybaseproject.git?path=/Packages/com.dreamy.editor-tools#dev"
```

When the packages are split into their own repositories, replace monorepo `?path=` URLs with package repository URLs:

```json
"com.dreamy.core": "https://github.com/Dreamy-Game-Foundation/com.dreamy.core.git#v2.0.0",
"com.dreamy.datasave": "https://github.com/Dreamy-Game-Foundation/com.dreamy.datasave.git#v0.1.0",
"com.dreamy.assets": "https://github.com/Dreamy-Game-Foundation/com.dreamy.assets.git#v0.1.0",
"com.dreamy.ui": "https://github.com/Dreamy-Game-Foundation/com.dreamy.ui.git#v0.1.0",
"com.dreamy.editor-tools": "https://github.com/Dreamy-Game-Foundation/com.dreamy.editor-tools.git#v0.1.0"
```

## Bootstrap

Add these components to the first scene:

- `GameInstaller`: registers shared services.
- `GameInit`: smoke-tests service registration and save/load.
- `PanelManager`: add this to the UI root canvas when using `com.dreamy.ui`.

You can generate the default scene from Unity:

```text
Tools/Dreamy/Template/Create Bootstrap Scene
```

## Service Order

1. Core primitives are available from package load.
2. `GameInstaller` registers `IDatasaveService`.
3. Game-specific services should register after datasave.
4. UI panels can use `PanelManager`.

## Addressables Convention

Recommended groups:

- `UI`
- `Audio`
- `Config`
- `Gameplay`

Recommended addresses:

- `ui_main_menu`
- `ui_loading`
- `cfg_level_table`
- `sfx_click`

## LeanPool

LeanPool is intentionally not listed in `manifest.json` yet because the template needs a real package URL or a mirrored private package. Add it here once the team has a stable source:

```json
"com.cwilkes.lean.common": "<team mirrored LeanCommon URL>",
"com.cwilkes.lean.pool": "<team mirrored LeanPool URL>"
```
