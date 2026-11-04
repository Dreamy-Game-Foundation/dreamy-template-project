using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dreamy.Economy;
using Dreamy.Feedback;
using Dreamy.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

namespace Dreamy.Template.Tests
{
    public sealed class ShopFeedbackIntegrationTests
    {
        [UnityTest] public IEnumerator ShopPrefabCommitsWalletThenUpdatesHolderAfterRealIconFly() => ShopPresentationFixture(0);
        [UnityTest] public IEnumerator ShopHideHookStopsFlightAndReleasesWhileGameObjectRemainsActive() => ShopPresentationFixture(1);
        [UnityTest] public IEnumerator ShopPresenterDisposeStopsFlightAndReleasesRewardHolder() => ShopPresentationFixture(2);

        private IEnumerator ShopPresentationFixture(int cleanup)
        {
#if UNITY_EDITOR
            string saveDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dreamy-template-flight-" + Guid.NewGuid().ToString("N"));
            var canvas = new GameObject("ShopFeedbackCanvas", typeof(Canvas));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var rig = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/_Feature/Dreamy Feedback/0.4.0/Basic Feedback/Generated/Rigs/GameFeedbackRig.prefab"));
            var viewRoot = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/_Feature/Dreamy Shop/0.1.2/Shop Feature/Prefabs/ShopPanel.prefab"), canvas.transform);
            Dreamy.Shop.ShopPresenter presenter = null;
            try
            {
                var wallet = new Dreamy.Economy.DatasaveResourceWallet(new Dreamy.Datasave.DatasaveService(
                    new Dreamy.Datasave.DatasaveOptions { DirectoryName = saveDirectory }),
                    initialBalances: new[] { new ResourceAmount(new ResourceId("currency.coin"), 100) });
                wallet.TryGrant(new ResourceGrantRequest("shop-test-gems", new ResourceAmount(new ResourceId("currency.gem"), 100)));
                var reward = new Dreamy.Shop.ShopResourceConfig();
                Set(reward, "resourceId", "currency.coin"); Set(reward, "amount", 1000L);
                var offer = new Dreamy.Shop.ShopOfferConfig();
                Set(offer, "id", "test-offer"); Set(offer, "costResourceId", "currency.gem"); Set(offer, "costAmount", 15L);
                Set(offer, "rewards", new List<Dreamy.Shop.ShopResourceConfig> { reward });
                var catalog = new Dreamy.Shop.ShopCatalogConfig();
                Set(catalog, "catalogId", "feedback-test"); Set(catalog, "offers", new List<Dreamy.Shop.ShopOfferConfig> { offer });
                var panel = viewRoot.GetComponent<Dreamy.Feature.Shop.Integration.ShopPanel>();
                foreach (var holder in viewRoot.GetComponentsInChildren<Dreamy.Economy.UI.ResourceUIHolder>()) holder.Bind(wallet);
                var coinHolder = viewRoot.GetComponentsInChildren<Dreamy.Economy.UI.ResourceUIHolder>().Single(h => h.ResourceId.Value == "currency.coin");
                var text = (TMPro.TMP_Text)typeof(Dreamy.Economy.UI.ResourceUIHolder).GetField("amountText", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(coinHolder);
                var gemHolder = viewRoot.GetComponentsInChildren<Dreamy.Economy.UI.ResourceUIHolder>().Single(h => h.ResourceId.Value == "currency.gem");
                var gemText = (TMPro.TMP_Text)typeof(Dreamy.Economy.UI.ResourceUIHolder).GetField("amountText", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(gemHolder);
                // A separate Home holder tracks the same wallet and must keep auto refreshing.
                var homeRoot = new GameObject("IndependentHomeHolder", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
                homeRoot.transform.SetParent(canvas.transform, false);
                var homeText = homeRoot.GetComponent<TMPro.TMP_Text>();
                var homeHolder = homeRoot.AddComponent<Dreamy.Economy.UI.ResourceUIHolder>();
                Set(homeHolder, "amountText", homeText);
                homeHolder.SetResource("currency.coin");
                homeHolder.Bind(wallet);
                panel.BindFeedbackHost(rig.GetComponent<FeedbackHost>());
                presenter = new Dreamy.Shop.ShopPresenter(new Dreamy.Shop.ShopModel(catalog, wallet), panel, panel.PresentPurchaseAsync);
                presenter.Show(); Canvas.ForceUpdateCanvases();
                typeof(Dreamy.Feature.Shop.Integration.ShopPanel).GetMethod("RequestPurchase", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(panel, new object[] { "test-offer" });
                Assert.That(wallet.GetBalance(new ResourceId("currency.coin")), Is.EqualTo(1100));
                Assert.That(text.text, Is.EqualTo("100"), "Reward HUD waits for IconFly completion.");
                Assert.That(gemText.text, Is.EqualTo("85"), "Cost HUD updates while IconFly is still running.");
                Assert.That(homeText.text, Is.EqualTo("1.1K"), "Shop hold must not freeze Home.");
                var player = viewRoot.GetComponent<FeedbackPlayer>();
                Assert.That(player.LastHandle.IsRunning, Is.True, "The wired prefab must play an actual IconFly.");
                if (cleanup == 1)
                {
                    // Exercise the pre-hide hook without disabling the holder/GameObject.
                    var hiding = (Action)typeof(UIPanel).GetField("OnPreHide", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(panel);
                    hiding.Invoke();
                    Assert.That(viewRoot.activeInHierarchy, Is.True);
                    Assert.That(text.text, Is.EqualTo("1.1K"), "Close releases immediately, before async completion.");
                }
                else if (cleanup == 2)
                {
                    presenter.Dispose();
                    Assert.That(text.text, Is.EqualTo("1.1K"));
                }
                float deadline = Time.realtimeSinceStartup + 8;
                while (player.LastHandle.IsRunning && Time.realtimeSinceStartup < deadline) yield return null;
                yield return null;
                Assert.That(player.LastHandle.Status, Is.EqualTo(cleanup == 0 ? FeedbackStatus.Completed : FeedbackStatus.Stopped));
                Assert.That(text.text, Is.EqualTo("1.1K"));
                Assert.That(wallet.GetBalance(new ResourceId("currency.gem")), Is.EqualTo(85));
            }
            finally { presenter?.Dispose(); Object.DestroyImmediate(viewRoot); Object.DestroyImmediate(rig); Object.DestroyImmediate(canvas); if (System.IO.Directory.Exists(saveDirectory)) System.IO.Directory.Delete(saveDirectory, true); }
#else
            Assert.Ignore("Prefab fixture requires the Editor."); yield return null;
#endif
        }

        private static void Set(object target, string field, object value) =>
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
