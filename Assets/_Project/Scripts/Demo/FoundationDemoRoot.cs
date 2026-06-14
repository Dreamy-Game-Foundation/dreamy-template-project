using System;
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
        private IDatasaveService datasave;
        private TemplateSave saveData;
        private FoundationDemoPanel panel;
        private int score;
        private float health = 100f;
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
            try
            {
                await UniTask.WaitUntil(
                    () => GameInstaller.State is BootstrapState.Ready or BootstrapState.Failed,
                    cancellationToken: this.GetCancellationTokenOnDestroy());

                if (GameInstaller.State == BootstrapState.Failed)
                {
                    Debug.LogError(
                        $"[FoundationDemo] Bootstrap failed: {GameInstaller.InitializationException}",
                        this);
                    return;
                }

                InitializeDemo();
                await SetPanelVisibleAsync(true);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void InitializeDemo()
        {
            if (!ServiceLocator.IsRegistered<IPoolService>())
            {
                ownedPool = new LeanPoolService();
                ServiceLocator.Register<IPoolService>(ownedPool);
            }

            datasave = ServiceLocator.Get<IDatasaveService>();
            saveData = datasave.Load<TemplateSave>();
            saveData.LaunchCount++;
            datasave.Save(saveData);
        }

        private async UniTask CreatePanelAsync()
        {
            FoundationDemoPanel created = await PanelManager.Instance.Show<FoundationDemoPanel>(
                Address.FoundationDemoPanel);
            if (isShuttingDown)
            {
                await created.Hide();
                return;
            }

            panel = created;
            BindPanel(created);
            RefreshPanel();

            TemplateConfig config = ServiceLocator.Get<IDataConfigService>()
                .GetTable<TemplateConfig>();
            created.SetStatus(
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
        }

        private void UnbindPanel(FoundationDemoPanel target)
        {
            if (ReferenceEquals(target, null)) return;
            target.AddScoreRequested -= AddScore;
            target.DamageRequested -= Damage;
            target.HealRequested -= Heal;
            target.SaveRequested -= Save;
            target.LoadRequested -= Load;
            target.Destroyed -= OnPanelDestroyed;
        }

        private void AddScore()
        {
            score += 10;
            RefreshPanel();
            panel?.SetStatus($"Score changed | score={score}");
        }

        private void Damage()
        {
            health = Mathf.Max(0f, health - 10f);
            RefreshPanel();
        }

        private void Heal()
        {
            health = Mathf.Min(100f, health + 10f);
            RefreshPanel();
        }

        private void Save()
        {
            saveData.Coins = score;
            saveData.Score = Mathf.Max(saveData.Score, score);
            datasave.Save(saveData);
            panel?.SetStatus($"Save PASS | coins={saveData.Coins}");
        }

        private void Load()
        {
            TemplateConfig config = ServiceLocator.Get<IDataConfigService>()
                .GetTable<TemplateConfig>();
            score = config.StartingCoins;
            RefreshPanel();
            panel?.SetStatus(
                $"Config Load PASS | coins={config.StartingCoins} ");
        }

        private void RefreshPanel()
        {
            if (panel == null) return;
            panel.SetScore(score);
            panel.SetHealth(health);
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
                    if (panel == null) await CreatePanelAsync();
                }
                else if (panel != null)
                {
                    FoundationDemoPanel hidingPanel = panel;
                    panel = null;
                    UnbindPanel(hidingPanel);
                    await hidingPanel.Hide();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
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
            FoundationDemoPanel destroyed = panel;
            panel = null;
            UnbindPanel(destroyed);
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
