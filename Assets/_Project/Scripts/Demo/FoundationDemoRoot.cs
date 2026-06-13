using Cysharp.Threading.Tasks;
using Dreamy.Core;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.Template.Pooling;
using Dreamy.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.Template.Demo
{
    [DisallowMultipleComponent]
    public sealed class FoundationDemoRoot : MonoBehaviour
    {
        [SerializeField] private Button togglePanelButton;

        private LeanPoolService ownedPool;
        private DemoSession session;
        private TemplatePlayerSave saveData;
        private IDatasaveService datasave;
        private FoundationDemoPanel panel;
        private EventBinding<DemoScoreChangedEvent> scoreBinding;
        private bool isTransitioning;
        private bool isShuttingDown;

        private void Awake()
        {
            if (togglePanelButton == null)
            {
                Debug.LogError("[FoundationDemo] Toggle panel button is not assigned.", this);
                enabled = false;
                return;
            }

            togglePanelButton.onClick.AddListener(TogglePanel);
        }

        private async void Start()
        {
            await UniTask.WaitUntil(
                () => GameInstaller.State is BootstrapState.Ready or BootstrapState.Failed,
                cancellationToken: this.GetCancellationTokenOnDestroy());

            if (GameInstaller.State == BootstrapState.Failed)
            {
                Debug.LogError(
                    $"[FoundationDemo] Bootstrap failed: {GameInstaller.InitializationException}");
                return;
            }

            InitializeDemo();
            await SetPanelVisibleAsync(true);
        }

        private void InitializeDemo()
        {
            if (!ServiceLocator.IsRegistered<IPoolService>())
            {
                ownedPool = new LeanPoolService();
                ServiceLocator.Register<IPoolService>(ownedPool);
            }

            session = new DemoSession(0, 100f);
            datasave = ServiceLocator.Get<IDatasaveService>();
            saveData = datasave.Load<TemplatePlayerSave>();
            saveData.LaunchCount++;
            datasave.Save(saveData);
        }

        private async UniTask CreateAndBindPanelAsync()
        {
            FoundationDemoPanel createdPanel = await PanelManager.Instance.Show<FoundationDemoPanel>(
                Address.FoundationDemoPanel);
            if (isShuttingDown)
            {
                await createdPanel.Hide();
                return;
            }

            panel = createdPanel;
            BindPanel(createdPanel);

            GameSettingsConfig config = ServiceLocator.Get<IDataConfigService>()
                .GetTable<GameSettingsConfig>();
            createdPanel.SetStatus(
                $"PASS | launch={saveData.LaunchCount} | config coins={config.StartingCoins}");
        }

        private void BindPanel(FoundationDemoPanel target)
        {
            target.AddScoreRequested += AddScore;
            target.DamageRequested += Damage;
            target.HealRequested += Heal;
            target.SaveRequested += Save;
            target.LoadRequested += Load;
            target.Destroyed += OnPanelDestroyed;
            session.Score.RegisterWithInitValue(target.SetScore)
                .UnRegisterOnDestroy(target.gameObject);
            session.Health.RegisterWithInitValue(target.SetHealth)
                .UnRegisterOnDestroy(target.gameObject);

            scoreBinding = new EventBinding<DemoScoreChangedEvent>(OnScoreChanged);
            MyEventBus<DemoScoreChangedEvent>.Register(scoreBinding);
        }

        private void UnbindPanel(FoundationDemoPanel target)
        {
            if (scoreBinding != null)
            {
                MyEventBus<DemoScoreChangedEvent>.Unregister(scoreBinding);
                scoreBinding = null;
            }

            if (target == null) return;
            target.AddScoreRequested -= AddScore;
            target.DamageRequested -= Damage;
            target.HealRequested -= Heal;
            target.SaveRequested -= Save;
            target.LoadRequested -= Load;
            target.Destroyed -= OnPanelDestroyed;
        }

        private void AddScore() => session.AddScore(10);
        private void Damage() => session.Damage(10f);
        private void Heal() => session.Heal(10f);

        private void Save()
        {
            saveData.Coins = session.Score.Value;
            saveData.TapRushHighScore = Mathf.Max(saveData.TapRushHighScore, saveData.Coins);
            datasave.Save(saveData);
            panel.SetStatus($"Save PASS | coins={saveData.Coins}");
        }

        private void Load()
        {
            saveData = datasave.Load<TemplatePlayerSave>();
            session.SetScore(saveData.Coins);
            panel.SetStatus($"Load PASS | coins={saveData.Coins}");
        }

        private void OnScoreChanged(DemoScoreChangedEvent value)
        {
            panel?.SetStatus($"EventBus PASS | score={value.Score} delta={value.Delta}");
        }

        private void TogglePanel()
        {
            if (!isTransitioning)
            {
                SetPanelVisibleAsync(panel == null).Forget();
            }
        }

        private async UniTask SetPanelVisibleAsync(bool visible)
        {
            isTransitioning = true;
            togglePanelButton.interactable = false;
            try
            {
                if (visible)
                {
                    if (panel == null)
                    {
                        await CreateAndBindPanelAsync();
                    }
                }
                else if (panel != null)
                {
                    FoundationDemoPanel panelToHide = panel;
                    panel = null;
                    UnbindPanel(panelToHide);
                    await panelToHide.Hide();
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                isTransitioning = false;
                if (!isShuttingDown && togglePanelButton != null)
                {
                    togglePanelButton.interactable = true;
                }
            }
        }

        private void OnPanelDestroyed()
        {
            FoundationDemoPanel destroyedPanel = panel;
            panel = null;
            UnbindPanel(destroyedPanel);
        }

        private void OnDestroy()
        {
            isShuttingDown = true;
            UnbindPanel(panel);
            panel = null;
            if (togglePanelButton != null)
            {
                togglePanelButton.onClick.RemoveListener(TogglePanel);
            }
            ownedPool?.Dispose();
            if (ownedPool != null)
            {
                ServiceLocator.Unregister<IPoolService>();
            }
        }
    }
}
