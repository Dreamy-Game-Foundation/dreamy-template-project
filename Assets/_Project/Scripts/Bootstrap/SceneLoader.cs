using System;
using Cysharp.Threading.Tasks;
using Dreamy.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base.LoadScene
{
    public sealed class SceneLoader : LiveSingleton<SceneLoader>
    {
        [SerializeField] private UILoadingScreen loadingScreen;
        [SerializeField] private float minimumLoadingDuration = 1.5f;

        public event Action OnScenePreloading;
        public event Action<float> OnSceneLoading;
        public event Action OnSceneLoaded;
        public event Action OnLastSceneHidden;
        public event Action OnScenePresented;

        private bool _isLoading;
        

        public async UniTask LoadScene(string sceneName)
        {
            if (_isLoading)
            {
                Debug.LogWarning($"SceneLoader is already loading a scene.");
                return;
            }

            _isLoading = true;

            try
            {
                InitializeLoading();

                OnScenePreloading?.Invoke();

                var loadOperation = SceneManager.LoadSceneAsync(
                    sceneName,
                    LoadSceneMode.Single);

                loadOperation.allowSceneActivation = false;

                float elapsedTime = 0f;

                while (loadOperation.progress < 0.9f)
                {
                    elapsedTime += Time.unscaledDeltaTime;

                    float progress =
                        (loadOperation.progress / 0.9f) * 0.8f;

                    ReportProgress(progress);

                    await UniTask.Yield();
                }

                OnSceneLoaded?.Invoke();
                OnLastSceneHidden?.Invoke();

                while (elapsedTime < minimumLoadingDuration)
                {
                    elapsedTime += Time.unscaledDeltaTime;

                    float t = Mathf.Clamp01(
                        elapsedTime / minimumLoadingDuration);

                    ReportProgress(Mathf.Lerp(0.8f, 1f, t));

                    await UniTask.Yield();
                }

                ReportProgress(1f);

                loadOperation.allowSceneActivation = true;

                await UniTask.WaitUntil(
                    () => loadOperation.isDone);

                await UniTask.Delay(
                    300,
                    ignoreTimeScale: true);

                loadingScreen.Hide();

                OnScenePresented?.Invoke();
            }
            finally
            {
                Cleanup();
            }
        }

        private void InitializeLoading()
        {
            loadingScreen.Show();
            loadingScreen.SetProgress(0f);
        }

        private void ReportProgress(float progress)
        {
            OnSceneLoading?.Invoke(progress);
            loadingScreen.SetProgress(progress);
        }

        private void Cleanup()
        {
            _isLoading = false;

            OnScenePreloading = null;
            OnSceneLoading = null;
            OnSceneLoaded = null;
            OnLastSceneHidden = null;
            OnScenePresented = null;
        }
    }
}