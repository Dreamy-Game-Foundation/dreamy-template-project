using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using UnityEngine;

namespace Dreamy.Template
{
    [DefaultExecutionOrder(-10000)]
    public sealed class GameInstaller : MonoBehaviour
    {
        [SerializeField] private bool prettySaveInEditor = true;

        [Tooltip("Optional component implementing IRemoteConfigProvider.")] [SerializeField]
        private MonoBehaviour remoteConfigProvider = null;

        private IDatasaveService datasaveService;

        private void Awake()
        {
            RegisterServicesAsync().Forget();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                datasaveService?.SaveAll();
            }
        }

        private void OnApplicationQuit()
        {
            datasaveService?.SaveAll();
        }

        private async UniTaskVoid RegisterServicesAsync()
        {
            if (!ServiceLocator.IsRegistered<IDatasaveService>())
            {
                var options = new DatasaveOptions
                {
                    PrettyPrint = Application.isEditor && prettySaveInEditor,
                    Codec = new PlainTextSaveCodec()
                };

                datasaveService = new DatasaveService(options);
                ServiceLocator.Register<IDatasaveService>(datasaveService);
            }
            else
            {
                datasaveService = ServiceLocator.Get<IDatasaveService>();
            }

            if (!ServiceLocator.IsRegistered<IDataConfigService>())
            {
                IRemoteConfigProvider remoteProvider =
                    ResolveRemoteConfigProvider();
                IDataConfigSource source =
                    DataConfigSources.CreateDefault(remoteProvider);
                DataConfigService dataConfigService = new(source);

                int configCount = dataConfigService.RegisterAllConfigs();
                await dataConfigService.InitializeAsync(
                    this.GetCancellationTokenOnDestroy());

                ServiceLocator.Register<IDataConfigService>(dataConfigService);
                Debug.Log(
                    $"[DreamyTemplate] Loaded {configCount} data config(s).");
                var testConfig = dataConfigService.GetTable<GameSettingsConfig>();
                Debug.Log(
                    $"[DreamyTemplate] Loaded {testConfig.GetType()} with {testConfig.StartingCoins} and {testConfig.MusicVolume}");
            }
        }

        private IRemoteConfigProvider ResolveRemoteConfigProvider()
        {
            if (!remoteConfigProvider)
            {
                return null;
            }

            if (remoteConfigProvider is IRemoteConfigProvider provider)
            {
                return provider;
            }

            Debug.LogError(
                $"{remoteConfigProvider.GetType().Name} must implement " +
                $"{nameof(IRemoteConfigProvider)}.");
            return null;
        }
    }
}