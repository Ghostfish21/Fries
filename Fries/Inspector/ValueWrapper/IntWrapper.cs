using System;
using UnityEngine;
# if UNITY_EDITOR
using UnityEditor;
# endif

namespace Fries.Inspector.ValueWrapper {
    [Serializable]
    public class IntWrapper {
        public string label;
        public int value;
        public Func<int> init;
        public Action<int> setter;

        public IntWrapper(Func<int> init) {
            this.init = init;
            value = this.init();
        }
    }

    # if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(IntWrapper))]
    public class IntWrapperDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginChangeCheck();
            
            SerializedProperty labelProperty = property.FindPropertyRelative("label");
            SerializedProperty valueProperty = property.FindPropertyRelative("value");
            string displayName = labelProperty.stringValue;
            EditorGUI.PropertyField(position, valueProperty, new GUIContent(displayName));

            IntWrapper iw = (IntWrapper)property.getValue();
            if (EditorGUI.EndChangeCheck()) {
                if (iw.setter == null) 
                    Debug.Log("Setter is null, please remember to set it before changing the value");
                iw.setter?.Invoke(iw.value);
            }
        }
    }
    # endif
}