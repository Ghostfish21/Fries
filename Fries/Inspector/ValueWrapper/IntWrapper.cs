using System;
using UnityEngine;
# if UNITY_EDITOR
using UnityEditor;
# endif

namespace Fries.Inspector.ValueWrapper {
    [Serializable]
    public class IntWrapper {
        public int value;
        public string label;
        
        public object target;
        public Func<int> init;
        public string initLabel;
        public Action<int> setter;
        public string setterLabel;

        public IntWrapper(Func<int> init) {
            this.init = init;
            value = this.init();
        }
        
        public IntWrapper(string initLabel, object target = null) {
            this.target = target;
            this.initLabel = initLabel;
            value = TaskPerformer.TaskPerformer.executeLabeledAction<int>(initLabel, null, target);
        }

        public void executeSetterLabel(float f, object targetLocal = null) {
            if (targetLocal == null) targetLocal = target;
            TaskPerformer.TaskPerformer.executeLabeledAction(setterLabel, new object[] { f }, targetLocal);
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
                if (iw.setter != null) iw.setter.Invoke(iw.value);
                else iw.executeSetterLabel(iw.value);
            }
        }
    }
    # endif
}