using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dreamy.UI
{
    public class UIScalable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private Vector3 originScale = Vector3.one;
        [SerializeField] private float pressScaleMultiplier = 0.9f;
        [SerializeField] private float releaseScaleMultiplier = 1.15f;
        [SerializeField] private float duration = 0.1f;

        private Tween currentTween;

        private void Reset()
        {
            originScale = transform.localScale;
        }

        private void OnValidate()
        {
            pressScaleMultiplier = Mathf.Max(0f, pressScaleMultiplier);
            releaseScaleMultiplier = Mathf.Max(0f, releaseScaleMultiplier);
            duration = Mathf.Max(0f, duration);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            StopCurrentTween();
            currentTween = transform
                .DOScale(originScale * pressScaleMultiplier, duration)
                .SetUpdate(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PlayRelease();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PlayRelease();
        }

        private void OnDisable()
        {
            StopCurrentTween();
            transform.localScale = originScale;
        }

        private void OnDestroy()
        {
            StopCurrentTween();
        }

        private void PlayRelease()
        {
            StopCurrentTween();
            currentTween = DOTween.Sequence()
                .SetUpdate(true)
                .Append(transform.DOScale(originScale * releaseScaleMultiplier, duration))
                .Append(transform.DOScale(originScale, duration));
        }

        private void StopCurrentTween()
        {
            if (currentTween != null)
            {
                currentTween.Kill();
            }
            currentTween = null;
        }
    }
}
