using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Audio;
using Dreamy.Core.Pooling;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.Economy;
using Dreamy.Feature.Shop.Integration;
using Dreamy.Template.Pooling;
using UnityEngine;

namespace Dreamy.Template
{
    public sealed partial class GameInstaller
    {
        private IDatasaveService datasave;
        private IDataConfigService dataConfig;
        private IResourceWallet wallet;
        private IResourceBalanceSource walletBalanceSource;

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
            RegisterService<IPoolService>(new LeanPoolService(), ownsInstance: true);
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
    }
}
