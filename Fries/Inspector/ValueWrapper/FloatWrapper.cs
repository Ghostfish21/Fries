using System;
using UnityEngine;
# if UNITY_EDITOR
using UnityEditor;
# endif

namespace Fries.Inspector.ValueWrapper {
    [Serializable]
    public class FloatWrapper {
        public string label;
        public float value;
        public Func<float> init;
        public Action<float> setter;

        public FloatWrapper(Func<float> init) {
            this.init = init;
            value = this.init();
        }
    }

    # if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FloatWrapper))]
    public class FloatWrapperDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginChangeCheck();
            
            SerializedProperty labelProperty = property.FindPropertyRelative("label");
            SerializedProperty valueProperty = property.FindPropertyRelative("value");
            string displayName = labelProperty.stringValue;
            EditorGUI.PropertyField(position, valueProperty, new GUIContent(displayName));

            FloatWrapper fw = (FloatWrapper)property.getValue();
            if (EditorGUI.EndChangeCheck()) {
                fw.setter(fw.value);
            }
        }
    }
    # endif
}