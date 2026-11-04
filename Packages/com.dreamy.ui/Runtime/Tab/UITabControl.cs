using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dreamy.UI
{
    public class UITabControl : MonoBehaviour
    {
        [SerializeField] private List<TabGroup> tabs = new List<TabGroup>();
        [SerializeField] private bool autoInit;
        [SerializeField] private bool autoOpen = true;
        [SerializeField] private int autoOpenTab;

        private int currentTab = -1;
        private bool initialized;
        private UniTaskCompletionSource initCompletion;

        private void Awake()
        {
            if (autoInit)
            {
                EnsureInitialized().Forget();
            }
        }

        private void OnEnable()
        {
            if (autoOpen)
            {
                OpenWhenInitialized().Forget();
            }
        }

        public UniTask Init()
        {
            return EnsureInitialized();
        }

        private async UniTask Initialize()
        {
            try
            {
                if (initialized)
                {
                    return;
                }

                for (int i = 0; i < tabs.Count; i++)
                {
                    if (tabs[i] == null)
                    {
                        continue;
                    }

                    tabs[i].Register(this, i);
                    await tabs[i].Init();
                }

                initialized = true;
                initCompletion.TrySetResult();
            }
            catch (System.Exception exception)
            {
                initCompletion.TrySetException(exception);
            }
        }

        public void OpenTab(int index)
        {
            if (!initialized)
            {
                Debug.LogWarning("UITabControl must be initialized before opening a tab.", this);
                return;
            }
            if (tabs.Count == 0)
            {
                return;
            }

            if (index < 0 || index >= tabs.Count)
            {
                Debug.LogWarning($"Invalid tab index: {index}");
                index = 0;
            }

            if (currentTab == index)
            {
                return;
            }

            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i] == null)
                {
                    continue;
                }

                if (i == index)
                {
                    tabs[i].Show();
                }
                else
                {
                    tabs[i].Hide();
                }
            }

            currentTab = index;
        }

        private UniTask EnsureInitialized()
        {
            if (initialized)
            {
                return UniTask.CompletedTask;
            }

            if (initCompletion != null)
            {
                return initCompletion.Task;
            }

            initCompletion = new UniTaskCompletionSource();
            Initialize().Forget();
            return initCompletion.Task;
        }

        private async UniTaskVoid OpenWhenInitialized()
        {
            await EnsureInitialized();
            if (this != null && isActiveAndEnabled && autoOpen)
            {
                OpenTab(autoOpenTab);
            }
        }
    }
}
