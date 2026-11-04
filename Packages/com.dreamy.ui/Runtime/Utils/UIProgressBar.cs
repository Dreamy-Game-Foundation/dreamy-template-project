using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamy.UI
{
    [DisallowMultipleComponent]
    public sealed class UIProgressBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private TMP_Text valueLabel;
        [SerializeField, Range(0f, 1f)] private float value;
        [SerializeField, Min(0f)] private float animationDuration = 0.25f;
        [SerializeField] private string valueFormat = "{0:0%}";

        private Tween valueTween;

        public float Value => value;

        private void Reset()
        {
            fillImage = GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.type = Image.Type.Filled;
            }
        }

        private void Awake()
        {
            Render(value);
        }

        private void OnValidate()
        {
            value = Mathf.Clamp01(value);
            animationDuration = Mathf.Max(0f, animationDuration);
            Render(value);
        }

        private void OnDisable()
        {
            valueTween?.Kill();
            valueTween = null;
        }

        public void SetValue(float normalizedValue)
        {
            valueTween?.Kill();
            valueTween = null;
            Render(normalizedValue);
        }

        public Tween AnimateTo(float normalizedValue)
        {
            valueTween?.Kill();
            float targetValue = Mathf.Clamp01(normalizedValue);
            valueTween = DOTween.To(
                    () => value,
                    Render,
                    targetValue,
                    animationDuration)
                .SetTarget(this)
                .OnKill(() => valueTween = null);
            return valueTween;
        }

        private void Render(float normalizedValue)
        {
            value = Mathf.Clamp01(normalizedValue);
            if (fillImage != null)
            {
                fillImage.fillAmount = value;
            }

            if (valueLabel != null)
            {
                valueLabel.text = string.Format(valueFormat, value);
            }
        }
    }
}
