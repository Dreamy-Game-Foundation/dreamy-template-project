using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
namespace Dreamy.Settings
{
    public readonly struct SettingsLanguageOption
    {
        public string Code { get; }
        public string DisplayName { get; }
        public SettingsLanguageOption(string code, string displayName) { Code = code; DisplayName = displayName; }
    }
    public readonly struct SettingsLanguageState
    {
        public IReadOnlyList<SettingsLanguageOption> Options { get; }
        public string CurrentCode { get; }
        public bool IsAvailable => Options != null && Options.Count > 0;
        public bool IsBusy { get; }
        public string ErrorKey { get; }
        public SettingsLanguageState(IReadOnlyList<SettingsLanguageOption> options, string code, bool busy, string errorKey)
        { Options = options; CurrentCode = code; IsBusy = busy; ErrorKey = errorKey; }
    }
    public interface ISettingsLanguageGateway
    {
        IReadOnlyList<SettingsLanguageOption> Options { get; }
        string CurrentCode { get; }
        event Action LanguageChanged;
        UniTask<bool> SetLanguageAsync(string code, CancellationToken token = default);
    }
    // Optional capability: existing ISettingsService consumers and fakes remain source-compatible.
    public interface ISettingsLanguageService
    {
        SettingsLanguageState GetLanguageState();
        event Action LanguageChanged;
        UniTask SetLanguageAsync(string code, CancellationToken token = default);
    }
    public interface ISettingsLanguageView
    {
        event Action<string> LanguageRequested;
        void RenderLanguage(SettingsLanguageState state);
    }
}
