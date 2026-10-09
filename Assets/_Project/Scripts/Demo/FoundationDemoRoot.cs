using System;
using Cysharp.Threading.Tasks;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using Dreamy.UI;
using UnityEngine;

namespace Dreamy.Template.Demo
{
    public sealed class FoundationDemoRoot : IPanelPresenter
    {
        private readonly IDatasaveService datasave;
        private readonly FoundationDemoPanel panel;
        private readonly IDataConfigService dataConfig;
        private bool isBound;
        private TemplateSave saveData;
        private int score;
        private float health = 100f;
        private bool disposed;

        public FoundationDemoRoot(
            FoundationDemoPanel panel,
            IDatasaveService datasave,
            IDataConfigService dataConfig)
        {
            this.panel = panel ?? throw new ArgumentNullException(nameof(panel));
            this.datasave = datasave ?? throw new ArgumentNullException(nameof(datasave));
            this.dataConfig = dataConfig ?? throw new ArgumentNullException(nameof(dataConfig));
        }

        public void Show()
        {
            if (disposed || isBound) return;
            isBound = true;
            TemplateConfig config = dataConfig.GetTable<TemplateConfig>();
            saveData = datasave.Load<TemplateSave>();
            if (!saveData.IsInitialized)
            {
                saveData.CurrentScore = config.StartingScore;
                saveData.BestScore = Mathf.Max(saveData.BestScore, saveData.CurrentScore);
                saveData.IsInitialized = true;
            }
            saveData.DemoOpenCount++;
            datasave.Save(saveData);
            score = saveData.CurrentScore;

            panel.AddScoreRequested += AddScore;
            panel.DamageRequested += Damage;
            panel.HealRequested += Heal;
            panel.SaveRequested += Save;
            panel.LoadRequested += Load;
            panel.CloseRequested += Close;
            RefreshPanel();
            panel.SetStatus($"Demo opened | count={saveData.DemoOpenCount} | starting score={config.StartingScore}");
        }

        private void AddScore()
        {
            score += 10;
            RefreshPanel();
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
            saveData.CurrentScore = score;
            saveData.BestScore = Mathf.Max(saveData.BestScore, score);
            datasave.Save(saveData);
            panel.SetStatus($"Saved | score={saveData.CurrentScore} | best score={saveData.BestScore}");
        }

        private void Load()
        {
            saveData = datasave.Load<TemplateSave>();
            score = saveData.CurrentScore;
            RefreshPanel();
            panel.SetStatus($"Loaded | score={saveData.CurrentScore} | best score={saveData.BestScore}");
        }

        private void RefreshPanel()
        {
            panel.SetScore(score);
            panel.SetHealth(health);
        }

        private void Close() => panel.Hide().Forget();

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            panel.AddScoreRequested -= AddScore;
            panel.DamageRequested -= Damage;
            panel.HealRequested -= Heal;
            panel.SaveRequested -= Save;
            panel.LoadRequested -= Load;
            panel.CloseRequested -= Close;
        }
    }
}
