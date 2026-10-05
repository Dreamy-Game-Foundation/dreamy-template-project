using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.Audio;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.Economy;
using Dreamy.Feature.Shop.Integration;
using Dreamy.Settings;
using Dreamy.Shop;
using Dreamy.Template.Pooling;
using UnityEngine;

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
        private IDataConfigService dataConfig;
        private IShopPurchaseGateway purchaseGateway;

        public static BootstrapState State { get; private set; }
        public static Exception InitializationException { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            State = BootstrapState.Initializing;
            InitializationException = null;
            try
            {
                var cancellationToken = this.GetCancellationTokenOnDestroy();

                // 1. Foundation: persistence, pooling and audio.
                InstallPersistence();
                InstallInfrastructure();

                // 2. Economy: one wallet shared by Shop and resource holders.
                InstallEconomy();

                // 3. Configuration: register every document before loading.
                await InstallConfigurationAsync(cancellationToken);

                // 4. Features: all required services and config are now ready.
                InstallFeatures();

                State = BootstrapState.Ready;
            }
            catch (Exception exception)
            {
                InitializationException = exception;
                State = BootstrapState.Failed;
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
            ServiceLocator.Register<IDatasaveService>(datasave);
        }

        private void InstallInfrastructure()
        {
            poolService = new LeanPoolService();
            ServiceLocator.Register<IPoolService>(poolService);
            ServiceLocator.Register<IAudioService>(DreamyAudio.Service);
        }

        private void InstallEconomy()
        {
            var resourceWallet = new DatasaveResourceWallet(datasave);
            wallet = resourceWallet;
            ServiceLocator.Register<IResourceWallet>(resourceWallet);
            ServiceLocator.Register<IResourceBalanceProvider>(resourceWallet);
        }

        private async UniTask InstallConfigurationAsync(CancellationToken cancellationToken)
        {
            dataConfig = new DataConfigService(new ResourcesJsonConfigSource());
            dataConfig.Register<TemplateConfig>("templateConfig");
            dataConfig.Register<TestConfigTable>("testConfigs");
            ShopInstaller.RegisterConfig(dataConfig);

            await dataConfig.InitializeAsync(cancellationToken);
            ServiceLocator.Register<IDataConfigService>(dataConfig);
        }

        private void InstallFeatures()
        {
            if (!ServiceLocator.TryGet<ISettingsPlatformGateway>(out var settingsGateway))
            {
                settingsGateway = new SettingsPlatformGateway();
                ServiceLocator.Register<ISettingsPlatformGateway>(settingsGateway);
            }
            SettingsInstaller.Install(ServiceLocator.Get<IAudioService>(), settingsGateway);

#if UNITY_EDITOR
            // Local IAP simulation belongs to the Editor demo only.
            purchaseGateway = new SimulatedShopPurchaseGateway();
            ServiceLocator.Register<IShopPurchaseGateway>(purchaseGateway);
#else
            ServiceLocator.TryGet(out purchaseGateway);
#endif
            ShopInstaller.Install(dataConfig.GetTable<ShopCatalogConfig>(), wallet, purchaseGateway);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) datasave?.SaveAll();
        }

        private void OnApplicationQuit() => datasave?.SaveAll();

        private void OnDestroy()
        {
            if (poolService is IDisposable disposable)
            {
                disposable.Dispose();
            }

            ServiceLocator.Unregister<IPoolService>();
        }
    }
}
