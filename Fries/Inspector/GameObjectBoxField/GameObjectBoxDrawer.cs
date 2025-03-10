# if UNITY_EDITOR

using Fries;
using UnityEditor;

namespace Fries.Inspector.GameObjectBoxField {
    using UnityEngine;
    using UnityEditor;
    using System;
    using System.Collections.Generic;

    [CustomPropertyDrawer(typeof(GameObjectBox), true)]
    public class GameObjectBoxDrawer : PropertyDrawer {
        // 以 controlID 为键保存对应的 PickerData
        private static readonly Dictionary<int, PickerData> pickerMap = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // 开始绘制属性
            EditorGUI.BeginProperty(position, label, property);
            
            // 获取当前属性的字段类型（应为 GameObjectBox<T>）
            Type fieldType = fieldInfo.FieldType;
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>)) 
                fieldType = fieldType.GetGenericArguments()[0];

            // 默认 T 为 UnityEngine.Object
            Type elementType = typeof(UnityEngine.Object);
            if (fieldType.IsGenericType) {
                // 取泛型参数，即 T 的类型
                Type[] args = fieldType.GetGenericArguments();
                if (args.Length > 0) {
                    elementType = args[0];
                    // 如果 element type 不是 Unity.Object 或者 LocalizedString 就打印warning并返回
                    if (!typeof(UnityEngine.Object).IsAssignableFrom(elementType) && elementType.Name != "LocalizedString" && elementType.Name != "StringSso") {
                        Debug.LogWarning($"Type {elementType} is not UnityEngine.Object or LocalizedString");
                        return;
                    }
                }
            }

            // 在按钮下方显示当前选中对象的信息（假设 GameObjectBox 中的字段名为 "value"）
            SerializedProperty valueProp = property.FindPropertyRelative("unityObj");
            string objectName = (valueProp != null && valueProp.objectReferenceValue != null)
                ? valueProp.objectReferenceValue.name
                : "Null";
            if (!typeof(UnityEngine.Object).IsAssignableFrom(elementType)) {
                valueProp = property.FindPropertyRelative("sysObj");
                objectName = valueProp.managedReferenceValue != null
                    ? valueProp.managedReferenceValue.ToString() : "Null";
            }
            
            // 绘制一个按钮，按钮高度为单行高度
            Rect buttonRect = new Rect(position.x, position.y, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
            string text = "x";
            if (objectName != "Null") text = "■";
            if (GUI.Button(buttonRect, text)) {
                // 生成一个 controlID，用于标识本次选择器
                int controlID = GUIUtility.GetControlID(FocusType.Passive);
                // 将当前属性数据存入字典中，以便在选择器关闭时能找到对应的属性
                PickerData data = new PickerData {
                    serializedObject = property.serializedObject,
                    propertyPath = property.propertyPath,
                    elementType = elementType
                };
                pickerMap[controlID] = data;

                if (typeof(UnityEngine.Object).IsAssignableFrom(elementType)) {
                    // 构造搜索过滤器，比如 "t:Texture" 将只显示 Texture 类型的对象
                    string filter = "t:" + elementType.Name;
                    // 打开对象选择器（注意这里使用的是 UnityEngine.Object 类型，但通过过滤器只显示指定类型的对象）
                    EditorGUIUtility.ShowObjectPicker<UnityEngine.Object>(null, false, filter, controlID);
                }
                else if (elementType.Name == "LocalizedString") {
                    // 打开自定义的 LocalizedString 选择器窗口
                    LocalizedStringPickerWindow window = EditorWindow.GetWindow<LocalizedStringPickerWindow>(true, "Select Localized String", true);
                    window.Init(controlID, property.FindPropertyRelative("sysObj"), data);
                }
                else if (elementType.Name == "StringSso") {
                    // 打开自定义的 LocalizedString 选择器窗口
                    StringInputWindow window = EditorWindow.GetWindow<StringInputWindow>(true, "Input string", true);
                    window.Init(controlID, property.FindPropertyRelative("sysObj"), data);
                }
            }

            // 检测鼠标是否悬停在 labelRect 内，如果是，则自定义绘制 tooltip
            if (buttonRect.Contains(Event.current.mousePosition)) {
                // 计算 tooltip 的尺寸
                Vector2 size = GUI.skin.box.CalcSize(new GUIContent(objectName));
                // 设定 tooltip 绘制区域：鼠标当前位置上方显示
                Rect tooltipRect = new Rect(Event.current.mousePosition.x,
                    Event.current.mousePosition.y - size.y,
                    size.x + 5f, size.y + 5f);
                // 绘制 tooltip 框
                GUI.Box(tooltipRect, objectName);
                // 强制立即重绘，确保 tooltip 能及时显示
                try {
                    EditorWindow.focusedWindow.Repaint();
                }
                catch (Exception _) {
                    // ignored
                }
            }

            // 当对象选择器关闭时，处理选择结果
            if (Event.current.commandName == "ObjectSelectorClosed") {
                // 获取关闭的对象选择器的 controlID
                int pickerControlID = EditorGUIUtility.GetObjectPickerControlID();
                if (pickerMap.ContainsKey(pickerControlID)) {
                    PickerData data = pickerMap[pickerControlID];
                    UnityEngine.Object pickedObj = EditorGUIUtility.GetObjectPickerObject();
                    if (pickedObj != null) {
                        // 根据存储的属性路径重新获取属性，并将选中的对象赋值到 "value" 字段
                        SerializedProperty prop = data.serializedObject.FindProperty(data.propertyPath);
                        SerializedProperty valProp = prop.FindPropertyRelative("unityObj");
                        if (valProp != null) {
                            valProp.objectReferenceValue = pickedObj;
                            data.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    else {
                        // 根据存储的属性路径重新获取属性，并将选中的对象赋值到 "value" 字段
                        SerializedProperty prop = data.serializedObject.FindProperty(data.propertyPath);
                        SerializedProperty valProp = prop.FindPropertyRelative("unityObj");
                        if (valProp != null) {
                            valProp.objectReferenceValue = null;
                            data.serializedObject.ApplyModifiedProperties();
                        }
                    }

                    // 清理记录
                    pickerMap.Remove(pickerControlID);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            // 返回两行高度（按钮一行 + 显示当前选中对象信息一行）
            return EditorGUIUtility.singleLineHeight * 2 + 2;
        }
    }
}

# endif