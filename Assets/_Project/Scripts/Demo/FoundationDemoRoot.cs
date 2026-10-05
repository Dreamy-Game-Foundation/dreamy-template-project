using System;
using Cysharp.Threading.Tasks;
using Dreamy.DataConfig;
using Dreamy.Datasave;
using UnityEngine;

namespace Dreamy.Template.Demo
{
    public sealed class FoundationDemoRoot : IDisposable
    {
        private readonly IDatasaveService datasave;
        private readonly FoundationDemoPanel panel;
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
            if (dataConfig == null) throw new ArgumentNullException(nameof(dataConfig));

            TemplateConfig config = dataConfig.GetTable<TemplateConfig>();
            saveData = datasave.Load<TemplateSave>();
            saveData.LaunchCount++;
            datasave.Save(saveData);
            score = config.StartingCoins;

            panel.AddScoreRequested += AddScore;
            panel.DamageRequested += Damage;
            panel.HealRequested += Heal;
            panel.SaveRequested += Save;
            panel.LoadRequested += Load;
            panel.CloseRequested += Close;
            panel.OnPostHide += Dispose;
            panel.Destroyed += Dispose;
            RefreshPanel();
            panel.SetStatus($"Demo opened | count={saveData.LaunchCount} | config coins={config.StartingCoins}");
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
            saveData.Coins = score;
            saveData.Score = Mathf.Max(saveData.Score, score);
            datasave.Save(saveData);
            panel.SetStatus($"Saved | coins={saveData.Coins} | best score={saveData.Score}");
        }

        private void Load()
        {
            saveData = datasave.Load<TemplateSave>();
            score = saveData.Coins;
            RefreshPanel();
            panel.SetStatus($"Loaded | coins={saveData.Coins} | best score={saveData.Score}");
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
            panel.OnPostHide -= Dispose;
            panel.Destroyed -= Dispose;
        }
    }
}