# Dreamy Template Project

Starter project for internal Dreamy Unity games.

## Included

- Git-based Dreamy packages: Core, DataSave, DataConfig, Assets, UI, and Editor Tools
- UniTask, DOTween, Addressables, Newtonsoft JSON, and LeanPool
- Bootstrap service registration through `GameInstaller`
- Minimal core demo sample for `ServiceLocator`, `EventBus`, and `BindableProperty`
- JSON game settings and persistent high score

## Run

Open `Assets/_Project/Scenes/Bootstrap.unity` and enter Play Mode. The bootstrap registers only the services the demo needs, then loads `MainScene` through `SceneLoader`.

## Scene Flow

```text
Bootstrap -> initialize services -> load MainScene -> demo HUD
```

`CorePackDemoController` is a drop-in sample for the main scene. It shows:

- `ServiceLocator` registration of a demo session service
- `BindableProperty` for score and health
- `MyEventBus<T>` for score change notifications
- `DataConfig` read from `Resources/DataConfig/gameSettings.json`
- `Datasave` load/save of `TemplatePlayerSave`

Wire the button refs in `CorePackDemoController` to `Add Score`, `Use Config`, `Damage`, `Heal`, `Save`, `Load`, `Apply FPS`, and `Reset`. The controller stays small on purpose; the scene owns the layout.

## Pooling

LeanPool remains available as a template dependency if your game needs pooling, but the demo flow no longer depends on `IPoolService`.

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

Scene creation and template validation are intentionally manual. The template does not install editor commands that create or repair scenes, and it no longer auto-spawns demo UI objects at runtime.
