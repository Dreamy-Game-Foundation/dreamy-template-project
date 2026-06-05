using Dreamy.Template;
using Dreamy.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Dreamy.Template.Editor
{
    public static class DreamyTemplateSceneBuilder
    {
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string MenuRoot = "Tools/Dreamy/Template/";

        [MenuItem(MenuRoot + "Create Bootstrap Scene")]
        public static void CreateBootstrapScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Bootstrap";

            GameObject bootstrap = new GameObject("Bootstrap");
            bootstrap.AddComponent<GameInstaller>();
            bootstrap.AddComponent<GameInit>();

            GameObject canvasObject = new GameObject("UIRoot");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            canvasObject.AddComponent<PanelManager>();

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
            EditorUtility.DisplayDialog("Dreamy Template", $"Created {BootstrapScenePath}", "OK");
        }
    }
}
