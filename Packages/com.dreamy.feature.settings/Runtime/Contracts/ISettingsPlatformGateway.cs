using System.Threading;
using Cysharp.Threading.Tasks;

namespace Dreamy.Settings
{
    public interface ISettingsPlatformGateway
    {
        bool CanShowGdprConsent { get; }
        bool CanRestorePurchases { get; }
        bool CanOpenStore { get; }
        bool CanRequestReview { get; }

        UniTask<SettingsOperationResult> ShowGdprConsentAsync(CancellationToken cancellationToken = default);
        UniTask<SettingsOperationResult> RestorePurchasesAsync(CancellationToken cancellationToken = default);
        UniTask<SettingsOperationResult> OpenStoreAsync(CancellationToken cancellationToken = default);
        UniTask<SettingsOperationResult> RequestReviewAsync(CancellationToken cancellationToken = default);
    }
}
