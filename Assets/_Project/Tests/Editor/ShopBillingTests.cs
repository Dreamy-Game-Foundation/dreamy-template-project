using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Shop;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Dreamy.Template.Tests
{
    public sealed class ShopBillingTests
    {
        private GameObject root;
        private GameInstaller installer;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Shop billing fixture");
            root.SetActive(false);
            installer = root.AddComponent<GameInstaller>();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(root);

        [Test]
        public void EditorPurchases_SucceedWithDistinctTransactionIds()
        {
            var first = Purchase();
            var second = Purchase();

            Assert.That(first.IsSuccess, Is.True);
            Assert.That(second.IsSuccess, Is.True);
            Assert.That(first.TransactionId, Does.StartWith("editor-iap-"));
            Assert.That(second.TransactionId, Is.Not.EqualTo(first.TransactionId));
        }

        [Test]
        public void CancelledPurchase_DoesNotReturnATransaction()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            var result = Purchase(cancellation.Token);

            Assert.That(result.Status, Is.EqualTo(ShopGatewayPurchaseStatus.Cancelled));
            Assert.That(result.TransactionId, Is.Null);
        }

        [Test]
        public void DisabledSimulation_FailsWithoutGrantingATransaction()
        {
            typeof(GameInstaller).GetField("simulateIapPurchasesInEditor", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(installer, false);
            LogAssert.Expect(LogType.Warning, "[Shop] Billing is not configured for test.product.");

            var result = Purchase();

            Assert.That(result.Status, Is.EqualTo(ShopGatewayPurchaseStatus.Failed));
            Assert.That(result.TransactionId, Is.Null);
        }

        private ShopGatewayPurchaseResult Purchase(CancellationToken token = default) =>
            ((UniTask<ShopGatewayPurchaseResult>)typeof(GameInstaller)
                .GetMethod("PurchaseWithBillingAsync", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(installer, new object[] { new ShopGatewayPurchaseRequest("test-offer", "test.product"), token }))
            .GetAwaiter().GetResult();
    }
}
