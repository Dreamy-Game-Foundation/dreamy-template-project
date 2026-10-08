using System;
using Cysharp.Threading.Tasks;
using Dreamy.Audio;
using UnityEngine;

namespace Dreamy.Template
{
    public sealed class GameInit : MonoBehaviour
    {
        private void Start() => InitializeAsync().Forget();

        private async UniTaskVoid InitializeAsync()
        {
            try
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

                // The persistent loader owns this operation: activation destroys GameInit's scene.
                await SceneLoader.Instance.LoadScene(Address.MainScene);
                DreamyAudio.PlayMusic(new AudioKey("core", "music.main"));
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
