using System;
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

        private IDatasaveService datasave;

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
            try
            {
                // inject service
                datasave = new DatasaveService(new DatasaveOptions
                {
                    PrettyPrint = Application.isEditor && prettySaveInEditor,
                    Codec = new PlainTextSaveCodec()
                });
                ServiceLocator.Register<IDatasaveService>(datasave);

                IRemoteConfigProvider remoteConfigProvider = new RemoteDataConfigProvider();
                IDataConfigSource configSource = new CompositeConfigSource(new IDataConfigSource[]
                {
                    new RemoteConfigSource(remoteConfigProvider),
                    new ResourcesJsonConfigSource()
                });
                DataConfigService dataConfig = new(configSource);
                dataConfig.Register<TemplateConfig>("templateConfig");
                await dataConfig.InitializeAsync(this.GetCancellationTokenOnDestroy());
                ServiceLocator.Register<IDataConfigService>(dataConfig);

                State = BootstrapState.Ready;
            }
            catch (Exception exception)
            {
                InitializationException = exception;
                State = BootstrapState.Failed;
                Debug.LogException(exception, this);
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) datasave?.SaveAll();
        }

        private void OnApplicationQuit() => datasave?.SaveAll();
    }
}
