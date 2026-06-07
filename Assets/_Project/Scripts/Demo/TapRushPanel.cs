using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.Template.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Template.Demo
{
    public sealed class TapRushPanel : MonoBehaviour
    {
        private const float TargetPadding = 90f;

        private HomePanel homePanel;
        private IPoolService poolService;
        private IDatasaveService datasaveService;
        private RectTransform playArea;
        private Text scoreText;
        private Text timerText;
        private Text resultText;
        private Button backButton;
        private GameObject targetPrefab;
        private TapTarget activeTarget;
        private float remainingTime;
        private int score;
        private bool isPlaying;

        public void Initialize(HomePanel targetHomePanel)
        {
            homePanel = targetHomePanel;
            poolService = ServiceLocator.Get<IPoolService>();
            datasaveService = ServiceLocator.Get<IDatasaveService>();
            BuildView();
            CreatePool();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            StartRound();
        }

        private void Update()
        {
            if (!isPlaying)
            {
                return;
            }

            remainingTime -= Time.unscaledDeltaTime;
            timerText.text = $"Time: {Mathf.CeilToInt(remainingTime)}";
            if (remainingTime <= 0f)
            {
                FinishRound();
            }
        }

        private void OnDestroy()
        {
            DespawnTarget();
            if (targetPrefab)
            {
                Destroy(targetPrefab);
            }
        }

        private void BuildView()
        {
            scoreText = DemoUiFactory.CreateText(
                transform,
                "Score",
                "Score: 0",
                42,
                TextAnchor.MiddleLeft);
            DemoUiFactory.SetRect(
                scoreText.rectTransform,
                new Vector2(0.08f, 0.88f),
                new Vector2(0.5f, 0.96f),
                Vector2.zero,
                Vector2.zero);

            timerText = DemoUiFactory.CreateText(
                transform,
                "Timer",
                "Time: 0",
                42,
                TextAnchor.MiddleRight);
            DemoUiFactory.SetRect(
                timerText.rectTransform,
                new Vector2(0.5f, 0.88f),
                new Vector2(0.92f, 0.96f),
                Vector2.zero,
                Vector2.zero);

            GameObject areaObject = new(
                "Play Area",
                typeof(RectTransform),
                typeof(Image));
            areaObject.transform.SetParent(transform, false);
            playArea = areaObject.GetComponent<RectTransform>();
            DemoUiFactory.SetRect(
                playArea,
                new Vector2(0.06f, 0.18f),
                new Vector2(0.94f, 0.85f),
                Vector2.zero,
                Vector2.zero);
            areaObject.GetComponent<Image>().color =
                new Color(0.1f, 0.14f, 0.2f, 1f);

            resultText = DemoUiFactory.CreateText(
                transform,
                "Result",
                string.Empty,
                42,
                TextAnchor.MiddleCenter);
            DemoUiFactory.SetRect(
                resultText.rectTransform,
                new Vector2(0.12f, 0.08f),
                new Vector2(0.7f, 0.16f),
                Vector2.zero,
                Vector2.zero);

            backButton = DemoUiFactory.CreateButton(
                transform,
                "Back",
                "HOME",
                BackHome);
            DemoUiFactory.SetRect(
                backButton.GetComponent<RectTransform>(),
                new Vector2(0.72f, 0.07f),
                new Vector2(0.94f, 0.16f),
                Vector2.zero,
                Vector2.zero);
        }

        private void CreatePool()
        {
            IDataConfigService configs =
                ServiceLocator.Get<IDataConfigService>();
            GameSettingsConfig settings =
                configs.GetTable<GameSettingsConfig>();

            targetPrefab = DemoUiFactory.CreateTargetPrefab();
            targetPrefab.transform.SetParent(transform, false);
            poolService.Preload(
                targetPrefab,
                settings.PoolPreloadCount,
                settings.PoolPreloadCount + 2,
                true);
        }

        private void StartRound()
        {
            GameSettingsConfig settings =
                ServiceLocator.Get<IDataConfigService>()
                    .GetTable<GameSettingsConfig>();
            score = 0;
            remainingTime = settings.DemoDurationSeconds;
            isPlaying = true;
            resultText.text = "Tap every target!";
            backButton.interactable = false;
            UpdateScore();
            Canvas.ForceUpdateCanvases();
            SpawnTarget();
        }

        private void HandleTargetTapped(TapTarget target)
        {
            if (!isPlaying || target != activeTarget)
            {
                return;
            }

            score++;
            UpdateScore();
            MoveTarget(target.GetComponent<RectTransform>());
        }

        private void SpawnTarget()
        {
            activeTarget = poolService.Spawn(
                targetPrefab.GetComponent<TapTarget>(),
                Vector3.zero,
                Quaternion.identity,
                playArea);
            activeTarget.Configure(HandleTargetTapped);
            MoveTarget(activeTarget.GetComponent<RectTransform>());
        }

        private void MoveTarget(RectTransform target)
        {
            Rect rect = playArea.rect;
            float x = Random.Range(
                rect.xMin + TargetPadding,
                rect.xMax - TargetPadding);
            float y = Random.Range(
                rect.yMin + TargetPadding,
                rect.yMax - TargetPadding);
            target.anchoredPosition = new Vector2(x, y);
        }

        private void FinishRound()
        {
            isPlaying = false;
            remainingTime = 0f;
            timerText.text = "Time: 0";
            DespawnTarget();

            TemplatePlayerSave save =
                datasaveService.Load<TemplatePlayerSave>();
            if (score > save.TapRushHighScore)
            {
                save.TapRushHighScore = score;
                datasaveService.Save(save);
                resultText.text = $"New best: {score}";
            }
            else
            {
                resultText.text = $"Score: {score}";
            }

            backButton.interactable = true;
        }

        private void BackHome()
        {
            if (isPlaying)
            {
                return;
            }

            gameObject.SetActive(false);
            homePanel.Show();
        }

        private void DespawnTarget()
        {
            if (!activeTarget)
            {
                return;
            }

            poolService.Despawn(activeTarget.gameObject);
            activeTarget = null;
        }

        private void UpdateScore()
        {
            scoreText.text = $"Score: {score}";
        }
    }
}
