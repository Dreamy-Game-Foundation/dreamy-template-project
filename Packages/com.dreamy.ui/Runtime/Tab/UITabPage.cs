using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dreamy.UI
{
    public class UITabPage : MonoBehaviour
    {
        protected UITabControl control;
        protected int id;

        public void Register(UITabControl control, int id)
        {
            this.control = control;
            this.id = id;
        }

        public virtual UniTask Init()
        {
            return UniTask.CompletedTask;
        }

        public virtual void Active()
        {
            gameObject.SetActive(true);
        }

        public virtual void Deactivate()
        {
            gameObject.SetActive(false);
        }

        [System.Obsolete("Use Deactivate.")]
        public void Deactive()
        {
            Deactivate();
        }
    }
}
