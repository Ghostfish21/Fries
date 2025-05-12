using System;
using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    public class FloatCustomData : CustomDataType {
        
        public string getDisplayName() {
            return "Float";
        }

        public Type getType() {
            return typeof(FloatWrapper);
        }

        public object getDefaultValue() {
            return new FloatWrapper();
        }
    }
}