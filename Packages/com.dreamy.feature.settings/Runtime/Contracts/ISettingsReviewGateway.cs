using System.Threading;
using Cysharp.Threading.Tasks;

namespace Dreamy.Settings
{
    /// <summary>Host-owned positive rating/request state and optional idempotent reward.</summary>
    /// <remarks>Opening a store does not verify a published review. Reward grants must persist
    /// a stable transaction ID together with the wallet mutation before returning success.</remarks>
    public interface ISettingsReviewGateway
    {
        bool HasPositiveRating { get; }
        bool HasClaimedReward { get; }
        bool CanGrantReward { get; }
        void RecordPositiveRating(int rating);
        UniTask<SettingsOperationResult> GrantRewardOnceAsync(CancellationToken cancellationToken = default);
    }
}
