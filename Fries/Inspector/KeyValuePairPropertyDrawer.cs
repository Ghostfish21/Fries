using System;
using System.IO;
using System.Reflection;

namespace Fries.Inspector {
    using UnityEngine;
    
    # if UNITY_EDITOR
    using UnityEditor;
    [CustomPropertyDrawer(typeof(KiiValuePair), true)]
    public class KeyValuePairPropertyDrawer : PropertyDrawer {
        public Type getType(SerializedProperty property) {
            // 获取序列化对象的目标对象
            object target = property.serializedObject.targetObject;
            Type targetType = target.GetType();

            // 分割 propertyPath，处理可能的嵌套情况（例如： "foo.bar"）
            string[] pathParts = property.propertyPath.Split('.');

            // 逐级解析字段
            FieldInfo fieldInfo = null;
            for (int i = 0; i < pathParts.Length; i++) {
                // 如果有数组或列表的情况，需要特殊处理
                string fieldName = pathParts[i];
                if (fieldName == "Array" && i + 1 < pathParts.Length && pathParts[i + 1].StartsWith("data[")) {
                    // 数组或 List 的元素类型
                    if (fieldInfo != null) {
                        Type elementType = fieldInfo.FieldType.IsArray
                            ? fieldInfo.FieldType.GetElementType()
                            : fieldInfo.FieldType.GetGenericArguments()[0];
                        fieldInfo = elementType.GetField("dummy",
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        // 此处只是为了更新 fieldInfo，实际只需要 elementType
                        // 跳过下一个部分
                        i++;
                        continue;
                    }
                }
                else {
                    fieldInfo = targetType.GetField(fieldName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fieldInfo == null) {
                        return null;
                    }

                    targetType = fieldInfo.FieldType;
                }
            }

            return fieldInfo != null ? fieldInfo.FieldType : null;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // 使用反射获取键和值的类型和名称
            var keyProp = property.FindPropertyRelative("key");
            var valueProp = property.FindPropertyRelative("value");
            
            string dataPath = Application.dataPath;
            string projectName = new DirectoryInfo(dataPath).Parent.Name;
            bool percentageControl = true;
            float keyPercentage = EditorPrefs.GetFloat($"{projectName}.{getType(property).Name}.Key_Width", 0);
            float valuePercentage = EditorPrefs.GetFloat($"{projectName}.{getType(property).Name}.Value_Width", 0);

            if (keyPercentage is >= 1 or <= 0) percentageControl = false;
            if (valuePercentage is >= 1 or <= 0) percentageControl = false;
            
            // 开始属性绘制
            EditorGUI.BeginProperty(position, label, property);

            // 去除缩进
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // 计算每个字段的区域
            float labelWidth = position.width / 2 - 5;
            float fieldWidth = (position.width - labelWidth) - 5;
            if (percentageControl) {
                labelWidth = position.width * keyPercentage;
                fieldWidth = position.width * valuePercentage;
            }

            Rect labelRect = new Rect(position.x + 5, position.y, labelWidth, position.height);
            Rect stringRect = new Rect(position.x + labelWidth + 5, position.y, fieldWidth, position.height);
            Rect glowLightRect = new Rect(position.x + labelWidth + fieldWidth + 10, position.y, fieldWidth, position.height);

            // 绘制标签
            // EditorGUI.LabelField(, label);

            // 绘制字段
            EditorGUI.PropertyField(labelRect, keyProp, GUIContent.none);
            EditorGUI.PropertyField(stringRect, valueProp, GUIContent.none);

            // 恢复缩进
            EditorGUI.indentLevel = indent;

            // 结束属性绘制
            EditorGUI.EndProperty();
        }
    }
    # endif
}