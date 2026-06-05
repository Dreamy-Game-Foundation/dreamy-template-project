using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.Datasave;
using UnityEngine;

namespace Dreamy.Template
{
    public sealed class GameInit : MonoBehaviour
    {
        [SerializeField] private bool runSmokeTest = true;

        private async void Start()
        {
            await UniTask.Yield();

            if (runSmokeTest)
            {
                RunSmokeTest();
            }
        }

        private static void RunSmokeTest()
        {
            if (!ServiceLocator.TryGet(out IDatasaveService datasave))
            {
                Debug.LogWarning("[DreamyTemplate] Datasave service is not registered. Add GameInstaller to the bootstrap scene.");
                return;
            }

            TemplatePlayerSave save = datasave.Load<TemplatePlayerSave>();
            save.LaunchCount++;
            datasave.Save(save);
            Debug.Log($"[DreamyTemplate] Bootstrap OK. LaunchCount={save.LaunchCount}");
        }
    }
}
