using Dreamy.Core;
using Dreamy.Datasave;
using UnityEngine;

namespace Dreamy.Template
{
    public sealed class GameInstaller : MonoBehaviour
    {
        [SerializeField] private bool prettySaveInEditor = true;

        private IDatasaveService datasaveService;

        private void Awake()
        {
            RegisterServices();
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

        private void RegisterServices()
        {
            if (!ServiceLocator.IsRegistered<IDatasaveService>())
            {
                var options = new DatasaveOptions
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
    }
}
