#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace Fries.Inspector {
    [CustomPropertyDrawer(typeof(SelectableInputType))]
    public class SelectableInputTypeDrawer : PropertyDrawer {
        private const float PopupWidth = 120f;
        private const float Spacing = 2f;
        private List<SerializedProperty> inputProps;
        private MonoBehaviour target;

        private void Init(SerializedProperty serializedProperty) {
            inputProps = new List<SerializedProperty>();
            SerializedProperty inputPropsSP = serializedProperty.FindPropertyRelative("inputProperties");
            target = serializedProperty.FindPropertyRelative("target").objectReferenceValue as MonoBehaviour;
            for (int i = 0; i < inputPropsSP.arraySize; i++) {
                string propertyName = inputPropsSP.GetArrayElementAtIndex(i).stringValue;
                inputProps.Add(getSerializedProperty(target, propertyName));
            }
        }

        private SerializedProperty getSerializedProperty(MonoBehaviour target, string propertyName) {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty serializedProperty = serializedObject.FindProperty(propertyName);
            return serializedProperty;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (inputProps == null) Init(property);

            EditorGUI.BeginProperty(position, label, property);

            // 获取属性元素
            SerializedProperty inputTypes = property.FindPropertyRelative("inputTypes");
            SerializedProperty inputProperties = property.FindPropertyRelative("inputProperties");
            SerializedProperty selectedIndex = property.FindPropertyRelative("selectedIndex");

            // 验证数据有效性
            if (inputTypes.arraySize != inputProperties.arraySize) {
                Debug.LogError("InputTypes and InputProperties size mismatch!");
                return;
            }

            // 计算布局
            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            Rect valueRect = new Rect(
                labelRect.xMax + Spacing, 
                position.y, 
                PopupWidth, 
                EditorGUIUtility.singleLineHeight
            );
            
            Rect popupRect = new Rect(
                valueRect.xMax + Spacing,
                position.y,
                EditorGUIUtility.currentViewWidth - valueRect.xMax - Spacing,
                EditorGUIUtility.singleLineHeight
            );

            // 绘制标签
            EditorGUI.LabelField(labelRect, label);

            // 绘制类型下拉菜单
            DrawTypePopup(popupRect, selectedIndex, inputTypes);

            // 绘制当前选中的属性
            if (selectedIndex.intValue >= 0 && selectedIndex.intValue < inputProperties.arraySize) {
                EditorGUI.BeginChangeCheck();
                SerializedProperty selectedProperty = inputProps[selectedIndex.intValue];
                EditorGUI.PropertyField(valueRect, selectedProperty, GUIContent.none);
                if (EditorGUI.EndChangeCheck()) {
                    object value = GetValue(selectedProperty);
                    string propertyName = inputProperties.GetArrayElementAtIndex(selectedIndex.intValue).stringValue;
                    setValue(propertyName, value);
                }
            }

            EditorGUI.EndProperty();
        }

        private void setValue(string propertyName, object value) {
            FieldInfo fieldInfo = target.GetType().GetField(propertyName);
            fieldInfo.SetValue(target, value);
        }

        private void DrawTypePopup(Rect rect, SerializedProperty selectedIndex, SerializedProperty inputTypes) {
            // 生成选项列表
            string[] options = new string[inputTypes.arraySize];
            for (int i = 0; i < inputTypes.arraySize; i++) {
                options[i] = inputTypes.GetArrayElementAtIndex(i).stringValue;
            }

            // 显示下拉菜单
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(rect, selectedIndex.intValue, options);
            if (EditorGUI.EndChangeCheck()) {
                selectedIndex.intValue = Mathf.Clamp(newIndex, 0, inputTypes.arraySize - 1);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        private object GetValue(SerializedProperty property) {
            switch (property.propertyType) {
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Color:
                    return property.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return property.intValue;
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex;
                case SerializedPropertyType.Vector2:
                    return property.vector2Value;
                case SerializedPropertyType.Vector3:
                    return property.vector3Value;
                case SerializedPropertyType.Vector4:
                    return property.vector4Value;
                case SerializedPropertyType.Rect:
                    return property.rectValue;
                case SerializedPropertyType.ArraySize:
                    return property.arraySize;
                case SerializedPropertyType.Character:
                    return (char)property.intValue;
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return property.boundsValue;
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue;

                // 如果有更多类型，可以继续添加

                default:
                    Debug.LogWarning($"未处理的 SerializedProperty 类型: {property.propertyType}");
                    return null;
            }
        }
    }
}
#endif