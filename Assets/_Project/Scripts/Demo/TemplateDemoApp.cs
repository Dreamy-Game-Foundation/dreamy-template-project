using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dreamy.Template.Demo
{
    public sealed class TemplateDemoApp : MonoBehaviour
    {
        private const string RootName = "[Template Demo]";

        public static void Create()
        {
            if (FindFirstObjectByType<TemplateDemoApp>())
            {
                return;
            }

            GameObject root = new(RootName);
            DontDestroyOnLoad(root);
            root.AddComponent<TemplateDemoApp>().Build();
        }

        private void Build()
        {
            EnsureEventSystem();

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            HomePanel homePanel =
                DemoUiFactory.CreatePanel<HomePanel>(
                    transform,
                    "Home Panel",
                    new Color(0.05f, 0.08f, 0.12f, 1f));
            TapRushPanel gamePanel =
                DemoUiFactory.CreatePanel<TapRushPanel>(
                    transform,
                    "Tap Rush Panel",
                    new Color(0.03f, 0.04f, 0.07f, 1f));

            homePanel.Initialize(gamePanel);
            gamePanel.Initialize(homePanel);
            gamePanel.gameObject.SetActive(false);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current)
            {
                return;
            }

            GameObject eventSystem = new("EventSystem");
            DontDestroyOnLoad(eventSystem);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
}
