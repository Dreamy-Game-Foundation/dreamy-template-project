# Dreamy Settings

Author: **Dreamy Game Studio**. Source version: **0.3.0** (prepared locally; no v0.3.0 tag/publish yet). See [CHANGELOG](CHANGELOG.md).

Runtime services and presenters for Music/SFX volume, haptic preferences, and host-owned consent, purchase restore, and Rate Us actions. Runtime has no UnityEngine/UnityEditor references. The optional Settings Feature sample provides MVP views, presenter factories, and explicit host adapters. It contains no per-feature demo, launcher, or controller classes.

## Installation

Implement your game's platform, haptic, and review gateways, then install at the composition root after audio and save initialization:

```csharp
ServiceLocator.Register<ISettingsPlatformGateway>(platformGateway);
ServiceLocator.Register<ISettingsHapticsGateway>(hapticsGateway);
ServiceLocator.Register<ISettingsReviewGateway>(reviewGateway);
ISettingsService settings = SettingsInstaller.Install();
```

Explicit dependencies are also supported:

```csharp
ISettingsService settings = SettingsInstaller.Install(
    audioService, platformGateway, hapticsGateway, reviewGateway);
```

A platform adapter may implement the optional haptic/review interfaces itself. Without a supported haptic gateway, `CanSetHaptics` is false and requests are ignored. Without a review gateway, opening the store remains available but no rating state or reward is persisted. Production hosts should supply the review gateway when persistence is required.

## Custom UI

Implement `ISettingsView` and `IRateUsView` in the game's UI and pass them to `SettingsPresenter` and `RateUsPresenter`. Call `Show()` when opening the view and `Dispose()` when closing or destroying its owner. The host owns UI creation, layout, transitions, and navigation. Reopening a disposed presenter rebinds it.

Settings views render `SettingsViewState`, including `HapticsEnabled`, `CanSetHaptics`, `CanRequestReview`, `HasPositiveRating`, and `HasClaimedReviewReward`. Emit `HapticsEnabledChanged` from the toggle. The haptic gateway owns persistence and applying the preference to the actual haptic provider; Settings does not produce vibration itself.

The UI needs only one Rate Us action. Custom rating views use `SubmitRatingAsync(int rating, CancellationToken)` for the store/persistence/reward flow. The legacy `RequestReviewAsync` service method opens the listing directly and bypasses rating persistence/rewards. The host `OpenStoreAsync` reports success only when it has successfully launched the listing.

## Review state and optional reward

The Rate Us presenter closes on ratings 1–3. Ratings 4–5 open the store listing and then call `ISettingsReviewGateway.RecordPositiveRating`. Save this as a local positive rating/request, not a verified published review: Google Play [does not report whether a user submitted a review](https://developer.android.com/guide/playcore/in-app-review/unity).

Persist `HasPositiveRating` and `HasClaimedReward` in the game's account-scoped Datasave/backend owner. Once a positive request is saved, the flow avoids reopening the store. `CanRequestReview` suppresses repeat requests while still allowing a pending optional reward to retry. Runtime rejects concurrent submissions, freezes the submitted rating, and passes lifetime cancellation to the host.

Rewards are opt-in through `ISettingsReviewGateway.CanGrantReward` and `GrantRewardOnceAsync`. Keep `CanGrantReward` false by default. The host decides platform eligibility (for an Android-only reward, return false in Editor, iOS, and other players). Persist a fixed transaction ID together with the wallet mutation, and return success for an already processed transaction. Only set the claimed flag after that durable grant; failed grants remain retryable without reopening the store. `PositiveRatingSubmitted` is a notification and must not grant an additional reward.

Google Play [prohibits incentivized reviews/ratings](https://support.google.com/googleplay/android-developer/answer/9898684?hl=en). Keep review-linked rewards disabled in Play builds; any feedback reward must be independent of publishing or selecting a positive store review.

## Settings Feature integration

Production integration for the supplied passive SettingsPanel and RateUsPanel views. At the composition root, after audio/save readiness:

```csharp
var factory = new PanelPresenterFactory();
SettingsFeatureInstaller.Install(factory, audioService, OpenRateUs, platformGateway, hapticsGateway, reviewGateway);
PanelManager.Instance.PresenterFactory = factory;
```

One Install call installs the runtime service and registers both presenters. If the host already owns an ISettingsService, use `SettingsFeatureInstaller.Install(factory, settingsService, OpenRateUs)`. Runtime SettingsInstaller remains available for custom UI; callers using these panels only need SettingsFeatureInstaller.

Open from any caller through `PanelManager.Show<SettingsPanel>(address)` or `Transition<SettingsPanel>(address)`. In the sandbox, GameInstaller owns the combined Shop/Settings factory, attaches it to each scene's manager, and opens Rate Us using `Panel/RateUsPanel.prefab`. FoundationDemoRoot only opens views. Every opening owns one presenter; close/disable/destroy/failure releases it, and cached reopen creates a fresh presenter. Add Dreamy.UI.Presentation to asmdefs using the factory or IPanelPresenter.

Gateway choices are explicit; the installer never creates mock providers:

- SettingsPlatformGateway is a placeholder for future DreamySDK integration. All capabilities return false and operations return Unavailable until SDK logic is added. It performs no store launch, consent, purchase restoration, or review request.
- PlayerPrefsSettingsHapticsGateway persists the device-local preference and calls an injected real haptic provider. Without an installed haptic gateway, the toggle is unavailable. The constructor applies the saved preference to the provider.
- PlayerPrefsSettingsReviewGateway stores device-local positive rating/request and optional reward claim. Account-scoped games should supply their Datasave/backend implementation. Existing Dreamy.Settings.Sample preference keys are preserved for compatibility.

Review-linked reward remains off by default. Its opt-in example requires Android and an idempotent wallet; keep it disabled for Google Play ([policy](https://support.google.com/googleplay/android-developer/answer/9898684?hl=en)). No gateway claims to verify a published store review.

## Validation

From a Unity project with generated script assemblies and the .NET 10 SDK:

```sh
python3 LocalPackages/com.dreamy.feature.settings/Tests~/validate-settings.py
```

The harness checks that views have no presenter/service/navigation construction, validates prefab references, compiles runtime/sample code against the project's assemblies, and verifies factory/host show/hide/reopen/disposal plus haptic/review behavior. Native store launch and host save/reward adapters require separate Unity/device validation.

## Cài package

Dùng Unity 6000.0 trở lên. Sandbox đã tham chiếu package bằng `file:../LocalPackages/com.dreamy.feature.settings`. Project khác dùng Package Manager > + > Install package from disk và chọn package.json, hoặc Git URL của repository nội bộ. Cài cả dependency Dreamy/Git vào manifest của game; version dependency không tự cấu hình registry riêng.

Dependency trực tiếp theo package.json:

- `com.dreamy.core` (1.1.2)
- `com.dreamy.audio` (0.1.0)
- `com.dreamy.feature` (0.1.0)
- `com.dreamy.ui` (0.2.0)
- `com.cysharp.unitask` (2.5.10)


## Optional language selection

The Settings Feature sample includes a TMP language selector. Settings Runtime defines ISettingsLanguageGateway / ISettingsLanguageService / ISettingsLanguageView and does not depend on a Localization package.

Initialize the host localization service first, then inject an ISettingsLanguageGateway through SettingsInstaller.Install(..., languageGateway: gateway), SettingsModel(..., languageGateway: gateway), or SettingsFeatureInstaller.Install(..., language: gateway). With the parameterless installer, register the optional gateway before installation. The host owns/disposes the gateway.

Without a gateway or available options, the actual Settings view hides the dropdown, language label and error, clears stale options and keeps audio/platform settings available. A legacy custom ISettingsService without the optional language capability also renders the selector unavailable. It does not initialize Localization implicitly.

The Settings Feature sample asmdef also references Dreamy.Economy.Runtime for its existing optional review-wallet adapter. Supply that package alongside Core/Audio/UI/Feature/UniTask before importing the sample; these sample references are distinct from Settings Runtime dependencies.

For Dreamy Localization 0.2.0, import that package's **Settings Integration** sample separately. It supplies LocalizationSettingsLanguageGateway and the Dreamy → Localization → Settings Integration authoring tool for the game's Settings prefab/table/font. This optional sample requires both packages; omit it when using Settings alone.

Production SettingsPanel keeps its normal PanelManager navigation. Independent test scenes use the supplied StandaloneSettingsView and direct SettingsPresenter Show/Dispose, without a PanelManager.

Language icons are game-owned presentation data in the Settings Feature sample. Create **Dreamy → Settings → Language Icons**, map canonical locale codes to Sprites, and assign it to the view. The optional Localization Settings Integration tool can wire caption/item images and spacing. Null catalogs or unmapped locales retain text options; no-gateway visibility handling remains unchanged.

## Editable samples and package updates

Keep Runtime code, Editor code, asmdefs and package.json under package ownership for long-term updates. Fix those through reviewed package changes rather than editing the UPM cache.

Import **Settings Feature** into Assets, then move the whole folder with its .meta into a game-owned UI location. That copy is intended for production: customize prefab layout, font, toggles, buttons, platform adapters and integration code. An imported old version folder (for example 0.2.0) stays at its existing path; changing the package version does not migrate or overwrite that game-owned copy.

When upgrading, compare new Samples~ sources against the customized copy and merge selected changes. Do not keep duplicate sample scripts/asmdefs or reimport over customized assets. Existing imported views need the new language fields/visibility handling if adopting this capability. Keep serialized field names and GUIDs when porting changes.
