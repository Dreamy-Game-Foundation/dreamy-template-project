using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Dreamy.Audio;
using Dreamy.Settings;

internal sealed class ReviewPlatform : ISettingsPlatformGateway
{
    public bool CanShowGdprConsent => false;
    public bool CanRestorePurchases => false;
    public bool CanOpenStore { get; set; } = true;
    public bool CanRequestReview => false;
    public int StoreCalls;
    public int NativeCalls;
    public SettingsOperationResult StoreResult = SettingsOperationResult.Succeeded();
    public UniTaskCompletionSource<SettingsOperationResult> Pending;
    public UniTask<SettingsOperationResult> OpenStoreAsync(CancellationToken cancellationToken = default)
    {
        StoreCalls++;
        return Pending != null ? Pending.Task : UniTask.FromResult(StoreResult);
    }
    public UniTask<SettingsOperationResult> RequestReviewAsync(CancellationToken cancellationToken = default)
    { NativeCalls++; return UniTask.FromResult(SettingsOperationResult.Succeeded()); }
    public UniTask<SettingsOperationResult> ShowGdprConsentAsync(CancellationToken cancellationToken = default) =>
        UniTask.FromResult(SettingsOperationResult.Unavailable());
    public UniTask<SettingsOperationResult> RestorePurchasesAsync(CancellationToken cancellationToken = default) =>
        UniTask.FromResult(SettingsOperationResult.Unavailable());
}

internal sealed class ReviewGateway : ISettingsReviewGateway
{
    public bool HasPositiveRating { get; private set; }
    public bool HasClaimedReward { get; private set; }
    public bool CanGrantReward { get; set; }
    public bool FailReward;
    public int SavedRating;
    public int RewardCalls;
    public void RecordPositiveRating(int rating) { HasPositiveRating = true; SavedRating = rating; }
    public UniTask<SettingsOperationResult> GrantRewardOnceAsync(CancellationToken cancellationToken = default)
    {
        RewardCalls++;
        if (FailReward) return UniTask.FromResult(SettingsOperationResult.Failed("Reward failure"));
        HasClaimedReward = true;
        return UniTask.FromResult(SettingsOperationResult.Succeeded());
    }
}

internal sealed class ReviewView : IRateUsView
{
    public event Action<int> RatingSelected;
    public event Action RateRequested;
    public event Action CloseRequested;
    public int RenderedRating;
    public int Updates;
    public int Closes;
    public bool Interactable;
    public void SetRating(int rating) { RenderedRating = rating; Updates++; }
    public void SetInteractable(bool interactable) { Interactable = interactable; Updates++; }
    public void Close() { Closes++; Updates++; }
    public void Select(int rating) => RatingSelected?.Invoke(rating);
    public void Submit() => RateRequested?.Invoke();
}

internal static class ReviewHarness
{
    private static void Check(bool condition, string label)
    { if (!condition) throw new Exception(label); Console.WriteLine("PASS " + label); }

    public static async Task RunAsync(IAudioService audio)
    {
        var platform = new ReviewPlatform();
        var state = new ReviewGateway();
        var model = new SettingsModel(audio, platform, reviewGateway: state);
        Check(!(await model.SubmitRatingAsync(3)).IsSuccess && platform.StoreCalls == 0 && !state.HasPositiveRating,
            "non-positive rating does not open store, save, or reward");
        platform.StoreResult = SettingsOperationResult.Failed();
        Check(!(await model.SubmitRatingAsync(5)).IsSuccess && !state.HasPositiveRating,
            "failed store launch does not mark a positive request");
        platform.StoreResult = SettingsOperationResult.Succeeded();
        Check((await model.SubmitRatingAsync(4)).IsSuccess && state.SavedRating == 4 && state.RewardCalls == 0 && platform.NativeCalls == 0,
            "positive request opens store directly and saves without a default reward");
        int launches = platform.StoreCalls;
        var reopened = new SettingsModel(audio, platform, reviewGateway: state);
        Check(reopened.GetState().HasPositiveRating && !reopened.GetState().CanRequestReview,
            "saved positive request suppresses the Rate Us action in a new model");
        await reopened.SubmitRatingAsync(5);
        Check(platform.StoreCalls == launches, "already saved request never relaunches store");
        state.CanGrantReward = true;
        state.FailReward = true;
        Check(!(await reopened.SubmitRatingAsync(5)).IsSuccess && !state.HasClaimedReward && reopened.GetState().CanRequestReview,
            "reward failure keeps saved request and exposes retry");
        state.FailReward = false;
        await reopened.SubmitRatingAsync(5);
        await reopened.SubmitRatingAsync(5);
        Check(state.HasClaimedReward && state.RewardCalls == 2 && platform.StoreCalls == launches,
            "reward retry succeeds once without repeating the store launch");

        platform = new ReviewPlatform { Pending = new UniTaskCompletionSource<SettingsOperationResult>() };
        state = new ReviewGateway { CanGrantReward = true };
        model = new SettingsModel(audio, platform, reviewGateway: state);
        var first = model.SubmitRatingAsync(5);
        Check(!(await model.SubmitRatingAsync(5)).IsSuccess && platform.StoreCalls == 1,
            "concurrent service requests are rejected");
        platform.Pending.TrySetResult(SettingsOperationResult.Succeeded());
        await first;
        Check(state.RewardCalls == 1, "in-flight request grants once");

        platform = new ReviewPlatform { Pending = new UniTaskCompletionSource<SettingsOperationResult>() };
        state = new ReviewGateway { CanGrantReward = true };
        model = new SettingsModel(audio, platform, reviewGateway: state);
        var view = new ReviewView();
        var presenter = new RateUsPresenter(model, view);
        int submittedRating = 0;
        presenter.PositiveRatingSubmitted += rating => submittedRating = rating;
        presenter.Show();
        presenter.Show();
        view.Select(4);
        view.Submit();
        view.Select(5);
        view.Submit();
        Check(platform.StoreCalls == 1 && view.RenderedRating == 4,
            "presenter blocks double submit and freezes the selected rating");
        platform.Pending.TrySetResult(SettingsOperationResult.Succeeded());
        Check(view.Closes == 1 && submittedRating == 4, "successful request closes once with original rating");

        platform = new ReviewPlatform { Pending = new UniTaskCompletionSource<SettingsOperationResult>() };
        state = new ReviewGateway { CanGrantReward = true };
        model = new SettingsModel(audio, platform, reviewGateway: state);
        view = new ReviewView();
        presenter = new RateUsPresenter(model, view);
        presenter.Show();
        view.Select(5);
        view.Submit();
        presenter.Dispose();
        int updates = view.Updates;
        platform.Pending.TrySetResult(SettingsOperationResult.Succeeded());
        Check(view.Updates == updates && !state.HasPositiveRating && state.RewardCalls == 0,
            "disposed presenter cancels saving/reward and avoids accessing the view");
        Check(!model.GetState().HasPositiveRating && model.GetState().CanRequestReview,
            "cancellation releases the service for retry");
    }
}
