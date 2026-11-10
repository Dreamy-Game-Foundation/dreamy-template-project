# Template Debugger

GameInstaller initializes Debugger in Manual mode after services/presenters are ready. Configuration: Assets/_Project/Data/TemplateDebuggerConfiguration.asset; project selection: ProjectSettings/DreamyDebuggerSettings.asset. Development mode, five top-left taps, PIN 0000, Compact HUD. Off disables startup. Home-only scene without GameInstaller does not initialize debugger.

Cheats: Coin/Gem balances, amount and confirmed grants; grant-only Remove Ads, Lifetime VIP, Starter Fund and Season 1 Premium Pass; time scale 0.1–3, FPS 15–120, bootstrap status and confirmed Save All. Grants persist through the real wallet; ownership grants do not stack. These actions change the current test save. No simulated billing transaction is created by ownership grants.

Registration is Editor/Development Build only; wallet change notifications refresh controls. Installer teardown releases lease/events before wallet cleanup and restores time scale/FPS. link.xml preserves reflected members for IL2CPP; device/stripping validation remains required.
