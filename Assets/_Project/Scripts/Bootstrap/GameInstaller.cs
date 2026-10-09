using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.Audio;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.Economy;
using Dreamy.Feature.Shop.Integration;
using Dreamy.Settings;
using Dreamy.Feature.Settings.Integration;
using Dreamy.Template.Home;
using Dreamy.Template.Demo;
using Dreamy.UI;
using Dreamy.Shop;
using Dreamy.Template.Pooling;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamy.Template
{
    [DefaultExecutionOrder(-10000)]
    public sealed class GameInstaller : MonoBehaviour
    {
        [Header("Persistence")]
        [SerializeField] private bool prettySaveInEditor = true;

        private IDatasaveService datasave;
        private IPoolService poolService;
        private IResourceWallet wallet;
        private IResourceBalanceSource walletBalanceSource;
        private IDataConfigService dataConfig;
        private IShopPurchaseGateway purchaseGateway;
        private PanelPresenterFactory presenterFactory;
        private readonly Dictionary<PanelManager, PanelPresenterFactory> previousFactories = new();
        private bool isOpeningRateUs;
        private readonly List<EntitlementEffectBinding> entitlementBindings = new();
        private static GameInstaller instance;
        private readonly List<Action> teardownActions = new();
        private CancellationTokenSource initializationCancellation;
        private bool ownsInstallation;

        public static BootstrapState State { get; private set; }
        public static Exception InitializationException { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            State = BootstrapState.None;
            InitializationException = null;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            ownsInstallation = true;
            initializationCancellation = new CancellationTokenSource();
            DontDestroyOnLoad(gameObject);
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            State = BootstrapState.Initializing;
            InitializationException = null;
            try
            {
                var cancellationToken = initializationCancellation.Token;

                // 1. Foundation: persistence, pooling and audio.
                InstallPersistence();
                InstallInfrastructure();

                // 2. Economy: one wallet shared by Shop and resource holders.
                InstallEconomy();

                // 3. Configuration: register every document before loading.
                await InstallConfigurationAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                // 4. Features: all required services and config are now ready.
                InstallFeatures();
                InstallPresentation();
                cancellationToken.ThrowIfCancellationRequested();

                State = BootstrapState.Ready;
            }
            catch (OperationCanceledException) when (initializationCancellation == null ||
                initializationCancellation.IsCancellationRequested)
            {
                TearDownServices();
            }
            catch (Exception exception)
            {
                TearDownServices();
                if (ReferenceEquals(instance, this))
                {
                    InitializationException = exception;
                    State = BootstrapState.Failed;
                }
                Debug.LogException(exception, this);
            }
        }

        private void InstallPersistence()
        {
            ISaveCodec codec;
#if UNITY_EDITOR
            codec = new PlainTextSaveCodec();
#else
            codec = new XorSaveCodec("Dreamy123@");
#endif

            datasave = new DatasaveService(new DatasaveOptions
            {
                PrettyPrint = Application.isEditor && prettySaveInEditor,
                Codec = codec
            });
            RegisterService<IDatasaveService>(datasave, ownsInstance: true);
        }

        private void InstallInfrastructure()
        {
            poolService = new LeanPoolService();
            RegisterService<IPoolService>(poolService, ownsInstance: true);
            // The Audio package owns the instance; only undo our registration.
            RegisterService<IAudioService>(DreamyAudio.Service);
        }

        private void InstallEconomy()
        {
            var resourceWallet = new DatasaveResourceWallet(datasave);
            wallet = resourceWallet;
            walletBalanceSource = resourceWallet;
            RegisterService<IResourceWallet>(resourceWallet, ownsInstance: true);
            RegisterService<IResourceBalanceProvider>(resourceWallet);
        }

        private async UniTask InstallConfigurationAsync(CancellationToken cancellationToken)
        {
            dataConfig = new DataConfigService(new ResourcesJsonConfigSource());
            dataConfig.Register<TemplateConfig>("templateConfig");
            dataConfig.Register<TestConfigTable>("testConfigs");
            ShopFeatureInstaller.RegisterConfig(dataConfig);

            RegisterService<IDataConfigService>(dataConfig, ownsInstance: true);
            await dataConfig.InitializeAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }

        private void InstallFeatures()
        {
            presenterFactory = new PanelPresenterFactory();
            if (!ServiceLocator.TryGet<ISettingsPlatformGateway>(out var settingsGateway))
            {
                settingsGateway = new SettingsPlatformGateway();
                RegisterService<ISettingsPlatformGateway>(settingsGateway, ownsInstance: true);
            }
            ServiceLocator.TryGet<ISettingsHapticsGateway>(out var hapticsGateway);
            ServiceLocator.TryGet<ISettingsReviewGateway>(out var reviewGateway);
            TrackFeatureInstallation<ISettingsService>(() =>
                SettingsFeatureInstaller.Install(presenterFactory, ServiceLocator.Get<IAudioService>(),
                    () => OpenRateUsAsync().Forget(), settingsGateway, hapticsGateway, reviewGateway));

#if UNITY_EDITOR
            // Local IAP simulation belongs to the Editor demo only.
            if (!ServiceLocator.TryGet(out purchaseGateway))
            {
                purchaseGateway = new SimulatedShopPurchaseGateway();
                RegisterService<IShopPurchaseGateway>(purchaseGateway, ownsInstance: true);
            }
#else
            ServiceLocator.TryGet(out purchaseGateway);
#endif
            TrackFeatureInstallation<IShopService>(() =>
                ShopFeatureInstaller.Install(presenterFactory, dataConfig.GetTable<ShopCatalogConfig>(),
                    wallet, purchaseGateway, (IResourceBalanceProvider)wallet));
            InstallEntitlementEffects();
        }

        private void InstallPresentation()
        {
            presenterFactory.Register<HomePanel>(view => new HomePresenter(view, new HomeNavigator()));
            presenterFactory.Register<FoundationDemoPanel>(view =>
                new FoundationDemoRoot(view, datasave, dataConfig));
            RegisterService<PanelPresenterFactory>(presenterFactory);
            SceneManager.sceneLoaded += AttachScenePresentation;
            SceneManager.sceneUnloaded += PruneScenePresentation;
            foreach (var manager in FindObjectsByType<PanelManager>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)) AttachPresentation(manager);
        }

        private void AttachScenePresentation(Scene scene, LoadSceneMode mode)
        {
            if (State != BootstrapState.Ready) return;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var manager in root.GetComponentsInChildren<PanelManager>(true))
                    AttachPresentation(manager);
        }

        private void PruneScenePresentation(Scene scene)
        {
            foreach (var manager in new List<PanelManager>(previousFactories.Keys))
                if (manager == null || manager.gameObject.scene == scene) previousFactories.Remove(manager);
        }

        private void AttachPresentation(PanelManager manager)
        {
            if (manager == null || previousFactories.ContainsKey(manager)) return;
            previousFactories.Add(manager, manager.PresenterFactory);
            manager.PresenterFactory = presenterFactory;
        }

        private async UniTask OpenRateUsAsync()
        {
            if (isOpeningRateUs || State != BootstrapState.Ready || !PanelManager.HasInstance) return;
            isOpeningRateUs = true;
            try { await PanelManager.Instance.Transition<RateUsPanel>(Address.RateUsPanel); }
            catch (OperationCanceledException) { }
            catch (Exception exception) { Debug.LogException(exception, this); }
            finally { isOpeningRateUs = false; }
        }

        private void ReleasePresentation()
        {
            SceneManager.sceneLoaded -= AttachScenePresentation;
            SceneManager.sceneUnloaded -= PruneScenePresentation;
            foreach (var entry in previousFactories)
                if (entry.Key != null && ReferenceEquals(entry.Key.PresenterFactory, presenterFactory))
                    entry.Key.PresenterFactory = entry.Value;
            previousFactories.Clear();
            presenterFactory = null;
        }

        private void RegisterService<T>(T service, bool ownsInstance = false) where T : class
        {
            ServiceLocator.TryGet<T>(out var previous);
            TrackRegistration(service, previous, ownsInstance);
            ServiceLocator.Register(service);
        }

        private void TrackFeatureInstallation<T>(Func<T> install) where T : class
        {
            ServiceLocator.TryGet<T>(out var previous);
            T service = install();
            TrackRegistration(service, previous, ownsInstance: true);
        }

        private void TrackRegistration<T>(T service, T previous, bool ownsInstance) where T : class
        {
            teardownActions.Add(() =>
            {
                // Never unregister a replacement installed by another owner.
                if (ServiceLocator.TryGet<T>(out var current) && ReferenceEquals(current, service))
                {
                    ServiceLocator.Unregister<T>();
                    if (previous != null) ServiceLocator.Register(previous);
                }

                if (ownsInstance && service is IDisposable disposable) disposable.Dispose();
            });
        }

        private void TearDownServices()
        {
            ReleasePresentation();
            DisposeEntitlementEffects();
            while (teardownActions.Count > 0)
            {
                int index = teardownActions.Count - 1;
                Action teardown = teardownActions[index];
                teardownActions.RemoveAt(index);
                try { teardown(); }
                catch (Exception exception) { Debug.LogException(exception, this); }
            }

            datasave = null;
            poolService = null;
            wallet = null;
            walletBalanceSource = null;
            dataConfig = null;
            purchaseGateway = null;
        }

        private void InstallEntitlementEffects()
        {
            BindEntitlement(EntitlementIds.RemoveAds, ApplyNoAdsDemo);
            BindEntitlement(EntitlementIds.LifetimeVip, ApplyLifetimeVipDemo);
            BindEntitlement(EntitlementIds.StarterFund, UnlockStarterFundDemo);
            BindEntitlement(EntitlementIds.Season1PremiumPass, UnlockSeason1PremiumPassDemo);
        }

        private void BindEntitlement(ResourceId id, Action effect)
        {
            entitlementBindings.Add(new EntitlementEffectBinding(
                walletBalanceSource, id, effect, exception => Debug.LogException(exception, this)));
        }

        // Demo effects: replace these callbacks with the game's ready SDK/services.
        // Wallet entitlement balances remain the ownership source of truth.
        // Save models below are examples to define in the game (derive from SaveData).
        private void ApplyNoAdsDemo()
        {
            // Production example, once DreamySDK is installed and ready:
            // DreamySDK.SetNoAds();
            LogEntitlementEffect("No Ads enabled");
        }

        private void ApplyLifetimeVipDemo()
        {
            // Optional saved projection for systems that need an IsVip flag:
            // var vipData = datasave.Load<VipSave>("vip");
            // vipData.IsVip = true;
            // datasave.Save(vipData, "vip");
            // Apply VIP benefits through the game's VIP service here.
            LogEntitlementEffect("VIP enabled");
        }

        private void UnlockStarterFundDemo()
        {
            // var fundData = datasave.Load<StarterFundSave>("starter-fund");
            // fundData.IsUnlocked = true;
            // datasave.Save(fundData, "starter-fund");
            // Keep milestone progress/claims separate; do not grant them on every startup.
            LogEntitlementEffect("Starter Fund unlocked");
        }

        private void UnlockSeason1PremiumPassDemo()
        {
            // var passData = datasave.Load<BattlePassSave>("battle-pass-season-1");
            // passData.IsPremium = true;
            // datasave.Save(passData, "battle-pass-season-1");
            // Apply to season 1 only; the pass service owns active-season checks and claims.
            LogEntitlementEffect("Season 1 Premium Pass unlocked");
        }

        private void LogEntitlementEffect(string effect)
        {
            Debug.Log($"[Shop] {effect}", this);
        }

        private void DisposeEntitlementEffects()
        {
            foreach (var binding in entitlementBindings) binding.Dispose();
            entitlementBindings.Clear();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) datasave?.SaveAll();
        }

        private void OnApplicationQuit() => datasave?.SaveAll();

        private void OnDestroy()
        {
            if (!ownsInstallation) return;
            if (ReferenceEquals(instance, this))
            {
                instance = null;
                State = BootstrapState.None;
                InitializationException = null;
            }

            initializationCancellation?.Cancel();
            TearDownServices();
            initializationCancellation?.Dispose();
            initializationCancellation = null;
            ownsInstallation = false;
        }
    }
}
