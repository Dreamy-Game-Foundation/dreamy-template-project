using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using UnityEngine;
using Base.LoadScene;

namespace Dreamy.Template
{
    public sealed class GameInit : MonoBehaviour
    {
        private async void Start()
        {
            await UniTask.WaitUntil(
                () => GameInstaller.State is
                    BootstrapState.Ready or BootstrapState.Failed,
                cancellationToken: this.GetCancellationTokenOnDestroy());

            if (GameInstaller.State == BootstrapState.Failed)
            {
                Debug.LogError(
                    $"[DreamyTemplate] Bootstrap failed: " +
                    $"{GameInstaller.InitializationException}");
                return;
            }

            SetInitSetting();
            await SceneLoader.Instance.LoadScene(Address.MainScene);
        }

        private static void SetInitSetting()
        {
            IDataConfigService dataConfig =
                ServiceLocator.Get<IDataConfigService>();
            GameSettingsConfig settings =
                dataConfig.GetTable<GameSettingsConfig>();
            Application.targetFrameRate = settings.TargetFrameRate;
        }
    }
}
