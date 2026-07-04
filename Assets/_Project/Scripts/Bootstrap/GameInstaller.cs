using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Newtonsoft.Json;
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
                var cancellationToken = this.GetCancellationTokenOnDestroy();

                // 1. Install Datasave Service
                InstallDatasaveService();

                // 2. Install DataConfig Service
                await InstallDataConfigServiceAsync(cancellationToken);

                State = BootstrapState.Ready;
            }
            catch (Exception exception)
            {
                InitializationException = exception;
                State = BootstrapState.Failed;
                Debug.LogException(exception, this);
            }
        }

        private void InstallDatasaveService()
        {
            datasave = new DatasaveService(new DatasaveOptions
            {
                PrettyPrint = Application.isEditor && prettySaveInEditor,
                Codec = new PlainTextSaveCodec()
            });
            ServiceLocator.Register<IDatasaveService>(datasave);
        }

        private async UniTask InstallDataConfigServiceAsync(CancellationToken cancellationToken)
        {
            IRemoteConfigProvider remoteConfigProvider = new RemoteDataConfigProvider();
            IDataConfigSource configSource = new CompositeConfigSource(new IDataConfigSource[]
            {
                new RemoteConfigSource(remoteConfigProvider),
                new ResourcesJsonConfigSource()
            });

            var dataConfig = new DataConfigService(configSource);
            dataConfig.Register<TemplateConfig>("templateConfig");

            // Example of registering a Table Config
            dataConfig.Register<DataConfigTable<TestConfig>>("testConfigs");

            await dataConfig.InitializeAsync(cancellationToken);
            ServiceLocator.Register<IDataConfigService>(dataConfig);

            // Example of retrieving and printing rows from the Table Config
            var testTable = dataConfig.GetTable<DataConfigTable<TestConfig>>();
            foreach (var row in testTable.GetAll())
            {
                Debug.Log($"[TestConfig] Id: {row.Id}, Name: {row.Name}, Value: {row.Value}");
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) datasave?.SaveAll();
        }

        private void OnApplicationQuit() => datasave?.SaveAll();
    }
}