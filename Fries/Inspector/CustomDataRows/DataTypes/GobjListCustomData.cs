using System;
using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    
    public class GobjListCustomData : CustomDataType {
        
        public string getDisplayName() {
            return "List<GameObject>";
        }

        public Type getType() {
            return typeof(ListWrapper<GameObject>);
        }

        public object getDefaultValue() {
            return new ListWrapper<GameObject>();
        }
    }
}