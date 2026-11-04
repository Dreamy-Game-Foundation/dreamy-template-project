namespace Dreamy.Settings
{
    public readonly struct SettingsViewState
    {
        public SettingsViewState(
            float musicVolume,
            float sfxVolume,
            bool canShowGdprConsent,
            bool canRestorePurchases,
            bool canOpenStore,
            bool hapticsEnabled = false,
            bool canSetHaptics = false,
            bool canRequestReview = false,
            bool hasPositiveRating = false,
            bool hasClaimedReviewReward = false)
        {
            MusicVolume = musicVolume;
            SfxVolume = sfxVolume;
            CanShowGdprConsent = canShowGdprConsent;
            CanRestorePurchases = canRestorePurchases;
            CanOpenStore = canOpenStore;
            HapticsEnabled = hapticsEnabled;
            CanSetHaptics = canSetHaptics;
            CanRequestReview = canRequestReview;
            HasPositiveRating = hasPositiveRating;
            HasClaimedReviewReward = hasClaimedReviewReward;
        }

        public bool CanRequestReview { get; }
        public bool HasPositiveRating { get; }
        public bool HasClaimedReviewReward { get; }
        public bool HapticsEnabled { get; }
        public bool CanSetHaptics { get; }
        public float MusicVolume { get; }
        public float SfxVolume { get; }
        public bool CanShowGdprConsent { get; }
        public bool CanRestorePurchases { get; }
        public bool CanOpenStore { get; }
    }
}
