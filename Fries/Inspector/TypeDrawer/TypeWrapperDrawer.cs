# if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Fries.Inspector.TypeDrawer {
    [CustomPropertyDrawer(typeof(TypeWrapper))]
    public class TypeWrapperDrawer : PropertyDrawer {
        // 绘制所有 public 选项后，如果选项为空，再单独绘制一个窗口用于拖拽monoscript，拽进去后捕获值到 TypeWrapper 中
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty sp = property.FindPropertyRelative("typeToString");
            EditorGUI.PropertyField(position, property.FindPropertyRelative("assemblyName"), GUIContent.none);
            EditorGUI.PropertyField(position, property.FindPropertyRelative("typeNameIncludeNamespace"), GUIContent.none);
            EditorGUI.PropertyField(position, sp, GUIContent.none);
            
            EditorGUI.EndProperty();
        }
    }
}
# endif