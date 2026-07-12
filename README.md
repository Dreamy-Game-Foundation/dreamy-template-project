# Dreamy Template Project

Unity template project for internal Dreamy mobile games. This repo is the
composition root for Dreamy packages and a working sample that shows how to wire
bootstrap, config, save, asset loading, UI, audio, and pooling together.


## What This Template Provides

- Unity 6000 mobile-ready starter project.
- Bootstrap scene and main scene flow.
- Dreamy package integration through Unity Package Manager.
- Sample service registration with `GameInstaller`.
- Sample config loading from `Resources/DataConfig`.
- Sample save/load flow with Dreamy Datasave.
- Sample UI panels using Dreamy UI and Addressables.
- Sample audio startup using Dreamy Audio.
- LeanPool adapter for pooled runtime/UI objects.
- Project conventions for mobile game production.

## Project Structure

```text
Assets/_Project
  Docs              Team handbook and project docs
  Scenes            BootstrapScene and MainScene
  Scripts
    Bootstrap       GameInstaller, GameInit, SceneLoader, address constants
    Config          Sample DataConfig rows/tables
    Demo            Demo root and demo panel flow
    Pooling         LeanPool adapter and pool helper
    Save            Sample save data
    UI              Shop/resource UI sample
  Prefabs           Loading screen, demo panel, shop panel, UI items
  Textures          Project UI/game textures
  SpriteAtlas       Sprite atlases
```

Reusable framework code should live in Dreamy packages. `Assets/_Project` should
stay focused on game-specific composition, samples, scenes, prefabs, and glue code.

## Dreamy Packages

| Package | Purpose | Typical usage |
| --- | --- | --- |
| `com.dreamy.core` | Foundation primitives: `ServiceLocator`, event bus, state machine, singleton helpers | Register and resolve shared services, dispatch decoupled events |
| `com.dreamy.datasave` | Versioned JSON save/load with migration support | Save player progress, settings, inventory, tutorial state |
| `com.dreamy.dataconfig` | Typed JSON config tables and validation | Load level, economy, reward, offer, shop, and tuning data |
| `com.dreamy.assets` | Addressables/Resources asset loading helpers | Load UI prefabs, sprite atlases, gameplay assets by key/address |
| `com.dreamy.ui` | UI panel lifecycle, panel manager, tabs, buttons, tween helpers | Show/hide panels and build reusable mobile UI flows |
| `com.dreamy.audio` | Catalog-driven audio playback and audio components | Play music/SFX through audio keys and profiles |
| `com.dreamy.editor-tools` | Unity editor utilities | Improve authoring, validation, and team workflows |

Third-party packages used by the template include UniTask, DOTween, Addressables,
TextMeshPro, UGUI, Newtonsoft Json, and LeanPool.

## Run The Template

1. Open the project with the Unity version used by the team template.
2. Wait for Unity Package Manager to restore packages.
3. Open `Assets/_Project/Scenes/BootstrapScene.unity`.
4. Enter Play Mode.
5. `GameInstaller` registers services, `GameInit` waits for bootstrap readiness,
   then `SceneLoader` loads `MainScene`.

Scene flow:

```text
BootstrapScene
  -> GameInstaller initializes services
  -> GameInit waits for Ready state
  -> SceneLoader loads MainScene
  -> FoundationDemoRoot shows demo UI
```

## Bootstrap And Service Usage

`GameInstaller` is the composition root. Add global services there or in a dedicated
installer called by it.

Example pattern:

```csharp
private void RegisterServices()
{
    IDatasaveService datasave = CreateDatasaveService();
    ServiceLocator.Register<IDatasaveService>(datasave);

    IDataConfigService dataConfig = CreateDataConfigService();
    ServiceLocator.Register<IDataConfigService>(dataConfig);

    IPoolService pool = new LeanPoolService();
    ServiceLocator.Register<IPoolService>(pool);
}
```

Use `ServiceLocator` only from bootstrap, feature roots, presenters, or top-level
controllers. Avoid resolving services inside small leaf components such as UI item
views, VFX instances, and pooled objects. Pass data/dependencies into those objects
through `Initialize(...)`.

## Data Config Usage

Use `com.dreamy.dataconfig` for static, read-only game data:

- level config
- offer/shop config
- reward tables
- enemy stats
- economy tuning

Config files are stored under:

```text
Assets/Resources/DataConfig
```

Example config row:

```csharp
public sealed class OfferConfig : DataConfigRow
{
    public string Name { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
    public int Price { get; set; }
}

public sealed class OfferConfigTable : DataConfigTable<OfferConfig>
{
}
```

Usage from a feature root:

```csharp
IDataConfigService configService = ServiceLocator.Get<IDataConfigService>();
OfferConfig offer = configService.GetTable<OfferConfigTable>().Get("starter_pack");
```

Do not parse JSON directly inside UI panels.

## Datasave Usage

Use `com.dreamy.datasave` for persistent player state:

- currencies
- inventory
- progress
- settings
- tutorial state
- daily reward state

Example save data:

```csharp
public sealed class PlayerSave : SaveData
{
    public int Coins { get; set; }
    public int Gems { get; set; }
    public int CurrentLevel { get; set; }
}
```

Rules:

- Save IDs/keys, not Unity object references.
- Save after important transactions and on app pause.
- Keep save schemas versioned.
- Add migration when changing released save structure.

## UI Usage

UI panels are loaded and controlled through Dreamy UI.

Address constants are centralized in:

```text
Assets/_Project/Scripts/Bootstrap/Address.cs
```

Example:

```csharp
UIShopPanel panel = await PanelManager.Instance.Show<UIShopPanel>(Address.ShopPanel);
```

UI rules:

- Use `UIPanel` for panels.
- Keep UI panels focused on view/input binding.
- Move business logic into presenters/services when it grows.
- For dynamic lists, despawn pooled items through the same pool that spawned them.
- Validate mobile safe area, aspect ratios, and tap target sizes.

## Asset Loading Usage

Use Addressables for runtime-loaded assets such as UI prefabs, sprite atlases,
gameplay prefabs, and audio/catalog assets.

Current sample addresses:

```csharp
public const string FoundationDemoPanel = "Panel/FoundationDemoPanel.prefab";
public const string ShopPanel = "Panel/UIShopPanel.prefab";
public const string ShopOfferAtlas = "SpriteAtlas/ShopOfferAtlas.spriteatlasv2";
```

Rules:

- Centralize address strings.
- Load asynchronously.
- Preload critical UI/audio before first use.
- Release assets according to the loader/package contract.

## Audio Usage

Dreamy Audio provides catalog/profile-driven playback.

Example:

```csharp
DreamyAudio.PlayMusic(new AudioKey("core", "music.main"));
DreamyAudio.PlaySfx(new AudioKey("ui", "button.click"));
```

Rules:

- Use audio keys instead of direct clip references in gameplay code.
- Separate music, SFX, and UI sounds.
- Validate missing keys before release.
- Save user volume/mute settings through Datasave.

## Pooling Usage

LeanPool is available through a project adapter.

Example:

```csharp
IPoolService pool = ServiceLocator.Get<IPoolService>();
GameObject instance = pool.Spawn(prefab, position, rotation);
pool.Despawn(instance);
```

Use pooling for frequently spawned objects such as VFX, floating text, projectiles,
coin fly items, enemies, and dynamic UI list items.

If an object is spawned by pool, return it by pool. Do not mix pool spawn with
`Destroy`.

## Mobile Defaults

Recommended baseline:

- IL2CPP.
- Android ARM64.
- Incremental GC enabled.
- Addressables for runtime content.
- Sprite atlases for UI.
- Compressed textures and audio per platform.
- Event-driven UI updates instead of polling.
- Object pooling for high-frequency spawn/despawn.
- Test on low-end Android devices before release.

## Development Rules

- Keep reusable framework logic in Dreamy packages.
- Keep project-specific glue in `Assets/_Project`.
- Do not edit `Library/PackageCache` as a permanent fix.
- Keep `.meta` files with assets.
- Keep asmdef references explicit and minimal.
- Do not let runtime assemblies reference editor assemblies.
- Update `Packages/manifest.json`, `packages-lock.json`, and docs together when
  package dependencies change.

