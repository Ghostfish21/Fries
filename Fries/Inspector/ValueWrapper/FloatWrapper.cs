using System;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
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

        private bool isInited = false;
        private VisualElement tracker;
        
        public void init(SerializedProperty property) {
            if (isInited) return;
            isInited = true;

            tracker = new VisualElement();
            SerializedProperty valueProperty = property.FindPropertyRelative("value");
            tracker.TrackPropertyValue(valueProperty, undoRedo);
        }

        private void undoRedo(SerializedProperty property) {
            FloatWrapper fw = (FloatWrapper)property.getValue();

            if (fw.setter == null) 
                Debug.Log("Setter is null, please remember to set it before changing the value");
            if (fw.setter != null) fw.setter.Invoke(fw.value);
            else fw.executeSetterLabel(fw.value);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            init(property);
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
                EditorUtility.SetDirty(targetObj);

                if (fw.setter == null) 
                    Debug.Log("Setter is null, please remember to set it before changing the value");
                if (fw.setter != null) fw.setter.Invoke(fw.value);
                else fw.executeSetterLabel(fw.value);
            }
        }
    }
    # endif
}