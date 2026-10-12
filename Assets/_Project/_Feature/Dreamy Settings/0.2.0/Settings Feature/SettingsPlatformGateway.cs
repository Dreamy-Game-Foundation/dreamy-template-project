using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Settings;
using UnityEngine;

namespace Dreamy.Feature.Settings.Integration
{
    /// <summary>Store listing launch plus optional host-owned consent and restore operations.</summary>
    public sealed class SettingsPlatformGateway : ISettingsPlatformGateway
    {
        private readonly string storeListingUrl;
        private readonly Func<CancellationToken, UniTask<SettingsOperationResult>> consent;
        private readonly Func<CancellationToken, UniTask<SettingsOperationResult>> restore;

        public SettingsPlatformGateway(string storeListingUrl = null,
            Func<CancellationToken, UniTask<SettingsOperationResult>> consent = null,
            Func<CancellationToken, UniTask<SettingsOperationResult>> restore = null)
        {
            if (!string.IsNullOrWhiteSpace(storeListingUrl) &&
                (!Uri.TryCreate(storeListingUrl, UriKind.Absolute, out var uri) ||
                 (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)))
                throw new ArgumentException("Store listing URL must be an absolute HTTP(S) URL.", nameof(storeListingUrl));
            this.storeListingUrl = string.IsNullOrWhiteSpace(storeListingUrl) ? null : storeListingUrl;
            this.consent = consent;
            this.restore = restore;
        }

        private string ListingUrl => storeListingUrl ??
            (Application.platform == RuntimePlatform.Android
                ? "https://play.google.com/store/apps/details?id=" + Uri.EscapeDataString(Application.identifier)
                : null);

        public bool CanShowGdprConsent => consent != null;
        public bool CanRestorePurchases => restore != null;
        public bool CanOpenStore => !string.IsNullOrEmpty(ListingUrl);
        public bool CanRequestReview => CanOpenStore;

        public UniTask<SettingsOperationResult> ShowGdprConsentAsync(CancellationToken token = default) =>
            Run(consent, token, "Consent provider is not configured.");
        public UniTask<SettingsOperationResult> RestorePurchasesAsync(CancellationToken token = default) =>
            Run(restore, token, "Restore provider is not configured.");

        public UniTask<SettingsOperationResult> OpenStoreAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested || !CanOpenStore)
                return UniTask.FromResult(SettingsOperationResult.Unavailable("Store listing is not available."));
            try
            {
                Application.OpenURL(ListingUrl);
                // OpenURL does not confirm that the user published a review.
                return UniTask.FromResult(SettingsOperationResult.Succeeded("Store launch requested."));
            }
            catch (Exception exception)
            {
                return UniTask.FromResult(SettingsOperationResult.Failed(exception.Message));
            }
        }

        public UniTask<SettingsOperationResult> RequestReviewAsync(CancellationToken token = default) => OpenStoreAsync(token);

        private static UniTask<SettingsOperationResult> Run(
            Func<CancellationToken, UniTask<SettingsOperationResult>> operation, CancellationToken token, string unavailable)
        {
            if (token.IsCancellationRequested || operation == null)
                return UniTask.FromResult(SettingsOperationResult.Unavailable(unavailable));
            return operation(token);
        }
    }
}
