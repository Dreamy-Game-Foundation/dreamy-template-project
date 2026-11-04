using System.Threading;
using Cysharp.Threading.Tasks;

namespace Dreamy.UI
{
    public interface ITween
    {
        bool IsAutoRun { get; }
        bool IsEnabled { get; }

        UniTask Init();
        UniTask Show(CancellationToken token);
        UniTask Hide(CancellationToken token);
        void Kill();
    }
}
