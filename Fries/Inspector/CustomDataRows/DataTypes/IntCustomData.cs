using System;
using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    public class IntCustomData : CustomDataType {
        
        public string getDisplayName() {
            return "Int";
        }

        public Type getType() {
            return typeof(IntWrapper);
        }

        public object getDefaultValue() {
            return new IntWrapper();
        }
    }
}