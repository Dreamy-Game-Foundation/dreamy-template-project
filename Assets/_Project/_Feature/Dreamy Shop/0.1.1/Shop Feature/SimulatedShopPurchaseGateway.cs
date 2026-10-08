using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Shop;
using UnityEngine;

namespace Dreamy.Feature.Shop.Integration
{
    public sealed class SimulatedShopPurchaseGateway : IShopPurchaseGateway
    {
        public UniTask<ShopGatewayPurchaseResult> PurchaseAsync(
            ShopGatewayPurchaseRequest request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Debug.Log($"[Shop] Cancelled: {request.OfferId}");
                return UniTask.FromResult(ShopGatewayPurchaseResult.Cancelled());
            }

            string transactionId = Guid.NewGuid().ToString("N");
            Debug.Log($"[Shop] Purchased: {request.OfferId}");
            return UniTask.FromResult(ShopGatewayPurchaseResult.Purchased(transactionId));
        }
    }
}
