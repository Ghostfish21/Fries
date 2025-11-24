# if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Fries.Inspector.TypeDrawer {
    [CustomPropertyDrawer(typeof(TypeWrapper))]
    public class TypeWrapperDrawer : PropertyDrawer {
        // 绘制所有 public 选项后，如果选项为空，再单独绘制一个窗口用于拖拽monoscript，拽进去后捕获值到 TypeWrapper 中
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUILayout.BeginVertical();
            
            SerializedProperty scriptPathSp = property.FindPropertyRelative("scriptPath");
            EditorGUILayout.LabelField(property.displayName + $" {scriptPathSp.stringValue}");
            if (string.IsNullOrEmpty(scriptPathSp.stringValue)) {
                MonoScript script = (MonoScript)EditorGUILayout.ObjectField("Type", null, typeof(MonoScript), false);
                if (script) {
                    scriptPathSp.stringValue = AssetDatabase.GetAssetPath(script);
                    property.serializedObject.ApplyModifiedProperties();
                    Debug.Log(scriptPathSp.stringValue);
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}
# endif