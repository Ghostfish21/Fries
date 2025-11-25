# if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Fries.Inspector.TypeDrawer {
    [CustomPropertyDrawer(typeof(StaticMethodSelector))]
    public class TypeMethodSelectorDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            StaticMethodSelector tms = (StaticMethodSelector)property.managedReferenceValue;
            if (tms == null) {
                Debug.LogError("TypeMethodSelector must be initialized and labeled as [SerializeReference]!");
                return;
            }

            EditorGUI.BeginProperty(position, label, property); 
            GUILayout.BeginArea(position);
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.PropertyField(property.FindPropertyRelative("script"));
            
            tms.refreshTypeNameArray();
            tms.selectedType = EditorGUILayout.Popup("Type", tms.selectedType, tms.typeNames);
            tms.recordSelectedTypeName();
            
            tms.refreshMethodNameArray();
            tms.selectedMethod = EditorGUILayout.Popup("Static Method", tms.selectedMethod, tms.methodNames);
            tms.recordSelectedMethodName();
            
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            EditorGUI.EndProperty();
        }
    }
}
# endif