using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.Template.Pooling;
using System;
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
        private IPoolService poolService;

        public static BootstrapState State { get; private set; }

        public static Exception InitializationException { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            State = BootstrapState.Initializing;
            RegisterServicesAsync().Forget();
        }

        private void OnDestroy()
        {
            if (poolService is IDisposable disposable)
            {
                disposable.Dispose();
            }
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
            try
            {
                RegisterDatasave();
                RegisterPool();
                await RegisterDataConfigAsync();
                State = BootstrapState.Ready;
            }
            catch (Exception exception)
            {
                InitializationException = exception;
                State = BootstrapState.Failed;
                Debug.LogException(exception);
            }
        }

        private void RegisterDatasave()
        {
            if (!ServiceLocator.IsRegistered<IDatasaveService>())
            {
                DatasaveOptions options = new()
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
        }

        private void RegisterPool()
        {
            if (!ServiceLocator.IsRegistered<IPoolService>())
            {
                poolService = new LeanPoolService();
                ServiceLocator.Register<IPoolService>(poolService);
            }
            else
            {
                poolService = ServiceLocator.Get<IPoolService>();
            }
        }

        private async UniTask RegisterDataConfigAsync()
        {
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
