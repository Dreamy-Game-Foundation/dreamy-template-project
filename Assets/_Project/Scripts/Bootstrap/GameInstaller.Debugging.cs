#if (UNITY_EDITOR || DEVELOPMENT_BUILD) && !DISABLE_DREAMY_DEBUGGER
using Dreamy.Debugger;
using Dreamy.Template.Debugging;
using UnityEngine;

namespace Dreamy.Template
{
    public sealed partial class GameInstaller
    {
        private void InstallDebugging()
        {
            var runtime = DebuggerBootstrap.Initialize();
            if (runtime == null || runtime.Cheats == null) return;
            var options = new TemplateCheats(wallet, (Dreamy.Economy.IResourceBalanceProvider)wallet,
                datasave);
            var lease = runtime.RegisterCheats(options, "template");
            float previousTimeScale = Time.timeScale;
            int previousFps = Application.targetFrameRate;
            System.Action<Dreamy.Economy.ResourceBalanceChanged> changed = _ => runtime.Cheats.NotifyValuesChanged();
            walletBalanceSource.BalanceChanged += changed;
            var source = walletBalanceSource;
            teardownActions.Add(() =>
            {
                source.BalanceChanged -= changed;
                lease.Dispose();
                Time.timeScale = previousTimeScale;
                Application.targetFrameRate = previousFps;
            });
        }
    }
}
#endif
