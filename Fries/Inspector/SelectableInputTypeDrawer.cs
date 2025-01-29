#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Fries.Inspector {
    [CustomPropertyDrawer(typeof(SelectableInputType))]
    public class SelectableInputTypeDrawer : PropertyDrawer {
        private const float PopupWidth = 120f;
        private const float Spacing = 2f;
        private List<SerializedProperty> inputProps;

        private void Init(SerializedProperty serializedProperty) {
            inputProps = new List<SerializedProperty>();
            SerializedProperty inputPropsSP = serializedProperty.FindPropertyRelative("inputProperties");
            var target = serializedProperty.FindPropertyRelative("target").objectReferenceValue as MonoBehaviour;
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
                SerializedProperty selectedProperty = inputProps[selectedIndex.intValue];
                EditorGUI.PropertyField(valueRect, selectedProperty, GUIContent.none);
            }

            EditorGUI.EndProperty();
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
    }
}
#endif