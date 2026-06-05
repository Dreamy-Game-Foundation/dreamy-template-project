# Dreamy Template Project

Starter Unity project for Dreamy internal games.

## What This Template Wires

- `com.dreamy.core`
- `com.dreamy.datasave`
- `com.dreamy.assets`
- `com.dreamy.ui`
- `com.dreamy.editor-tools`
- UniTask
- DOTween
- Addressables
- Newtonsoft JSON

Current package references use Git URLs with `?path=` so the template can update packages from the shared repository:

```json
"com.dreamy.core": "https://gitlab.com/trinhtuu.05/unitybaseproject.git?path=/Packages/com.dreamy.core#dev"
```

For stable releases, replace `#dev` with a tag or commit hash.

## First Scene Setup

Create or open the bootstrap scene and add:

- `GameInstaller`
- `GameInit`
- `PanelManager` on the UI root canvas when UI is needed

Or generate it from Unity:

```text
Tools/Dreamy/Template/Create Bootstrap Scene
```

Scripts live under:

```text
Assets/_DreamyTemplate/Scripts
```

Detailed notes:

```text
Assets/_DreamyTemplate/Docs/README_TEMPLATE.md
```
