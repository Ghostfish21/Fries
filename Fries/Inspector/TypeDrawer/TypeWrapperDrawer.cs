# if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Fries.Inspector.TypeDrawer {
    [CustomPropertyDrawer(typeof(TypeWrapper))]
    public class TypeWrapperDrawer : PropertyDrawer {
        // 绘制所有 public 选项后，如果选项为空，再单独绘制一个窗口用于拖拽monoscript，拽进去后捕获值到 TypeWrapper 中
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUILayout.BeginVertical();
            
            SerializedProperty t2s = property.FindPropertyRelative("typeToString");
            SerializedProperty assemblyName = property.FindPropertyRelative("assemblyName");
            SerializedProperty typeNameIncludeNamespace = property.FindPropertyRelative("typeNameIncludeNamespace");
            EditorGUILayout.LabelField(property.displayName + $" {t2s.stringValue}");
            if (string.IsNullOrEmpty(t2s.stringValue)) {
                MonoScript script = (MonoScript)EditorGUILayout.ObjectField("Class", null, typeof(MonoScript), false);
                if (script) Debug.Log(script.text);
            }

            EditorGUILayout.EndVertical();
        }
    }
}
# endif