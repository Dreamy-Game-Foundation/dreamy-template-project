using System;
using Dreamy.Audio;
using Dreamy.Core;

namespace Dreamy.Settings
{
    public static class SettingsInstaller
    {
        public static ISettingsService Install()
        {
            ISettingsPlatformGateway platformGateway = ServiceLocator.TryGet<ISettingsPlatformGateway>(
                out ISettingsPlatformGateway resolvedGateway)
                ? resolvedGateway
                : null;
            ISettingsHapticsGateway hapticsGateway = ServiceLocator.TryGet<ISettingsHapticsGateway>(
                out ISettingsHapticsGateway resolvedHapticsGateway)
                ? resolvedHapticsGateway
                : null;
            ServiceLocator.TryGet<ISettingsReviewGateway>(out ISettingsReviewGateway reviewGateway);
            ServiceLocator.TryGet<ISettingsLanguageGateway>(out var languageGateway);
            return Install(ServiceLocator.Get<IAudioService>(), platformGateway, hapticsGateway, reviewGateway, languageGateway);
        }

        public static ISettingsService Install(
            IAudioService audioService,
            ISettingsPlatformGateway platformGateway = null,
            ISettingsHapticsGateway hapticsGateway = null,
            ISettingsReviewGateway reviewGateway = null,
            ISettingsLanguageGateway languageGateway = null)
        {
            if (audioService == null)
            {
                throw new ArgumentNullException(nameof(audioService));
            }

            SettingsModel service = new(audioService, platformGateway, hapticsGateway, reviewGateway, languageGateway);
            ServiceLocator.Register<ISettingsService>(service);
            return service;
        }
    }
}
