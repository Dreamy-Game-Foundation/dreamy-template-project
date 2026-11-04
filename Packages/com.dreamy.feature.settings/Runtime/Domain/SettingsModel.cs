using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Audio;

namespace Dreamy.Settings
{
    public sealed class SettingsModel : ISettingsService, ISettingsLanguageService
    {
        private readonly IAudioService audioService;
        private readonly ISettingsPlatformGateway platformGateway;

        private readonly ISettingsHapticsGateway hapticsGateway;
        private readonly ISettingsReviewGateway reviewGateway;
        private bool reviewInProgress;
        private readonly ISettingsLanguageGateway languageGateway;
        private bool languageBusy;
        private string languageError;
        private event Action languageChanged;
        public event Action LanguageChanged
        {
            add { languageChanged += value; if (languageGateway != null) languageGateway.LanguageChanged += value; }
            remove { languageChanged -= value; if (languageGateway != null) languageGateway.LanguageChanged -= value; }
        }
        public SettingsLanguageState GetLanguageState() => new(languageGateway?.Options,
            languageGateway?.CurrentCode, languageBusy, languageError);
        public async UniTask SetLanguageAsync(string code, CancellationToken token = default)
        {
            if (languageGateway == null || languageBusy) return;
            languageBusy = true; languageError = null; languageChanged?.Invoke();
            try
            {
                if (!await languageGateway.SetLanguageAsync(code, token)) languageError = "settings.language_unavailable";
            }
            catch (OperationCanceledException) { }
            catch (Exception) { languageError = "settings.language_failed"; }
            finally { languageBusy = false; languageChanged?.Invoke(); }
        }

        public SettingsModel(
            IAudioService audioService,
            ISettingsPlatformGateway platformGateway = null,
            ISettingsHapticsGateway hapticsGateway = null,
            ISettingsReviewGateway reviewGateway = null,
            ISettingsLanguageGateway languageGateway = null)
        {
            this.audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            this.platformGateway = platformGateway;
            this.hapticsGateway = hapticsGateway ?? platformGateway as ISettingsHapticsGateway;
            this.reviewGateway = reviewGateway ?? platformGateway as ISettingsReviewGateway;
            this.languageGateway = languageGateway;
        }

        public SettingsViewState GetState() => new(
            audioService.GetVolume(AudioBusId.Music),
            audioService.GetVolume(AudioBusId.Sfx),
            platformGateway?.CanShowGdprConsent ?? false,
            platformGateway?.CanRestorePurchases ?? false,
            platformGateway?.CanOpenStore ?? false,
            hapticsGateway?.IsSupported == true && hapticsGateway.IsEnabled,
            hapticsGateway?.IsSupported ?? false,
            !reviewInProgress && (reviewGateway?.HasPositiveRating == true
                ? reviewGateway.CanGrantReward && !reviewGateway.HasClaimedReward
                : platformGateway?.CanOpenStore == true),
            reviewGateway?.HasPositiveRating ?? false,
            reviewGateway?.HasClaimedReward ?? false);

        public void SetMusicVolume(float volume) =>
            audioService.SetVolume(AudioBusId.Music, Clamp01(volume));

        public void SetSfxVolume(float volume) =>
            audioService.SetVolume(AudioBusId.Sfx, Clamp01(volume));

        public void SetHapticsEnabled(bool enabled)
        {
            if (hapticsGateway?.IsSupported == true && hapticsGateway.IsEnabled != enabled)
            {
                hapticsGateway.SetEnabled(enabled);
            }
        }

        public UniTask<SettingsOperationResult> ShowGdprConsentAsync(CancellationToken cancellationToken = default) =>
            platformGateway?.CanShowGdprConsent == true
                ? platformGateway.ShowGdprConsentAsync(cancellationToken)
                : UniTask.FromResult(SettingsOperationResult.Unavailable());

        public UniTask<SettingsOperationResult> RestorePurchasesAsync(CancellationToken cancellationToken = default) =>
            platformGateway?.CanRestorePurchases == true
                ? platformGateway.RestorePurchasesAsync(cancellationToken)
                : UniTask.FromResult(SettingsOperationResult.Unavailable());

        public UniTask<SettingsOperationResult> OpenStoreAsync(CancellationToken cancellationToken = default) =>
            platformGateway?.CanOpenStore == true
                ? platformGateway.OpenStoreAsync(cancellationToken)
                : UniTask.FromResult(SettingsOperationResult.Unavailable());

        // Explicit Rate Us actions open the store listing rather than a quota-limited native prompt.
        public UniTask<SettingsOperationResult> RequestReviewAsync(CancellationToken cancellationToken = default) =>
            OpenStoreAsync(cancellationToken);

        public async UniTask<SettingsOperationResult> SubmitRatingAsync(
            int rating, CancellationToken cancellationToken = default)
        {
            if (rating < 4 || rating > 5 || reviewInProgress || cancellationToken.IsCancellationRequested)
            {
                return SettingsOperationResult.Unavailable("Rating is not eligible or a request is in progress.");
            }

            reviewInProgress = true;
            try
            {
                if (reviewGateway?.HasPositiveRating != true)
                {
                    SettingsOperationResult result = await RequestReviewAsync(cancellationToken);
                    if (!result.IsSuccess || cancellationToken.IsCancellationRequested)
                    {
                        return cancellationToken.IsCancellationRequested
                            ? SettingsOperationResult.Unavailable("Review request cancelled.")
                            : result;
                    }

                    reviewGateway?.RecordPositiveRating(rating);
                }

                if (reviewGateway?.CanGrantReward == true && !reviewGateway.HasClaimedReward)
                {
                    return await reviewGateway.GrantRewardOnceAsync(cancellationToken);
                }

                return SettingsOperationResult.Succeeded("Positive rating/request recorded; store review is unverified.");
            }
            catch (OperationCanceledException)
            {
                return SettingsOperationResult.Unavailable("Review request cancelled.");
            }
            catch (Exception exception)
            {
                return SettingsOperationResult.Failed(exception.Message);
            }
            finally
            {
                reviewInProgress = false;
            }
        }

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
    }
}
