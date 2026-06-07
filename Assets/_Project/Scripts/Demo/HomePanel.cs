using Dreamy.Core;
using Dreamy.Datasave;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Template.Demo
{
    public sealed class HomePanel : MonoBehaviour
    {
        private Text highScoreText;
        private TapRushPanel gamePanel;

        public void Initialize(TapRushPanel targetGamePanel)
        {
            gamePanel = targetGamePanel;
            BuildView();
            Refresh();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Refresh();
        }

        private void BuildView()
        {
            Text title = DemoUiFactory.CreateText(
                transform,
                "Title",
                "DREAMY TEMPLATE",
                72,
                TextAnchor.MiddleCenter);
            DemoUiFactory.SetRect(
                title.rectTransform,
                new Vector2(0.1f, 0.68f),
                new Vector2(0.9f, 0.86f),
                Vector2.zero,
                Vector2.zero);

            Text subtitle = DemoUiFactory.CreateText(
                transform,
                "Subtitle",
                "Home panel + LeanPool service demo",
                34,
                TextAnchor.MiddleCenter);
            DemoUiFactory.SetRect(
                subtitle.rectTransform,
                new Vector2(0.12f, 0.56f),
                new Vector2(0.88f, 0.66f),
                Vector2.zero,
                Vector2.zero);

            highScoreText = DemoUiFactory.CreateText(
                transform,
                "High Score",
                string.Empty,
                38,
                TextAnchor.MiddleCenter);
            DemoUiFactory.SetRect(
                highScoreText.rectTransform,
                new Vector2(0.2f, 0.44f),
                new Vector2(0.8f, 0.52f),
                Vector2.zero,
                Vector2.zero);

            Button playButton = DemoUiFactory.CreateButton(
                transform,
                "Play",
                "PLAY TAP RUSH",
                StartGame);
            DemoUiFactory.SetRect(
                playButton.GetComponent<RectTransform>(),
                new Vector2(0.2f, 0.26f),
                new Vector2(0.8f, 0.37f),
                Vector2.zero,
                Vector2.zero);
        }

        private void StartGame()
        {
            gameObject.SetActive(false);
            gamePanel.Show();
        }

        private void Refresh()
        {
            IDatasaveService datasave =
                ServiceLocator.Get<IDatasaveService>();
            TemplatePlayerSave save =
                datasave.Load<TemplatePlayerSave>();
            highScoreText.text =
                $"Best Score: {save.TapRushHighScore}";
        }
    }
}
