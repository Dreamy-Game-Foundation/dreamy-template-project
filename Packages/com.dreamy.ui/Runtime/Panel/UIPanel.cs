using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dreamy.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPanel : MonoBehaviour, IPanel
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected UITweenPlayer tweenPlayer;

        private CancellationTokenSource tokenSource;
        private PanelState state = PanelState.Hidden;
        private int operationVersion;
        private PanelPresenterHost presenterHost;

        public abstract bool CanBack { get; }
        public virtual UILayer Layer => UILayer.Screen;
        public virtual bool CanCache => false;
        public virtual bool ShowOnStart => false;

        public event Action OnPreShow;
        public event Action OnPostShow;
        public event Action OnPreHide;
        public event Action OnPostHide;

        protected virtual void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            tweenPlayer = GetComponent<UITweenPlayer>();
        }

        public virtual UniTask Init()
        {
            PanelManager.Instance.Register(this);
            return UniTask.CompletedTask;
        }

        public virtual async UniTask PostInit()
        {
            gameObject.SetActive(false);

            UITweenPlayer transition = ResolveTweenPlayer();
            if (transition != null)
            {
                await transition.Init();
            }
        }

        public async UniTask Show()
        {
            if (state == PanelState.Showing || state == PanelState.Shown)
            {
                return;
            }

            state = PanelState.Showing;
            int version = ResetToken();
            try
            {
                OnPreShow?.Invoke();
                gameObject.SetActive(true);
                var factory = PanelManager.Instance.PresenterFactory;
                if (factory != null)
                {
                    presenterHost ??= new PanelPresenterHost(factory, this);
                    presenterHost.Show();
                }
                PanelManager.Instance.MarkShown(this);
                await ShowTween();
                if (version != operationVersion)
                {
                    return;
                }

                state = PanelState.Shown;
                OnPostShow?.Invoke();
            }
            catch (OperationCanceledException)
            {
                if (version == operationVersion)
                {
                    ReleasePresenter();
                    state = PanelState.Hidden;
                    if (PanelManager.HasInstance)
                    {
                        PanelManager.Instance.MarkHidden(this);
                    }
                }
                // A newer panel operation owns the final state otherwise.
            }
            catch
            {
                if (version == operationVersion)
                {
                    ReleasePresenter();
                    state = PanelState.Hidden;
                    PanelManager.Instance.MarkHidden(this);
                    gameObject.SetActive(false);
                }

                throw;
            }
        }

        public async UniTask Hide()
        {
            if (state == PanelState.Hiding || state == PanelState.Hidden)
            {
                return;
            }

            state = PanelState.Hiding;
            int version = ResetToken();
            OnPreHide?.Invoke();
            try
            {
                UniTask restorePreviousTask = PanelManager.HasInstance
                    ? PanelManager.Instance.RestorePreviousPanelVisual(this)
                    : UniTask.CompletedTask;
                await UniTask.WhenAll(HideTween(), restorePreviousTask);
                if (version != operationVersion)
                {
                    return;
                }

                PanelManager.Instance.MarkHidden(this);
                PanelManager.Instance.CompletePanelHide(this);
                state = PanelState.Hidden;

                ReleasePresenter();
                OnPostHide?.Invoke();

                if (CanCache)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            catch (OperationCanceledException)
            {
                if (PanelManager.HasInstance)
                {
                    if (version == operationVersion)
                    {
                        PanelManager.Instance.MarkHidden(this);
                        PanelManager.Instance.CompletePanelHide(this);
                        state = PanelState.Hidden;
                        ReleasePresenter();
                    }
                    else
                    {
                        PanelManager.Instance.CancelPanelHide(this);
                    }
                }
            }
            catch
            {
                if (version == operationVersion)
                {
                    state = PanelState.Shown;
                    SetInteractable(true);
                }

                if (PanelManager.HasInstance)
                {
                    PanelManager.Instance.CancelPanelHide(this);
                }

                throw;
            }
        }

        public UniTask ShowTween()
        {
            UITweenPlayer transition = ResolveTweenPlayer();
            return transition != null
                ? transition.ShowTween(tokenSource?.Token ?? CancellationToken.None)
                : UniTask.CompletedTask;
        }

        public UniTask HideTween()
        {
            UITweenPlayer transition = ResolveTweenPlayer();
            return transition != null
                ? transition.HideTween(tokenSource?.Token ?? CancellationToken.None)
                : UniTask.CompletedTask;
        }

        public void SetInteractable(bool interactable)
        {
            if (canvasGroup != null)
            {
                canvasGroup.interactable = interactable;
                canvasGroup.blocksRaycasts = interactable;
            }
        }

        internal void ReleasePresenter()
        {
            var host = presenterHost;
            presenterHost = null;
            host?.Dispose();
        }

        protected virtual void OnDestroy()
        {
            ReleasePresenter();
            tokenSource?.Cancel();
            tokenSource?.Dispose();
            ResolveTweenPlayer()?.Kill();

            if (PanelManager.HasInstance)
            {
                PanelManager.Instance.Unregister(this);
            }
        }

        protected virtual void OnDisable()
        {
            ReleasePresenter();
            if (state == PanelState.Showing || state == PanelState.Hiding)
            {
                tokenSource?.Cancel();
            }
            else if (state == PanelState.Shown)
            {
                state = PanelState.Hidden;
                if (PanelManager.HasInstance) PanelManager.Instance.MarkHidden(this);
            }
        }

        private int ResetToken()
        {
            tokenSource?.Cancel();
            tokenSource?.Dispose();
            tokenSource = new CancellationTokenSource();
            operationVersion++;
            return operationVersion;
        }

        private UITweenPlayer ResolveTweenPlayer()
        {
            if (tweenPlayer == null)
            {
                tweenPlayer = GetComponent<UITweenPlayer>();
            }

            return tweenPlayer;
        }

        private enum PanelState
        {
            Hidden,
            Showing,
            Shown,
            Hiding
        }
    }
}
