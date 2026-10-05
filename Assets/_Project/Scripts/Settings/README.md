# Settings / Rate Us platform adapter

Host adapter: `SettingsPlatformGateway.cs`, implementing
`Dreamy.Settings.ISettingsPlatformGateway`. It is registered by GameInstaller
before installing ISettingsService. An already registered gateway is preserved.
No MonoBehaviour or prefab component is required.

Music and SFX use IAudioService; they do not need the platform adapter.
SettingsPresenter and RateUsPresenter share the same ISettingsService, which
forwards platform operations to this gateway.

| UI action | Capability | Adapter method to connect to DreamySDK |
| --- | --- | --- |
| GDPR / consent | CanShowGdprConsent | ShowGdprConsentAsync |
| Restore purchases | CanRestorePurchases | RestorePurchasesAsync |
| Open store | CanOpenStore | OpenStoreAsync |
| Rate Us native review | CanRequestReview | RequestReviewAsync |

## Current behavior

DreamySDK is not imported/configured in this checkout. All capabilities are
false and direct calls return SettingsOperationResult.Unavailable with an
operation-specific explanation. Direct calls with a cancelled token throw
OperationCanceledException. The package SettingsModel checks capabilities first,
so unsupported operations normally return Unavailable without calling the adapter.

GDPR/Restore/Open Store remain hidden according to SettingsViewState. The rating
panel can still collect its UI rating; requesting the native review is unavailable.
No consent, restore, store launch, review or reward is simulated by this adapter.

## Connecting DreamySDK later

1. Import the SDK, inspect its actual APIs and add its Runtime assembly reference
   to Dreamy.Template.Runtime.asmdef if needed. Keep Editor APIs out of Runtime.
2. Implement each method in SettingsPlatformGateway using the verified SDK call.
   Convert the SDK completion result into Succeeded, Failed or Unavailable.
3. Change each Can* property to reflect platform support and SDK readiness.
   Do not enable a capability while its method still returns the placeholder.
4. For callback APIs, await completion with UniTask and propagate cancellation.
   Do not return Succeeded immediately after starting an asynchronous operation.
5. Keep native-review handling separate from any game reward/claim rule. A gateway
   result should describe the SDK operation; it does not verify a submitted rating.
6. Initialize the SDK before installing Settings if it must be ready at startup.
   Otherwise capability properties may query current readiness; re-render the panel
   when readiness changes because the gateway does not push UI state itself.
7. Alternatively create another ISettingsPlatformGateway implementation and register
   it before GameInstaller.InstallFeatures. The installer preserves that instance.
   Replacing the registry entry after Settings is installed does not replace the
   gateway captured by SettingsModel; rebuild the service before creating panels.

The removed SimulatedSettingsPlatformGateway was an imported standalone sample,
not the host production adapter. SDK-specific calls belong here in host code,
not in Library/PackageCache and not in SettingsPanel/RateUsPanel.

Check in Editor/device after SDK integration: available/unavailable capability,
SDK failure, cancellation, restore completion, native review request and reopening
Settings/RateUs. The present scaffold is not an SDK integration or runtime QA pass.