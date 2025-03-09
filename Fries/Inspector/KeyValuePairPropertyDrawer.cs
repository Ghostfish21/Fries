using System;
using System.IO;
using System.Reflection;

namespace Fries.Inspector {
    using UnityEngine;
    
    # if UNITY_EDITOR
    using UnityEditor;
    [CustomPropertyDrawer(typeof(KiiValuePair), true)]
    public class KeyValuePairPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // 使用反射获取键和值的类型和名称
            var keyProp = property.FindPropertyRelative("key");
            var valueProp = property.FindPropertyRelative("value");
            
            string dataPath = Application.dataPath;
            string projectName = new DirectoryInfo(dataPath).Parent.Name;
            bool percentageControl = true;
            float keyPercentage = EditorPrefs.GetFloat($"{projectName}.{property.type}.Key_Width", 0);
            float valuePercentage = EditorPrefs.GetFloat($"{projectName}.{property.type}.Value_Width", 0);

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