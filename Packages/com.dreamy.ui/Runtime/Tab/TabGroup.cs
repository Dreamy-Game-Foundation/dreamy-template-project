using System;
using Cysharp.Threading.Tasks;

namespace Dreamy.UI
{
    [Serializable]
    public sealed class TabGroup
    {
        public UITabButton Button;
        public UITabPage Page;

        public void Register(UITabControl control, int id)
        {
            Button?.Register(control, id);
            Page?.Register(control, id);
        }

        public UniTask Init()
        {
            Button?.Init();
            return Page != null ? Page.Init() : UniTask.CompletedTask;
        }

        public void Show()
        {
            Button?.Active();
            Page?.Active();
        }

        public void Hide()
        {
            Button?.Deactive();
            Page?.Deactivate();
        }
    }
}
