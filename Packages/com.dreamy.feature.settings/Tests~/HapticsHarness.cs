using System;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dreamy.Audio;
using Dreamy.Settings;

public class AudioProxy : DispatchProxy
{
    protected override object Invoke(MethodInfo method, object[] args) =>
        method.Name == "GetVolume" ? 1f : null;
}

internal sealed class HapticsGateway : ISettingsHapticsGateway
{
    public bool IsSupported { get; set; } = true;
    public bool IsEnabled { get; private set; } = true;
    public int Writes { get; private set; }
    public void SetEnabled(bool enabled) { IsEnabled = enabled; Writes++; }
}

internal sealed class View : ISettingsView
{
    public event Action<float> MusicVolumeChanged;
    public event Action<float> SfxVolumeChanged;
    public event Action<bool> HapticsEnabledChanged;
    public event Action GdprRequested;
    public event Action RestorePurchasesRequested;
    public event Action OpenRateUsRequested;
    public event Action CloseRequested;
    public SettingsViewState State;
    public int Renders, InteractivityWrites;
    public void Render(SettingsViewState state) { State = state; Renders++; }
    public void SetPlatformActionsInteractable(bool interactable) { InteractivityWrites++; }
    public void Close() { }
    public void Toggle(bool enabled) => HapticsEnabledChanged?.Invoke(enabled);
    public void RequestRateUs() => OpenRateUsRequested?.Invoke();
    public void RequestConsent() => GdprRequested?.Invoke();
    public void RequestClose() => CloseRequested?.Invoke();
}

internal sealed class PendingConsentGateway : ISettingsPlatformGateway
{
    public bool CanShowGdprConsent => true;
    public bool CanRestorePurchases => false;
    public bool CanOpenStore => false;
    public bool CanRequestReview => false;
    public readonly UniTaskCompletionSource<SettingsOperationResult> Pending = new();
    public UniTask<SettingsOperationResult> ShowGdprConsentAsync(CancellationToken token = default) => Pending.Task;
    public UniTask<SettingsOperationResult> RestorePurchasesAsync(CancellationToken token = default) => UniTask.FromResult(SettingsOperationResult.Succeeded());
    public UniTask<SettingsOperationResult> OpenStoreAsync(CancellationToken token = default) => UniTask.FromResult(SettingsOperationResult.Succeeded());
    public UniTask<SettingsOperationResult> RequestReviewAsync(CancellationToken token = default) => UniTask.FromResult(SettingsOperationResult.Succeeded());
}

internal static class Program
{
    private static void Check(bool condition, string description)
    {
        if (!condition) throw new Exception(description);
        Console.WriteLine("PASS " + description);
    }

    private static void Main()
    {
        BindingHarness.Run();
        PlatformGatewayHarness.Run();
        var audio = DispatchProxy.Create<IAudioService, AudioProxy>();
        ReviewHarness.RunAsync(audio).GetAwaiter().GetResult();
        var unavailable = new SettingsModel(audio);
        unavailable.SetHapticsEnabled(true);
        Check(!unavailable.GetState().CanSetHaptics && !unavailable.GetState().HapticsEnabled,
            "missing gateway disables haptics without throwing");
        var gateway = new HapticsGateway();
        var model = new SettingsModel(audio, hapticsGateway: gateway);
        var view = new View();
        int navigation = 0;
        var presenter = new SettingsPresenter(model, view, () => navigation++);
        presenter.Show();
        presenter.Show();
        view.RequestRateUs();
        Check(navigation == 1, "repeated show attaches navigation once");
        Check(view.State.HapticsEnabled && view.State.CanSetHaptics, "initial preference renders");
        int renders = view.Renders;
        view.Toggle(false);
        Check(gateway.Writes == 1 && !view.State.HapticsEnabled && view.Renders == renders + 1,
            "repeated Show binds once and toggle applies and renders");
        view.Toggle(false);
        Check(gateway.Writes == 1, "unchanged preference does not write again");
        Check(!new SettingsModel(audio, hapticsGateway: gateway).GetState().HapticsEnabled,
            "new model reads host-owned preference");
        gateway.IsSupported = false;
        model.SetHapticsEnabled(true);
        Check(gateway.Writes == 1 && !model.GetState().CanSetHaptics, "unsupported gateway ignores changes");
        gateway.IsSupported = true;
        presenter.Dispose();
        view.RequestRateUs();
        Check(navigation == 1, "dispose detaches navigation callback");
        view.Toggle(true);
        Check(gateway.Writes == 1, "dispose removes toggle listener");
        presenter.Show();
        view.Toggle(true);
        Check(gateway.Writes == 2 && view.State.HapticsEnabled, "reopen rebinds and enables haptics");
        view.RequestClose();
        view.Toggle(false);
        Check(gateway.Writes == 2, "close removes toggle listener");

        var platform = new PendingConsentGateway();
        var pendingView = new View();
        var pendingPresenter = new SettingsPresenter(new SettingsModel(audio, platform), pendingView);
        pendingPresenter.Show();
        pendingView.RequestConsent();
        pendingPresenter.Dispose();
        pendingPresenter.Show();
        int pendingRenders = pendingView.Renders, pendingWrites = pendingView.InteractivityWrites;
        platform.Pending.TrySetResult(SettingsOperationResult.Succeeded());
        Check(pendingView.Renders == pendingRenders && pendingView.InteractivityWrites == pendingWrites,
            "late operation from disposed opening cannot update reopened view");
        pendingPresenter.Dispose();
    }
}
