using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using System;
using UnityEngine;

namespace Dreamy.Template
{
    [DefaultExecutionOrder(-10000)]
    public sealed class GameInstaller : MonoBehaviour
    {
        [SerializeField] private bool prettySaveInEditor = true;

        private IDatasaveService datasaveService;

        public static BootstrapState State { get; private set; }

        public static Exception InitializationException { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            State = BootstrapState.Initializing;
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
            try
            {
                RegisterDatasave();
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

        private async UniTask RegisterDataConfigAsync()
        {
            if (!ServiceLocator.IsRegistered<IDataConfigService>())
            {
                IDataConfigSource source =
                    new ResourcesJsonConfigSource();
                DataConfigService dataConfigService = new(source);

                int configCount = dataConfigService.RegisterAllConfigs();
                await dataConfigService.InitializeAsync(
                    this.GetCancellationTokenOnDestroy());

                ServiceLocator.Register<IDataConfigService>(dataConfigService);
                Debug.Log(
                    $"[DreamyTemplate] Loaded {configCount} data config(s).");
            }
        }
    }
}
