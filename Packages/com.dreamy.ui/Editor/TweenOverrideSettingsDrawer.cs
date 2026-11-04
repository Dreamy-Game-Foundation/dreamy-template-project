using UnityEditor;
using UnityEngine;

namespace Dreamy.UI.Editor
{
    [CustomPropertyDrawer(typeof(TweenOverrideSettings))]
    public sealed class TweenOverrideSettingsDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            float y = position.y;
            DrawOverride(ref y, position.x, position.width, property, "overrideEaseIn", "easeIn", "Show Ease");
            DrawOverride(ref y, position.x, position.width, property, "overrideEaseOut", "easeOut", "Hide Ease");
            DrawOverride(ref y, position.x, position.width, property, "overrideDurationIn", "durationIn", "Show Duration");
            DrawOverride(ref y, position.x, position.width, property, "overrideDurationOut", "durationOut", "Hide Duration");
            DrawOverride(ref y, position.x, position.width, property, "overrideDelayIn", "delayIn", "Show Delay");
            DrawOverride(ref y, position.x, position.width, property, "overrideDelayOut", "delayOut", "Hide Delay");
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0f;
            height += GetOverrideHeight(property, "overrideEaseIn", "easeIn");
            height += GetOverrideHeight(property, "overrideEaseOut", "easeOut");
            height += GetOverrideHeight(property, "overrideDurationIn", "durationIn");
            height += GetOverrideHeight(property, "overrideDurationOut", "durationOut");
            height += GetOverrideHeight(property, "overrideDelayIn", "delayIn");
            height += GetOverrideHeight(property, "overrideDelayOut", "delayOut");
            return height;
        }

        private static void DrawOverride(
            ref float y,
            float x,
            float width,
            SerializedProperty root,
            string toggleName,
            string valueName,
            string label)
        {
            SerializedProperty toggle = root.FindPropertyRelative(toggleName);
            SerializedProperty value = root.FindPropertyRelative(valueName);
            Rect toggleRect = new Rect(x, y, width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(toggleRect, toggle, new GUIContent(label));
            y += EditorGUIUtility.singleLineHeight + Spacing;
            if (!toggle.boolValue) return;

            Rect valueRect = new Rect(x + 15f, y, width - 15f,
                EditorGUI.GetPropertyHeight(value, true));
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(valueRect, value, GUIContent.none, true);
            EditorGUI.indentLevel--;
            y += valueRect.height + Spacing;
        }

        private static float GetOverrideHeight(SerializedProperty root, string toggleName, string valueName)
        {
            SerializedProperty toggle = root.FindPropertyRelative(toggleName);
            float height = EditorGUIUtility.singleLineHeight + Spacing;
            if (toggle.boolValue)
            {
                height += EditorGUI.GetPropertyHeight(root.FindPropertyRelative(valueName), true) + Spacing;
            }

            return height;
        }
    }
}
