using System;
using Cysharp.Threading.Tasks;

namespace Dreamy.UI
{
    public interface IPanel
    {
        event Action OnPreShow;

        event Action OnPostShow;

        event Action OnPreHide;

        event Action OnPostHide;

        bool CanBack { get; }

        void SetInteractable(bool interactable);

        UniTask Init();

        UniTask PostInit();

        UniTask Show();

        UniTask Hide();

        UniTask ShowTween();

        UniTask HideTween();
    }
}
