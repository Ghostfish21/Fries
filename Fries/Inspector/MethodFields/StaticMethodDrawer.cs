using System;

namespace Fries.Inspector.MethodFields {
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using System.Linq;
    using System.Reflection;

    [CustomPropertyDrawer(typeof(StaticMethod))]
    public class StaticMethodDrawer : PropertyDrawer {
        private const float namePortion = 0.15f;
        const float ScriptPortion = 0.30f; // 左侧所占比例
        const float Spacing = 4f; // 中间留白

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight; // 单行高度

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var scriptProp = property.FindPropertyRelative("targetScript");
            var nameProp = property.FindPropertyRelative("selectedMethodName");
            var onValueChanged = property.FindPropertyRelative("onValueChanged");

            var nameRect = new Rect(position.x, position.y, position.width * namePortion, position.height);
            var scriptRect = new Rect(nameRect.xMax + Spacing, position.y, (position.width - nameRect.width - Spacing) * ScriptPortion, position.height);
            var popupRect = new Rect(scriptRect.xMax + Spacing, position.y, position.width - scriptRect.width - Spacing - nameRect.width - Spacing, position.height);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.LabelField(nameRect, label);
            EditorGUI.PropertyField(scriptRect, scriptProp, GUIContent.none);

            // 2. 构造下拉选项
            string[] options;
            int current = 0;

            if (!scriptProp.objectReferenceValue) {
                options = new[] { "No script found    "  };
                nameProp.stringValue = null;
            }
            else {
                MonoScript ms = (MonoScript)scriptProp.objectReferenceValue;
                System.Type type = ms.GetClass(); // 取脚本里首个类

                if (type != null) {
                    options = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .Select(m => m.Name).ToArray();

                    if (options.Length == 0)
                        options = new[] { "No static method found    " };
                }
                else options = new[] { "No class found in script    " };
                
                // 计算当前选项索引
                current = System.Array.IndexOf(options, nameProp.stringValue);
                if (current < 0) current = 0;
            }

            // 3. 绘制下拉并写回选择
            int newIndex = EditorGUI.Popup(popupRect, current, options);
            if (options.Length > 0 && newIndex >= 0 && newIndex < options.Length &&
                !options[newIndex].EndsWith("    ")) {
                nameProp.stringValue = options[newIndex];
                ((Action)onValueChanged.getValue())();
            }

            EditorGUI.EndProperty();
        }
    }
#endif
}