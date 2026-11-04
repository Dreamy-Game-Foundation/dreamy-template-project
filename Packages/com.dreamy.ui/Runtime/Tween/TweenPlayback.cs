using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Dreamy.UI
{
    internal sealed class TweenPlayback
    {
        private Tween currentTween;
        private UniTaskCompletionSource currentCompletion;

        public UniTask Play(
            Tween tween,
            Action onComplete,
            UnityEngine.Object owner)
        {
            if (tween == null)
            {
                return UniTask.CompletedTask;
            }

            Kill();
            currentTween = tween;

            UniTaskCompletionSource completionSource = new UniTaskCompletionSource();
            currentCompletion = completionSource;
            tween.OnComplete(() =>
            {
                try
                {
                    if (owner != null)
                    {
                        onComplete?.Invoke();
                    }
                }
                finally
                {
                    if (currentTween == tween)
                    {
                        currentTween = null;
                        currentCompletion = null;
                    }

                    completionSource.TrySetResult();
                }
            });
            tween.OnKill(() =>
            {
                if (currentTween == tween)
                {
                    currentTween = null;
                    currentCompletion = null;
                }

                completionSource.TrySetResult();
            });
            return completionSource.Task;
        }

        public void Kill()
        {
            Tween tween = currentTween;
            UniTaskCompletionSource completion = currentCompletion;
            currentTween = null;
            currentCompletion = null;
            try
            {
                // DOTween can defer removal until its next update. Stop writes immediately.
                tween?.Pause();
                tween?.Kill();
            }
            finally
            {
                // Do not depend on a DOTween update to settle a delayed/killed playback.
                completion?.TrySetResult();
            }
        }
    }
}
