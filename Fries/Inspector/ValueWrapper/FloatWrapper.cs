using System;
using System.Threading.Tasks;
using UnityEngine;
# if UNITY_EDITOR
using UnityEditor;
# endif

namespace Fries.Inspector.ValueWrapper {
    [Serializable]
    public class FloatWrapper {
        public float value;
        public string label;

        public object target;
        public Func<float> init;
        public string initLabel;
        public Action<float> setter;
        public string setterLabel;

        public FloatWrapper(Func<float> init) {
            this.init = init;
            value = this.init();
        }
        public FloatWrapper(string initLabel, object target = null) {
            this.target = target;
            this.initLabel = initLabel;
            value = TaskPerformer.TaskPerformer.executeLabeledAction<float>(initLabel, null, target);
        }

        public void executeSetterLabel(float f, object targetLocal = null) {
            if (targetLocal == null) targetLocal = target;
            TaskPerformer.TaskPerformer.executeLabeledAction(setterLabel, new object[] { f }, targetLocal);
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
                var targetObj = property.serializedObject.targetObject as UnityEngine.Object;
                Undo.RecordObject(targetObj, "FloatWrapper.value Changed");
                property.serializedObject.ApplyModifiedProperties();

                if (fw.setter == null) 
                    Debug.Log("Setter is null, please remember to set it before changing the value");
                if (fw.setter != null) fw.setter.Invoke(fw.value);
                else fw.executeSetterLabel(fw.value);
            }
        }
    }
    # endif
}