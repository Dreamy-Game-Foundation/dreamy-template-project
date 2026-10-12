# Bootstrap installation

`GameInstaller` is the persistent application composition root. Its partial files form one component with one lifetime:

- `GameInstaller.cs`: bootstrap order, readiness/failure, cancellation, application save hooks.
- `GameInstaller.Foundation.cs`: persistence, pooling, audio, economy, configuration.
- `GameInstaller.Features.cs`: Settings, Shop, and project entitlement callbacks.
- `GameInstaller.Presentation.cs`: presenter factory, scene attachment, Rate Us navigation.
- `GameInstaller.Services.cs`: registration ownership and reverse-order cleanup.

Place one `GameInstaller` in the bootstrap scene. There is no singleton or cross-object duplicate guard: scene authoring owns this constraint because services use global `ServiceLocator` registrations. `GameInit` and `HomeBootstrap` read the static readiness state. The installer stays alive across scene changes so services and scene callbacks share the application lifetime; do not add another installer to later scenes or reload the bootstrap scene while the first installer is alive.

To add a feature, register/load its configuration in `InstallConfigurationAsync`, then add a dedicated install method to `InstallFeatures`. Use `RegisterService` for direct registrations and `TrackFeatureInstallation` for package installers that register their own service. Set `ownsInstance` only for instances created by the project. Cleanup restores prior registrations, preserves replacements installed by other owners, and disposes owned instances in reverse order.
