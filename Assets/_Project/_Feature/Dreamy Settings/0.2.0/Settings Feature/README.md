# Settings Feature

Production integration for the supplied passive SettingsPanel and RateUsPanel views. At the composition root, after audio/save readiness:

```csharp
var factory = new PanelPresenterFactory();
SettingsFeatureInstaller.Install(factory, audioService, OpenRateUs, platformGateway, hapticsGateway, reviewGateway);
PanelManager.Instance.PresenterFactory = factory;
```

One Install call installs the runtime service and registers both presenters. If the host already owns an ISettingsService, use `SettingsFeatureInstaller.Install(factory, settingsService, OpenRateUs)`. Runtime SettingsInstaller remains available for custom UI; callers using these panels only need SettingsFeatureInstaller.

Open from any caller through `PanelManager.Show<SettingsPanel>(address)` or `Transition<SettingsPanel>(address)`. In the sandbox, GameInstaller owns the combined Shop/Settings factory, attaches it to each scene's manager, and opens Rate Us using `Panel/RateUsPanel.prefab`. FoundationDemoRoot only opens views. Every opening owns one presenter; close/disable/destroy/failure releases it, and cached reopen creates a fresh presenter. Add Dreamy.UI.Presentation to asmdefs using the factory or IPanelPresenter.

Gateway choices are explicit; the installer never creates mock providers:

- SettingsPlatformGateway opens a real HTTP(S) store listing with Application.OpenURL. Pass the store URL (required for iOS; Android can derive it from Application.identifier). Optional consent/restore callbacks call the host SDK. Missing callbacks disable those actions. Launch success is a request to open the URL, not confirmation of a published review.
- PlayerPrefsSettingsHapticsGateway persists the device-local preference and calls an injected real haptic provider. Without an installed haptic gateway, the toggle is unavailable. The constructor applies the saved preference to the provider.
- GameInstaller registers this gateway with the project-owned `Dreamy.Template.Feedback.MobileHaptics` provider in `Assets/_Project/Scripts/Feedback` when no host gateway is installed. Call `MobileHaptics.Vibrate()` on the Unity main thread for gameplay/UI vibration; it respects the Settings preference, simulates vibration with a `[MobileHaptics] Simulated vibration.` Console log in the Editor, and skips desktop players and unfocused device playback. It uses Unity's `Handheld.Vibrate()` on Android/iOS, with OS-controlled duration. Platform support does not guarantee a vibration motor. Changing the toggle updates the preference without triggering vibration.
- PlayerPrefsSettingsReviewGateway stores device-local positive rating/request and optional reward claim. Account-scoped games should supply their Datasave/backend implementation. Existing Dreamy.Settings.Sample preference keys are preserved for compatibility.

Review-linked reward remains off by default. Its opt-in example requires Android and an idempotent wallet; keep it disabled for Google Play ([policy](https://support.google.com/googleplay/android-developer/answer/9898684?hl=en)). No gateway claims to verify a published store review.
