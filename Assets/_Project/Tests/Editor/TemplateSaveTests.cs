using System;
using System.IO;
using Dreamy.Datasave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dreamy.Template.Tests
{
    public sealed class TemplateSaveTests
    {
        [TestCase(42)]
        [TestCase(0)]
        public void LoadVersionOne_PreservesScoreAndDemoCount_ThenWritesVersionTwo(int score)
        {
            string directory = Path.Combine(Path.GetTempPath(), "dreamy-template-save-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                string path = Path.Combine(directory, "TemplateSave.json");
                var envelope = new JObject
                {
                    ["FormatVersion"] = 1,
                    ["DataVersion"] = 1,
                    ["DataType"] = typeof(TemplateSave).AssemblyQualifiedName,
                    ["Payload"] = JsonConvert.SerializeObject(new { LaunchCount = 7, Coins = score, Score = 99 })
                };
                File.WriteAllText(path, JsonConvert.SerializeObject(envelope));
                var service = new DatasaveService(new DatasaveOptions { DirectoryName = directory });

                TemplateSave save = service.Load<TemplateSave>();
                Assert.That(save.CurrentScore, Is.EqualTo(score));
                Assert.That(save.BestScore, Is.EqualTo(99));
                Assert.That(save.DemoOpenCount, Is.EqualTo(7));
                Assert.That(save.IsInitialized, Is.True);
                Assert.That(save.SaveKey, Is.EqualTo("TemplateSave"));

                service.Save(save);
                JObject upgraded = JObject.Parse(File.ReadAllText(path));
                Assert.That(upgraded.Value<int>("DataVersion"), Is.EqualTo(2));
                JObject payload = JObject.Parse(upgraded.Value<string>("Payload"));
                Assert.That(payload["Coins"], Is.Null);
                Assert.That(payload["Score"], Is.Null);
                Assert.That(payload["LaunchCount"], Is.Null);
                TemplateSave reloaded = service.Load<TemplateSave>();
                Assert.That(reloaded.CurrentScore, Is.EqualTo(score));
                Assert.That(reloaded.BestScore, Is.EqualTo(99));
                Assert.That(reloaded.DemoOpenCount, Is.EqualTo(7));
                Assert.That(reloaded.IsInitialized, Is.True);
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        [Test]
        public void NewSave_IsNotInitialized_UntilDemoSeedsIt()
        {
            Assert.That(new TemplateSave().IsInitialized, Is.False);
        }

        [TestCase("{\"startingCoins\":36}")]
        [TestCase("{\"startingScore\":36}")]
        public void Config_AcceptsLegacyAndCurrentScoreName(string json)
        {
            TemplateConfig config = JsonConvert.DeserializeObject<TemplateConfig>(json);
            Assert.That(config.StartingScore, Is.EqualTo(36));
            Assert.That(JObject.Parse(JsonConvert.SerializeObject(config))["startingCoins"], Is.Null);
        }
    }
}
