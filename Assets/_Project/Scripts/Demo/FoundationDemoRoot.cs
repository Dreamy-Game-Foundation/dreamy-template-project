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

        private IDatasaveService datasave;
        private TemplateSave saveData;
        private FoundationDemoPanel panel;
        private UIShopPanel shopPanel;
        
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
            target.OpenShopRequested += OpenShop;
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
            target.OpenShopRequested -= OpenShop;
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

        private void OpenShop()
        {
            OpenShopAsync().Forget();
        }

        private async UniTaskVoid OpenShopAsync()
        {
            if (isTransitioning) return;
            isTransitioning = true;
            try
            {
                if (panel != null)
                {
                    FoundationDemoPanel hidingPanel = panel;
                    panel = null;
                    UnbindPanel(hidingPanel);
                    await hidingPanel.Hide();
                }

                UIShopPanel created = await PanelManager.Instance.Show<UIShopPanel>(Address.ShopPanel);
                shopPanel = created;
                shopPanel.OpenDemoRequested += OpenDemoFromShop;
                shopPanel.Destroyed += OnShopDestroyed;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                isTransitioning = false;
            }
        }

        private void OpenDemoFromShop()
        {
            OpenDemoFromShopAsync().Forget();
        }

        private async UniTaskVoid OpenDemoFromShopAsync()
        {
            if (isTransitioning) return;
            isTransitioning = true;
            try
            {
                if (shopPanel != null)
                {
                    UIShopPanel hidingShop = shopPanel;
                    shopPanel = null;
                    hidingShop.OpenDemoRequested -= OpenDemoFromShop;
                    hidingShop.Destroyed -= OnShopDestroyed;
                    await hidingShop.Hide();
                }

                if (panel == null)
                {
                    await CreatePanelAsync();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                isTransitioning = false;
            }
        }

        private void OnShopDestroyed()
        {
            if (shopPanel != null)
            {
                shopPanel.OpenDemoRequested -= OpenDemoFromShop;
                shopPanel.Destroyed -= OnShopDestroyed;
                shopPanel = null;
            }
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
                bool showDemo = (panel == null && shopPanel == null);
                SetPanelVisibleAsync(showDemo).Forget();
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
                    if (panel == null && shopPanel == null) await CreatePanelAsync();
                }
                else
                {
                    if (panel != null)
                    {
                        FoundationDemoPanel hidingPanel = panel;
                        panel = null;
                        UnbindPanel(hidingPanel);
                        await hidingPanel.Hide();
                    }
                    if (shopPanel != null)
                    {
                        UIShopPanel hidingShop = shopPanel;
                        shopPanel = null;
                        hidingShop.OpenDemoRequested -= OpenDemoFromShop;
                        hidingShop.Destroyed -= OnShopDestroyed;
                        await hidingShop.Hide();
                    }
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

            if (shopPanel != null)
            {
                shopPanel.OpenDemoRequested -= OpenDemoFromShop;
                shopPanel.Destroyed -= OnShopDestroyed;
                shopPanel = null;
            }

            if (togglePanelButton != null)
            {
                togglePanelButton.onClick.RemoveListener(TogglePanel);
            }
        }
    }
}
