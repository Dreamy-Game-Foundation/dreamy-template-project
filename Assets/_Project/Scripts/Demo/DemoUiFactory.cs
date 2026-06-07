using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Dreamy.Template.Demo
{
    internal static class DemoUiFactory
    {
        private static readonly Color AccentColor =
            new(0.16f, 0.76f, 0.62f, 1f);

        public static T CreatePanel<T>(
            Transform parent,
            string name,
            Color color)
            where T : MonoBehaviour
        {
            GameObject panel = CreateUiObject(name, parent);
            Stretch(panel.GetComponent<RectTransform>());
            panel.AddComponent<Image>().color = color;
            return panel.AddComponent<T>();
        }

        public static Text CreateText(
            Transform parent,
            string name,
            string value,
            int fontSize,
            TextAnchor alignment)
        {
            GameObject textObject = CreateUiObject(name, parent);
            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button CreateButton(
            Transform parent,
            string name,
            string label,
            UnityAction onClick)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            Image image = buttonObject.AddComponent<Image>();
            image.color = AccentColor;
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            Text text = CreateText(
                buttonObject.transform,
                "Label",
                label,
                42,
                TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        public static GameObject CreateTargetPrefab()
        {
            GameObject target = new("Tap Target");
            RectTransform rect = target.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150f, 150f);
            Image image = target.AddComponent<Image>();
            image.color = new Color(0.96f, 0.35f, 0.24f, 1f);
            Button button = target.AddComponent<Button>();
            button.targetGraphic = image;
            target.AddComponent<TapTarget>();
            target.SetActive(false);
            return target;
        }

        public static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static GameObject CreateUiObject(
            string name,
            Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void Stretch(RectTransform rect)
        {
            SetRect(
                rect,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
        }
    }
}
