# Dreamy Foundation Verification Lab

## Revised implementation prompt

You are a Senior Unity Architect working directly in this repository. First inspect the installed Dreamy package source, package versions, samples, current scenes, Addressables groups, asmdefs, and uncommitted demo work. Do not invent APIs and do not overwrite unrelated local changes.

Design and implement a very small mobile-friendly **Framework Verification Lab**, not a production game. A complete manual or automated run must take less than three minutes and produce an explicit PASS, FAIL, or SKIP result for each capability.

Installed runtime packages are `com.dreamy.core`, `com.dreamy.assets`, `com.dreamy.dataconfig`, `com.dreamy.datasave`, and `com.dreamy.ui`. `com.dreamy.editor-tools` is editor-only. Third-party dependencies are UniTask, DOTween, LeanPool, Newtonsoft Json, and Addressables.

Use the package APIs that actually exist in the checked-out versions. Keep project-specific gameplay, popup prefabs, scene orchestration, Addressables setup, and composition-root wiring in the template project. Use JSON through Dreamy DataConfig for runtime balance data. Use ScriptableObject only for Unity authoring references that cannot live in JSON, such as prefab/address keys and scene references.

Apply SOLID, MVP at the UI boundary, a service layer, explicit composition in `GameInstaller`, and no new global singleton unless required by an existing Dreamy package API. Separate Domain, Application, Presentation, and Infrastructure responsibilities where the size justifies it. Every verification capability must be independently replaceable and report diagnostics.

Deliver: concept, loop, package coverage matrix, scenes, panels, services, configs, folders, asmdefs, bootstrap/save/scene/pool/popup/EventBus flows, verification cases, extension rules, mini GDD, implementation, and compile/test results. Do not spend implementation time generating UML or dependency diagrams. Mark unsupported or unconfigured checks as SKIP rather than faking success.

## 1. Concept

**Foundation Lab** is a one-screen diagnostics game. The player taps a target to gain coins, takes/heals damage, opens a result popup, saves a snapshot, reloads it, and switches scene once. A guided `Run All` sequence executes the same operations automatically and shows one row per framework capability.

The thin game fiction makes state changes visible but never becomes the architecture. The product is the verification report.

## 2. Minimal loop

1. Bootstrap services and load JSON config.
2. Enter `MainScene` through the loading screen.
3. Tap/spawn targets for 10-20 seconds.
4. Observe BindableProperty and EventBus updates.
5. Open/close a result popup.
6. Save, mutate state, load, and compare.
7. Display PASS/FAIL/SKIP summary and allow rerun.

## 3. Package coverage

| Feature | Packages verified |
|---|---|
| Bootstrap/composition | Core ServiceLocator, UniTask |
| Reactive HUD | Core BindableProperty |
| Score notification | Core EventBus |
| Runtime balance | DataConfig, Newtonsoft Json |
| Snapshot persistence | Datasave, Newtonsoft Json |
| Loading screen | Core LiveSingleton, UniTask, DOTween |
| Dynamic target asset | Assets, Addressables, UniTask |
| Target lifecycle | LeanPool through project `IPoolService` |
| Popup stack/animation | UI, Assets, Addressables, DOTween |
| Build/scene/config validation | Editor Tools and DataConfig editor menus |

`editor-tools` cannot be runtime-tested. Its verification is an Editor checklist or EditMode test.

## 4-7. Scenes, panels, services, config

Scenes: `Bootstrap` is the composition root and persistent loading UI; `MainScene` contains the lab canvas and target area. A third scene is unnecessary.

Panels: the Addressable `FoundationDemoPanel` and the existing `UILoadingScreen`. The root prefab owns a separate toggle button; hiding destroys the current panel and showing creates and binds a fresh panel through `PanelManager`.

Services used by the current demo: `IDataConfigService`, `IDatasaveService`, and `IPoolService`. The root keeps the tiny score/health state directly.

Configs:

- JSON `gameSettings.json`: numeric/read-only balance and timing.
- `DemoAssetCatalog : ScriptableObject`: Addressable keys for target and popup.
- `DemoSceneCatalog : ScriptableObject`: scene names or AssetReferences.
- `TemplatePlayerSave`: writable player progress, never design config.

## 8-9. Folder and assembly structure

```text
Assets/_Project/
  Runtime/
    FoundationLab/
      Domain/              # Session rules and result models; no UnityEngine
      Application/         # Use cases, ports, verification cases
      Presentation/        # Panels, presenters, MonoBehaviours
      Infrastructure/      # Dreamy, Addressables, LeanPool adapters
      Config/              # ScriptableObject authoring catalogs
      module-contract.md
    Bootstrap/
  Tests/EditMode/
  Tests/PlayMode/
  Scenes/
  Prefabs/UI/
  Prefabs/Gameplay/
  Docs/
```

Target asmdefs: `Dreamy.Template.FoundationLab.Domain` has no engine references; `Application -> Domain + UniTask`; `Infrastructure -> Application + Dreamy packages + Unity`; `Presentation -> Application + UI`; `Bootstrap -> Application + Infrastructure + Presentation`; EditMode tests reference Domain/Application, PlayMode tests reference Bootstrap/Presentation. The current single `Dreamy.Template.Runtime` asmdef remains a pragmatic starter until scene references are migrated.

## 10. Bootstrap flow

`Bootstrap scene -> GameInstaller.Awake -> register Datasave -> register Pool -> initialize DataConfig -> mark Ready -> GameInit applies settings -> optional smoke test -> SceneLoader loads MainScene`.

Failure is terminal for that run and must be displayed/logged with the original exception. Cancellation comes from the owning MonoBehaviour.

## 11-15. Runtime flows

Save/load: presenter requests save use case; use case maps session to `TemplatePlayerSave`; Datasave writes its versioned atomic envelope. Load replaces session values through methods so BindableProperty and EventBus still notify views. Save on pause/quit remains a fallback, not the only save trigger.

Scene loading: a scene port calls the existing loader; loader rejects concurrent loads, shows UI, reports normalized progress, delays activation until minimum display time, activates, then hides loading UI. Subscribers must not own the loader lifetime.

Pool: preload configured count; load target prefab through Dreamy Assets; spawn through `IPoolService`; `PooledBehaviour.OnSpawn` resets state; timeout/tap calls despawn; scene exit calls `DespawnAll`; asset release happens only after all instances are returned.

Popup: `PanelManager.Show<ResultPopup>(address)` loads the Addressable prefab, initializes/registers it, binds a presenter, animates show, and pushes it on the stack. Hide animates, unregisters, and destroys the instance. Escape/Android Back only closes a panel with `CanBack`.

The compact prefab demo updates its panel directly and intentionally does not depend on BindableProperty.

## 16. Verification cases

| Case | Pass condition |
|---|---|
| Bootstrap | required services resolve and state is Ready |
| DataConfig | JSON loads and values satisfy declared ranges |
| Bindable | one mutation produces one expected observer value |
| EventBus | registered probe receives payload; unregistered probe does not |
| Save | file exists after save and envelope can be loaded |
| Load | loaded values equal saved values after in-memory mutation |
| Scene | MainScene activates and progress is monotonic 0..1 |
| Addressables | configured key loads expected type and releases cleanly |
| Pool | preloaded instance spawns, resets, despawns, and respawns |
| Popup | popup shows, becomes top panel, closes, and unregisters |
| DOTween | show/hide tween completes or cancels without leaked tween |
| Pause save | simulated lifecycle invokes SaveAll |
| Editor tools | menus open; scene/build validation reports no error |

Missing Addressable keys/prefabs are SKIP with setup instructions. Exceptions, timeouts, wrong values, and leaked registrations are FAIL.

## 17. Extension model

Each future package contributes one `IVerificationCase`, optional UI row/prefab, and a package-specific adapter. Discovery may be an explicit ordered list in the composition root; avoid reflection in player builds. Results use a stable `{ Id, Status, Duration, Message }` contract. Package checks must not reference each other's concrete classes. Additive checks therefore do not change the runner or existing checks.

Every feature module should include `module-contract.md` listing commands, queries, consumed/emitted events, required ports, and internal types.

## 18. Clone, enable, and disable

The demo entry point is the prefab `Assets/_Project/Prefabs/FoundationDemo.prefab`.

- Enable: drag one prefab instance into `MainScene`.
- Disable: delete that prefab instance from `MainScene`.
- The root prefab waits for the shared bootstrap and opens `FoundationDemoPanel` through Dreamy UI using `Address.FoundationDemoPanel`.
- Create the panel prefab manually with `FoundationDemoPanel`, `CanvasGroup`, `TweenPlayer`, and the desired Dreamy UI tween components.
- Assign all serialized text/button references and register the prefab with address `Address.FoundationDemoPanel`.
- No scripting define, Editor toggle, Resources entry, Addressables preload label, or extra build scene is required.

This keeps the clone workflow visible and local to the scene. Without a prefab instance there is no demo entry point and no demo runtime allocation. The prefab and scripts remain small source assets in the repository; Unity only includes assets reachable by a built scene or an Addressables group. Keep future demo-only content under the prefab or a dedicated non-preloaded `FoundationDemo` Addressables group.

## 19. Mini GDD

Session length is 20-60 seconds. Tap a spawned target for configured coins. Damage/heal buttons demonstrate bounded health. Save records coins and high score. The result popup shows capability results. There is no loss condition, economy, progression, backend, or content pipeline. Success means all configured checks pass; skipped checks keep the run incomplete but usable.

## 20. Why this fits the template

The lab exercises real package boundaries while keeping replaceable adapters around static or Unity-specific APIs. JSON remains appropriate for remotely overridable balance; ScriptableObject remains appropriate for Unity asset references. Two scenes minimize maintenance and build time. Explicit service registration exposes ownership and disposal. MVP keeps package verification out of view code. PASS/FAIL/SKIP makes missing template setup visible instead of turning it into brittle gameplay behavior.

The approach follows the dependency rule: Presentation calls Application, Application owns ports, Infrastructure implements them, and Domain remains independent. It is a starter pattern; split the current assembly only after serialized scene references have been migrated and PlayMode coverage protects the change.
