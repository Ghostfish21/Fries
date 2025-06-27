using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    [Serializable]
    public class CustomDataRuntimeItem {
        public string name;
        public string type;
        public object value;
        public string valueStr;
        
        public T getValue<T>() {
            return (T)value;
        }

        public CustomDataRuntimeItem(string name, string type, object initialValue = null) {
            this.name = name;
            this.type = type;
            value = initialValue;
            if (value == null) valueStr = "NULL";
            else valueStr = value + $" @{RuntimeHelpers.GetHashCode(value):x}";
        }
        public void reset(string type, object value) {
            this.type = type;
            this.value = value;
            if (value == null) valueStr = "NULL";
            else valueStr = value + $" @{RuntimeHelpers.GetHashCode(value):x}";
        }
    }
}