using System;
using UnityEngine;
using UnityEngine.UIElements;
# if UNITY_EDITOR
using UnityEditor.UIElements;
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
        private bool isInited = false;
        private VisualElement tracker;
        
        public void init(SerializedProperty property) {
            if (isInited) return;
            isInited = true;

            tracker = new VisualElement();
# if UNITY_EDITOR
            tracker.TrackPropertyValue(property, onValueChanged);
# endif
            if (EditorWindow.focusedWindow != null) 
                EditorWindow.focusedWindow.rootVisualElement.Add(tracker);
        }

        private void onValueChanged(SerializedProperty property) {
            IntWrapper iw = (IntWrapper)property.getValue();

            if (iw.setter != null) iw.setter.Invoke(iw.value);
            else if (iw.setterLabel != null) iw.executeSetterLabel(iw.value);
            else Debug.Log("Setter is null, please remember to set it before changing the value");
        }
        
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            init(property);
            EditorGUI.BeginChangeCheck();
            
            SerializedProperty labelProperty = property.FindPropertyRelative("label");
            SerializedProperty valueProperty = property.FindPropertyRelative("value");
            string displayName = labelProperty.stringValue;
            EditorGUI.PropertyField(position, valueProperty, new GUIContent(displayName));
        }
    }
    # endif
}