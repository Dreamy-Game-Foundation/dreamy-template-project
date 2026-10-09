using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Settings;

namespace Dreamy.Template
{
    public sealed class SettingsPlatformGateway : ISettingsPlatformGateway
    {
        public bool CanShowGdprConsent => false;
        public bool CanRestorePurchases => false;
        public bool CanOpenStore => true;
        public bool CanRequestReview => true;

        public UniTask<SettingsOperationResult> ShowGdprConsentAsync(
            CancellationToken cancellationToken = default) =>
            Unavailable("DreamySDK consent integration is not configured.", cancellationToken);

        public UniTask<SettingsOperationResult> RestorePurchasesAsync(
            CancellationToken cancellationToken = default) =>
            Unavailable("DreamySDK restore purchases integration is not configured.", cancellationToken);

        public UniTask<SettingsOperationResult> OpenStoreAsync(
            CancellationToken cancellationToken = default) =>
            Unavailable("DreamySDK store integration is not configured.", cancellationToken);

        public UniTask<SettingsOperationResult> RequestReviewAsync(
            CancellationToken cancellationToken = default) =>
            Unavailable("DreamySDK native review integration is not configured.", cancellationToken);

        private static UniTask<SettingsOperationResult> Unavailable(
            string message,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return UniTask.FromResult(SettingsOperationResult.Unavailable(message));
        }
    }
}