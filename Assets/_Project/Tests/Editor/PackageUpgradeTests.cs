using System;
using System.IO;
using Dreamy.Datasave;
using Dreamy.Economy;
using Dreamy.Economy.UI;
using Dreamy.Feature.Settings.Integration;
using Dreamy.Settings;
using Dreamy.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Dreamy.Template.Tests
{
    public sealed class PackageUpgradeTests
    {
        [TestCase("Assets/_Project/_Feature/Dreamy Economy Contracts/0.2.1/Resource Holders/Prefabs/CoinHolder.prefab", 1)]
        [TestCase("Assets/_Project/_Feature/Dreamy Economy Contracts/0.2.1/Resource Holders/Prefabs/GemHolder.prefab", 1)]
        [TestCase("Assets/_Project/Prefabs/UI/EconomyHolder.prefab", 2)]
        [TestCase("Assets/_Project/_Feature/Dreamy Shop/0.1.2/Shop Feature/Prefabs/ShopPanel.prefab", 2)]
        public void HolderPrefabs_RetainBindingsAndAutomaticRefresh(string path, int count)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null);
            var holders = prefab.GetComponentsInChildren<ResourceUIHolder>(true);
            Assert.That(holders.Length, Is.EqualTo(count));
            foreach (var holder in holders)
            {
                var serialized = new SerializedObject(holder);
                Assert.That(serialized.FindProperty("autoRefresh").boolValue, Is.True, path);
                Assert.That(serialized.FindProperty("iconImage").objectReferenceValue, Is.Not.Null, path);
                Assert.That(serialized.FindProperty("amountText").objectReferenceValue, Is.Not.Null, path);
            }
        }

        [Test]
        public void LegacyWallet_MigratesBalancesEntitlementsAndPurchaseIds_AndSurvivesRestart()
        {
            string directory = Path.Combine(Path.GetTempPath(), "dreamy-template-upgrade-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                string path = Path.Combine(directory, "economy.json");
                File.WriteAllText(path, JsonConvert.SerializeObject(new
                {
                    FormatVersion = 1,
                    DataVersion = 1,
                    DataType = typeof(EconomySaveData).AssemblyQualifiedName,
                    Payload = JsonConvert.SerializeObject(new
                    {
                        Balances = new[]
                        {
                            new { ResourceId = "currency.coin", Amount = 42L },
                            new { ResourceId = "currency.gem", Amount = 7L },
                            new { ResourceId = "entitlement.remove-ads", Amount = 1L }
                        },
                        TransactionIds = new[] { "legacy-purchase" }
                    })
                }));
                var options = new DatasaveOptions { DirectoryName = directory, CreateFileOnFirstLoad = false };
                var wallet = new DatasaveResourceWallet(new DatasaveService(options));
                var coin = new ResourceId("currency.coin");
                Assert.That(wallet.TryGrant(new ResourceGrantRequest("legacy-purchase", new ResourceAmount(coin, 100))), Is.True);
                Assert.That(wallet.GetBalance(coin), Is.EqualTo(42));
                Assert.That(wallet.TryGrant(new ResourceGrantRequest("after-upgrade", new ResourceAmount(coin, 5))), Is.True);
                Assert.That(JObject.Parse(File.ReadAllText(path)).Value<int>("DataVersion"), Is.EqualTo(4));

                var restored = new DatasaveResourceWallet(new DatasaveService(options));
                Assert.That(restored.GetBalance(coin), Is.EqualTo(47));
                Assert.That(restored.GetBalance(new ResourceId("currency.gem")), Is.EqualTo(7));
                Assert.That(restored.GetBalance(new ResourceId("entitlement.remove-ads")), Is.EqualTo(1));
                Assert.That(restored.TryGrant(new ResourceGrantRequest("after-upgrade", new ResourceAmount(coin, 5))), Is.True);
                Assert.That(restored.GetBalance(coin), Is.EqualTo(47));
            }
            finally { Directory.Delete(directory, true); }
        }

        [TestCase("CoinSuggestionPanel", typeof(CoinSuggestionPanel))]
        [TestCase("GemSuggestionPanel", typeof(GemSuggestionPanel))]
        public void SuggestionPrefab_PreservesSafeAreaBindingsAndAddress(string name, Type panelType)
        {
            string path = "Assets/_Project/Prefabs/UI/ResourceSuggestions/" + name + ".prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null);
            var panel = prefab.GetComponent(panelType) as ResourceSuggestionPanel;
            Assert.That(panel, Is.Not.Null);
            Assert.That(panel.Layer, Is.EqualTo(UILayer.Popup));
            Assert.That(prefab.GetComponentInChildren<SafeArea>(true), Is.Not.Null);
            var serialized = new SerializedObject(panel);
            Assert.That(serialized.FindProperty("acquireButton").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("laterButton").objectReferenceValue, Is.Not.Null);
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            Assert.That(settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(path)).address,
                Is.EqualTo("Panel/" + name + ".prefab"));
        }

        [Test]
        public void Settings_WithoutLanguageOptions_HidesAndClearsLanguageControls()
        {
            var parent = new GameObject("Settings upgrade fixture");
            parent.SetActive(false);
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/_Feature/Dreamy Settings/0.2.2/Settings Feature/Prefabs/SettingsPanel.prefab");
                var instance = UnityEngine.Object.Instantiate(prefab, parent.transform);
                var panel = instance.GetComponent<SettingsPanel>();
                var dropdownRoot = new GameObject("Language", typeof(RectTransform), typeof(TMP_Dropdown));
                dropdownRoot.transform.SetParent(instance.transform, false);
                var dropdown = dropdownRoot.GetComponent<TMP_Dropdown>();
                dropdown.options.Add(new TMP_Dropdown.OptionData("stale locale"));
                var label = new GameObject("LanguageLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
                label.transform.SetParent(instance.transform, false);
                var error = new GameObject("LanguageError", typeof(RectTransform), typeof(TextMeshProUGUI));
                error.transform.SetParent(instance.transform, false);
                var serialized = new SerializedObject(panel);
                serialized.FindProperty("languageDropdown").objectReferenceValue = dropdown;
                serialized.FindProperty("languageLabel").objectReferenceValue = label.GetComponent<TMP_Text>();
                serialized.FindProperty("languageError").objectReferenceValue = error.GetComponent<TMP_Text>();
                serialized.ApplyModifiedPropertiesWithoutUndo();

                panel.RenderLanguage(default(SettingsLanguageState));
                Assert.That(dropdownRoot.activeSelf, Is.False);
                Assert.That(label.activeSelf, Is.False);
                Assert.That(error.activeSelf, Is.False);
                Assert.That(dropdown.options, Is.Empty);
                Assert.That(dropdown.interactable, Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(parent); }
        }
    }
}
