# Dreamy Template Project

Starter project for internal Dreamy Unity games.

## Included

- Git-based Dreamy packages: Core, DataSave, DataConfig, Assets, UI, and Editor Tools
- UniTask, DOTween, Addressables, Newtonsoft JSON, and LeanPool
- Bootstrap service registration through `GameInstaller`
- `IPoolService` backed by LeanPool
- Runtime Home panel and Tap Rush demo
- JSON game settings and persistent high score

## Run

Open `Assets/_Project/Scenes/Bootstrap.unity` and enter Play Mode. The bootstrap registers services, loads `MainScene` through `SceneLoader`, then creates the Home panel and Tap Rush demo.

## Scene Flow

```text
Bootstrap -> initialize services -> load MainScene -> show Home/demo
```

`SceneLoader.LoadAsync` reports normalized progress through `ProgressChanged`, so a game-specific loading screen can subscribe without changing `GameInit`.

## Pooling

Game code depends on `IPoolService`; LeanPool remains a template dependency rather than part of `com.dreamy.core`.

```csharp
IPoolService pool = ServiceLocator.Get<IPoolService>();
GameObject instance = pool.Spawn(prefab, position, rotation);
pool.Despawn(instance);
```

Preload shared prefabs during bootstrap or feature initialization:

```csharp
pool.Preload(prefab, preloadCount: 8, capacity: 16);
```

## Project Defaults

- Asset serialization: Force Text
- Color space: Linear
- Incremental GC: enabled
- Input handling: Both
- Android: ARM64 and IL2CPP
- First build scene: Bootstrap

Scene creation and template validation are intentionally manual. The template does not install editor commands that create or repair scenes.
