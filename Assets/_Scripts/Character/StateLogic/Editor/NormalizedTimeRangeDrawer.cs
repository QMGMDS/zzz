using UnityEditor;
using UnityEngine;

namespace SPCharacterController.Editor
{
    /// <summary>
    /// 归一化时间区间绘制器 - 使用双端滑块限制配置范围和端点顺序。
    /// </summary>
    [CustomPropertyDrawer(typeof(NormalizedTimeRange))]
    public sealed class NormalizedTimeRangeDrawer : PropertyDrawer
    {
        private const float FieldWidth = 48f;
        private const float Spacing = 4f;

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty startProperty = property.FindPropertyRelative("_start");
            SerializedProperty endProperty = property.FindPropertyRelative("_end");
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);
            float sliderWidth = Mathf.Max(0f, contentPosition.width - FieldWidth * 2f - Spacing * 2f);

            Rect startPosition = new Rect(contentPosition.x, contentPosition.y, FieldWidth, contentPosition.height);
            Rect sliderPosition = new Rect(startPosition.xMax + Spacing, contentPosition.y, sliderWidth, contentPosition.height);
            Rect endPosition = new Rect(sliderPosition.xMax + Spacing, contentPosition.y, FieldWidth, contentPosition.height);

            float originalStart = startProperty.floatValue;
            float originalEnd = endProperty.floatValue;
            float start = float.IsNaN(originalStart) ? 0f : Mathf.Clamp01(originalStart);
            float end = float.IsNaN(originalEnd) ? start : Mathf.Clamp(originalEnd, start, 1f);

            EditorGUI.BeginChangeCheck();
            start = EditorGUI.FloatField(startPosition, start);
            end = EditorGUI.FloatField(endPosition, end);
            start = float.IsNaN(start) ? 0f : Mathf.Clamp01(start);
            end = float.IsNaN(end) ? start : Mathf.Clamp(end, start, 1f);
            EditorGUI.MinMaxSlider(sliderPosition, ref start, ref end, 0f, 1f);
            if (EditorGUI.EndChangeCheck() || start != originalStart || end != originalEnd)
            {
                startProperty.floatValue = start;
                endProperty.floatValue = end;
            }

            EditorGUI.EndProperty();
        }
    }
}
