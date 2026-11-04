using System;
using System.Threading;
using Dreamy.Feature.Settings.Integration;
using Dreamy.Settings;

internal static class PlatformGatewayHarness
{
    private static void Check(bool value, string description)
    { if (!value) throw new Exception(description); Console.WriteLine("PASS " + description); }
    public static void Run()
    {
        var gateway = new SettingsPlatformGateway();
        Check(!gateway.CanShowGdprConsent && !gateway.CanRestorePurchases &&
            !gateway.CanOpenStore && !gateway.CanRequestReview,
            "placeholder disables platform actions until SDK integration");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        foreach (var token in new[] { CancellationToken.None, cancellation.Token })
        {
            Check(gateway.ShowGdprConsentAsync(token).GetAwaiter().GetResult().Status == SettingsOperationStatus.Unavailable,
                "placeholder consent returns unavailable");
            Check(gateway.RestorePurchasesAsync(token).GetAwaiter().GetResult().Status == SettingsOperationStatus.Unavailable,
                "placeholder restore returns unavailable");
            Check(gateway.OpenStoreAsync(token).GetAwaiter().GetResult().Status == SettingsOperationStatus.Unavailable,
                "placeholder store returns unavailable");
            Check(gateway.RequestReviewAsync(token).GetAwaiter().GetResult().Status == SettingsOperationStatus.Unavailable,
                "placeholder review returns unavailable");
        }
    }
}
