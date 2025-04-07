using System;
using UnityEngine;
# if UNITY_EDITOR
using UnityEditor;
# endif

namespace Fries.Inspector.ValueWrapper {
    [Serializable]
    public class FloatWrapper {
        public string label;
        public float value;
    }

    # if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FloatWrapper))]
    public class FloatWrapperDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // 获取 FloatWrapper 中的 label 和 value 字段
            SerializedProperty labelProperty = property.FindPropertyRelative("label");
            SerializedProperty valueProperty = property.FindPropertyRelative("value");

            // 使用 label 字段的值作为 value 字段的显示名称
            string displayName = labelProperty.stringValue;

            // 绘制 value 字段，并使用自定义的显示名称
            EditorGUI.PropertyField(position, valueProperty, new GUIContent(displayName));
        }
    }
    # endif
}