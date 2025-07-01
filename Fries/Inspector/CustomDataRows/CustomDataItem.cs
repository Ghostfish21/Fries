using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    [Serializable]
    public class CustomDataItem {
        public string name;
        public string type;
        [SerializeReference] public object value;
        public bool shouldCopyToRuntime;
        
        public T getValue<T>() {
            return (T)value;
        }

        public CustomDataItem(string name, string type, object initialValue = null) {
            this.name = name;
            this.type = type;
            if (initialValue != null) value = initialValue;
            else value = CustomDataTypes.cachedTypes[this.type].getDefaultValue();
        }
    }
}