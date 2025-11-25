# if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Fries.Inspector.TypeDrawer {
    [CustomPropertyDrawer(typeof(StaticMethodSelector))]
    public class TypeMethodSelectorDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return 0;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            StaticMethodSelector tms = (StaticMethodSelector)property.managedReferenceValue;
            if (tms == null) {
                Debug.LogError("TypeMethodSelector must be initialized and labeled as [SerializeReference]!");
                return;
            }

            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.PropertyField(property.FindPropertyRelative("script"));
            EditorGUI.BeginChangeCheck();

            tms.refreshTypeNameArray();
            tms.selectedType = EditorGUILayout.Popup("Type", tms.selectedType, tms.typeNames.Nullable());
            tms.recordSelectedTypeName();
            
            tms.refreshMethodNameArray();
            tms.selectedMethod = EditorGUILayout.Popup("Static Method", tms.selectedMethod, tms.methodNames.Nullable());
            tms.recordSelectedMethodName();
            
            if (EditorGUI.EndChangeCheck()) {
                var target = property.serializedObject.targetObject;
                Undo.RecordObject(target, "Change Static Method Selector");
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
# endif