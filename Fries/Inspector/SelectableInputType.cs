using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;
#endif

namespace Fries.Inspector {
    [System.Serializable]
    public class SelectableInputType {
#if UNITY_EDITOR
        public List<string> inputTypes;
        public List<string> inputProperties;
        public MonoBehaviour target;

        public SelectableInputType(MonoBehaviour target) {
            this.target = target;
        }

        public int selectedIndex;

        public string getSelectedType() {
            return inputTypes[selectedIndex];
        }

        public object getValue() {
            FieldInfo fieldInfo = target.GetType().GetField(inputProperties[selectedIndex]);
            return fieldInfo.GetValue(target);
        }
#endif
    }

}
