#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Collections;

namespace Fries.Inspector {
    [CustomPropertyDrawer(typeof(RefUnionDrawer))]
    public class RefUnionDrawer : PropertyDrawer {
        private const float PopupWidth = 120f;
        private const float Spacing = 2f;
        private List<SerializedProperty> inputProps;
        private MonoBehaviour target;

        private void Init(SerializedProperty serializedProperty) {
            target = serializedProperty.FindPropertyRelative("target").objectReferenceValue as MonoBehaviour;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Init(property);

            EditorGUI.BeginProperty(position, label, property);

            // 获取属性元素
            SerializedProperty inputTypes = property.FindPropertyRelative("inputTypes");
            SerializedProperty inputObjRefs = property.FindPropertyRelative("inputObjectRefs");
            SerializedProperty selectedIndex = property.FindPropertyRelative("selectedIndex");
            
            // 验证数据有效性
            if (inputTypes.arraySize != inputObjRefs.arraySize) {
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

            DrawTypePopup(popupRect, selectedIndex, inputTypes);

            int maxIndex = inputObjRefs.arraySize;
            // 绘制当前选中的属性
            if (selectedIndex.intValue >= 0 && selectedIndex.intValue < maxIndex) {
                EditorGUI.BeginChangeCheck();
                SerializedProperty selectedProperty = property.serializedObject.FindProperty(
                    AnInspector.getCachedPropertyPath(target, inputObjRefs.GetArrayElementAtIndex(selectedIndex.intValue).getValue()));
                EditorGUI.PropertyField(valueRect, selectedProperty, GUIContent.none);
                if (EditorGUI.EndChangeCheck()) {
                    object value = selectedProperty.getValue(); 
                    setValue(selectedProperty.propertyPath, value);
                }
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
                selectedIndex.intValue = newIndex;
                selectedIndex.serializedObject.ApplyModifiedProperties();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
        
        private void setValue(string propertyPath, object setTo) {
            Type parentType = target.GetType();
            string[] comps = propertyPath.Split(".");
            object value = target;
            FieldInfo fi = null;

            foreach (var comp in comps) {
                if (comp == "Array") continue;
                if (comp.Contains("data[")) {
                    int i = int.Parse(comp.Replace("data[", "").Replace("]", ""));
                    IList list = value as IList;
                    value = list[i]; 
                    parentType = value.GetType();
                    continue;
                }
                fi = parentType.GetField(comp);
                fi.SetValue(value, setTo);
                value = fi.GetValue(value);
                parentType = value.GetType();
            }
        }
    }
}
#endif