# Template reward source rehearsal

Source-only B9/C18/F27 handoff, 2026-10-14. This is not a Unity import,
runtime, release, rollback, or production certification.

Consumer Git root: `D:/Game/dreamy-base-project/DreamyTemplateProject`.
Baseline HEAD: `f6486d0e8fa7e877360d7b775c96ad419474fe0f`.
Branch: `feature/reward-evolution-20261014`. No commits created.
The unrelated `Assets/generation-report.json` and `.meta` are preserved.

## Composition and signatures

The four manifest overrides are `file:../../../dreamy-package-sandbox/LocalPackages/<package>`,
relative to this consumer's `Packages` directory. Feature, Economy, Daily Reward,
and Missions use local working source, not a release pin. Unity must resolve
`Packages/packages-lock.json`; it was deliberately not edited.

`GameInstaller.InstallEconomy()` still constructs exactly one
`DatasaveResourceWallet(IDatasaveService, string saveKey = "economy",
IReadOnlyList<ResourceAmount> initialBalances = null, int transactionHistoryLimit = 4096)`.
The same instance supplies the existing Shop wallet and balance provider.
It is the sole `IRewardDestination` in
`RewardDeliveryGateway(IReadOnlyList<IRewardDestination>)`, registered as
`IRewardDeliveryGateway`. No fake item owner or legacy bool-wallet adapter is
used by Daily/Missions. A mixed currency/item bundle fails validation before delivery.
The reference currency owner accepts syntactically valid resource IDs; this
template does not add a separate currency allowlist.

Config registration precedes `IDataConfigService.InitializeAsync`.
`TrackFeatureInstallation<T>` captures prior registrations and restores them on
reverse-order teardown. Panel factories capture the same services, not fresh
models on each opening. Gateway registration is removed before the wallet owner.

The source-adopted installers retain current package sample signatures:

```csharp
IDailyRewardService DailyRewardFeatureInstaller.Install(
    PanelPresenterFactory factory, DailyRewardScheduleConfig config,
    IDatasaveService save, IRewardDeliveryGateway gateway, IRewardClock clock,
    string saveKey = DailyRewardInstaller.DefaultSaveKey,
    IDailyRewardFeedback feedback = null);
IMissionService MissionFeatureInstaller.Install(
    PanelPresenterFactory factory, MissionCatalogConfig config,
    IDatasaveService save, IRewardDeliveryGateway gateway,
    string saveKey = "missions", IMissionClock clock = null,
    IMissionRandom random = null);
void MissionPanel.ConfigureControls(IMissionService service);
```

Daily uses `SystemRewardClock`, save key `daily-reward`, stable schedule
`template-daily-seven-day`, revision `1`, UTC, seven days, StopAfterLastDay,
reset-on-missed=true. Coins total 1400; final milestone is 10 gems.
Missions uses save key `missions`, stable catalog `template-permanent-missions`,
revision `1`, explicit Permanent, no selection, zero free rerolls. The sample
events are `stage.win` and `coin.collect`. Demo controls simulate these events;
they are not a gameplay bridge or an authority for actual coin collection.
Shop billing remains the existing simulated fixture.

The selected sample C# files live in `Assets/_Project/Scripts/Features/` and
compile into `Dreamy.Template.Runtime`. Do not import another copy of the
sample scripts or their asmdefs. InMemoryResourceWallet, bundle fake-item
sample, periodic sample, LuckyWheel, Tutorial, and Progression were not adopted.

## Exact Unity MCP handoff

New component script GUIDs (use these adoption scripts, not sample GUIDs):

| Component | Script GUID |
| --- | --- |
| DailyRewardPanel | a8542ad9423549799beb8d1fa04fbecf |
| DailyRewardRewardItem | d6e265def2ce464f9062aa1f8070b416 |
| DailyRewardAudioFeedback | 54ae1cb403d14b50ac40b23fecf01bb7 |
| MissionPanel | 602b821b09fe43b5b7d523c0cc4d446b |
| MissionItem | 34f14f149e3849ffb21535856a593917 |
| MissionDemoControls | 7bb6178e5ff74bc8836499708404ba06 |

Root owns all following serialized changes. Do these only in the consumer root
above, through Unity, not YAML editing. Back up the entire save generation
before smoke tests; use disposable test saves for claims.

1. Resolve the manifest, refresh scripts, inspect the console, and let Unity
   write the lock. Confirm four local resolved paths and no duplicate assemblies.
2. Preserve `Assets/_Project/Prefabs/Panel/HomePanel.prefab`, its script/GUID,
   Shop/Settings/Demo bindings and cached Home behavior. Add two Button children.
   Bind `HomePanel.dailyRewardButton` to Daily and `HomePanel.missionsButton`
   to Missions. Code owns listeners; do not add persistent onClick callbacks.
3. Create `Assets/_Project/Prefabs/Panel/DailyRewardPanel.prefab` with component
   `Dreamy.Feature.DailyReward.Integration.DailyRewardPanel`.
   Bind serialized fields exactly:
   - `claimButton`: Button (also the pending-claim retry action).
   - `closeButton`: Button; no persistent listener.
   - `statusText`: TMP_Text.
   - `rewardContainer`: RectTransform content parent.
   - `rewardItemPrefab`: the DailyRewardRewardItem component on the item below.
   Create `Assets/_Project/Prefabs/Panel/DailyRewardItem.prefab` with component
   `Dreamy.Feature.DailyReward.Integration.DailyRewardRewardItem` and assign
   `dayText`, `amountText`, `stateText` to TMP_Text children.
   Use the existing template panel canvas/safe-area/layer conventions.
4. Create `Assets/_Project/Prefabs/Panel/MissionPanel.prefab` with component
   `Dreamy.Feature.Missions.Integration.MissionPanel`. Bind `closeButton`
   (Button), `statusText` (TMP_Text), `itemContainer` (RectTransform content),
   and `itemPrefab` (MissionItem component below).
   Create `Assets/_Project/Prefabs/Panel/MissionItem.prefab` with component
   `Dreamy.Feature.Missions.Integration.MissionItem`. Assign `titleText`,
   `progressText`, `rewardText`, `claimLabel` (TMP_Text) and `claimButton` (Button).
5. On the same MissionPanel root, add
   `Dreamy.Feature.Missions.Integration.MissionDemoControls`. Assign
   `winStageButton`, `collectCoinsButton` (Button) and `statusText` (TMP_Text).
   Leave `rerollButton` and `refreshButton` unassigned for Permanent mode.
   The factory calls `ConfigureControls(service)` before presenter Show.
   No scene installer or duplicate service component is required.
6. Add the two panel prefabs to the template's existing Addressables owner:
   Daily key `Panel/DailyRewardPanel.prefab`, Missions key `Panel/MissionPanel.prefab`.
   Do not change Shop/Settings keys or add separate addressable entries for rows.
   Home uses `PanelManager.Show`, so Daily/Missions overlay the existing Home.
   `CanBack=true`; close calls Hide; PanelManager owns presenter disposal.
7. Optional audio is not wired by this source rehearsal. The adopted
   `DailyRewardAudioFeedback` requires `claimedAudioId` and `failedAudioId`
   if used. Current installer intentionally passes no feedback rather than
   claiming an audio binding exists.

Package sample prefabs may serve as visual references only:
`com.dreamy.feature.daily-reward/Samples~/Daily Reward Feature/Prefabs/`
and `com.dreamy.feature.missions/Samples~/Mission Feature/Prefabs/`.
Their script GUIDs are not this adoption's new GUIDs; blindly copying them
does not bind the new components. Recreate/rebind explicitly through Unity.

## Authored tests and unrun gates

Source checks actually performed: all four local paths resolve to package.json
with matching package names; 11 consumer `_Project` JSON/asmdef documents parse,
with no repeated assembly references; all 629 Assets metadata GUIDs are unique;
all 12 new source/config/test files have metadata. `git diff --check` returned
exit 0. The current lock blob equals HEAD:
`e4a5b9cbd21d5dc0322f085d532932e565863ca6`. The lock still contains the old
Feature/Economy Git pins and has no Daily/Missions entries, as expected before
root Unity resolution. Package versions in local package.json remain
0.1.0/0.2.1/0.1.0/0.1.0; these are working-source rehearsals, not new releases.

`Assets/_Project/Tests/Editor/RewardFeatureIntegrationTests.cs` has four
file-backed tests for seven-day Stop completion/recreation, missed-day reset,
Permanent progress/claim/event dedup with the shared wallet and legacy Shop
exchange, and pre-delivery mixed-kind rejection. Config comes from the shipped
Resources documents. `RewardPresenterLifecycleTests.cs` has two synchronous
100-open/close factory-host tests for listener release. No sleeps or polling.

Root must run these six tests and existing template EditMode tests, especially
InstallerOwnershipTests, HomePresenterLifecycleTests, ShopBillingTests and
TemplateSaveTests. Also run package focused/suite tests on resolved source.
Required remaining gates: fresh resolve/import, C# compilation and console,
EditMode/PlayMode, missing scripts/references, Home -> Daily/Missions -> Back
repeatedly with one shared wallet and intact Shop/Settings, actual 100-cycle
panel/Addressables ownership, desktop/mobile screenshots and safe area,
player compile. Clock-dependent claims should use controlled test clocks,
not real-time sleeps. Periodic UI/sample proof remains separate from this
Permanent consumer installation.

No Unity operations, prefab/scene/Addressables edits, lock edits, package-source
edits, parent-repository edits, SDK calls, commits, tags, merges, or publication
were performed by this source-only worker. LSP diagnostics were attempted but
the daemon was unreachable. Authored tests have not been executed here.
