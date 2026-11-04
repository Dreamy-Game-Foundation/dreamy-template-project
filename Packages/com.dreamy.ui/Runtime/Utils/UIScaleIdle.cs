using DG.Tweening;
using UnityEngine;

namespace Dreamy.UI
{
    public class UIScaleIdle : MonoBehaviour
    {
        [SerializeField] private Vector3 originScale = Vector3.one;
        [SerializeField] private float idleScaleMultiplier = 1.03f;
        [SerializeField, Min(0f)] private float idleDuration = 0.8f;
        [SerializeField, Min(0f)] private float idleDelay = 0.2f;
        [SerializeField] private Ease idleEase = Ease.InOutSine;

        private Tween idleTween;

        private void Reset()
        {
            originScale = transform.localScale;
        }

        private void OnValidate()
        {
            idleScaleMultiplier = Mathf.Max(0f, idleScaleMultiplier);
            idleDuration = Mathf.Max(0f, idleDuration);
            idleDelay = Mathf.Max(0f, idleDelay);
        }

        private void OnEnable()
        {
            PlayIdle();
        }

        private void OnDisable()
        {
            StopIdleTween();
            transform.localScale = originScale;
        }

        private void OnDestroy()
        {
            StopIdleTween();
        }

        private void PlayIdle()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            StopIdleTween();
            
            // Ensure we start from the origin scale
            transform.localScale = originScale;

            idleTween = transform
                .DOScale(originScale * idleScaleMultiplier, idleDuration)
                .SetDelay(idleDelay)
                .SetEase(idleEase)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        private void StopIdleTween()
        {
            if (idleTween != null)
            {
                idleTween.Kill();
                idleTween = null;
            }
        }
    }
}
