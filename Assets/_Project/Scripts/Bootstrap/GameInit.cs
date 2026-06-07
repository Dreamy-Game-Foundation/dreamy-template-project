using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.Template.Demo;
using UnityEngine;
using Base.LoadScene;

namespace Dreamy.Template
{
    public sealed class GameInit : MonoBehaviour
    {
        [SerializeField] private bool runSmokeTest = true;

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
            if (runSmokeTest)
            {
                RunSmokeTest();
            }

        SceneLoader.Instance.LoadScene(Address.MainScene);
          

            TemplateDemoApp.Create();
        }

        private static void RunSmokeTest()
        {
            if (!ServiceLocator.TryGet(out IDatasaveService datasave))
            {
                Debug.LogWarning(
                    "[DreamyTemplate] Datasave service is not registered. Add GameInstaller to the bootstrap scene.");
                return;
            }

            TemplatePlayerSave save = datasave.Load<TemplatePlayerSave>();
            save.LaunchCount++;
            datasave.Save(save);
            Debug.Log($"[DreamyTemplate] Bootstrap OK. LaunchCount={save.LaunchCount}");
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