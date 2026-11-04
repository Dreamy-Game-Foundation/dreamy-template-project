using System.Threading;
using Cysharp.Threading.Tasks;

namespace Dreamy.Settings
{
    public interface ISettingsService
    {
        SettingsViewState GetState();
        void SetMusicVolume(float volume);
        void SetSfxVolume(float volume);
        void SetHapticsEnabled(bool enabled);
        UniTask<SettingsOperationResult> ShowGdprConsentAsync(CancellationToken cancellationToken = default);
        UniTask<SettingsOperationResult> RestorePurchasesAsync(CancellationToken cancellationToken = default);
        UniTask<SettingsOperationResult> OpenStoreAsync(CancellationToken cancellationToken = default);
        UniTask<SettingsOperationResult> SubmitRatingAsync(int rating, CancellationToken cancellationToken = default);
        UniTask<SettingsOperationResult> RequestReviewAsync(CancellationToken cancellationToken = default);
    }
}
