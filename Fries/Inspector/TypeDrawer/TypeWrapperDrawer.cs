# if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Fries.Inspector.TypeDrawer {
    [CustomPropertyDrawer(typeof(TypeWrapper))]
    public class TypeWrapperDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return 0;
        }
        
        // 绘制所有 public 选项后，如果选项为空，再单独绘制一个窗口用于拖拽monoscript，拽进去后捕获值到 TypeWrapper 中
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            TypeWrapper tw = (TypeWrapper)property.managedReferenceValue;
            if (tw == null) {
                Debug.LogError("TypeWrapper must be initialized and labeled as [SerializeReference]!");
                return;
            }

            EditorGUILayout.BeginHorizontal();
            
            SerializedProperty scriptPathSp = property.FindPropertyRelative("scriptPath");
            if (string.IsNullOrEmpty(scriptPathSp.stringValue)) {
                EditorGUILayout.EndHorizontal();
                MonoScript script = (MonoScript)EditorGUILayout.ObjectField(property.displayName, null, typeof(MonoScript), false);
                if (script) {
                    scriptPathSp.stringValue = AssetDatabase.GetAssetPath(script);
                    property.serializedObject.ApplyModifiedProperties();
                    EditorGUI.BeginChangeCheck();
                    tw.forceLoad = true;
                    tw.load();
                    if (EditorGUI.EndChangeCheck()) {
                        var target = property.serializedObject.targetObject;
                        Undo.RecordObject(target, "Change Static Method Selector");
                        EditorUtility.SetDirty(target);
                    }
                } 
            }
            else {
                EditorGUILayout.LabelField(property.displayName + $" : {scriptPathSp.stringValue}");
                if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(EditorGUIUtility.singleLineHeight))) {
                    scriptPathSp.stringValue = string.Empty;
                    property.serializedObject.ApplyModifiedProperties();
                }
                else {
                    EditorGUI.BeginChangeCheck();
                    tw.load();
                    if (EditorGUI.EndChangeCheck()) {
                        var target = property.serializedObject.targetObject;
                        Undo.RecordObject(target, "Change Static Method Selector");
                        EditorUtility.SetDirty(target);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
# endif