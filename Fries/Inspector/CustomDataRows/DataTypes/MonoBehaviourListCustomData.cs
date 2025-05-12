using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    public class MonoBehaviourListCustomData : CustomDataType {

        public string getDisplayName() {
            return "List<MonoBehaviour>";
        }

        public Type getType() {
            return typeof(ListWrapper<MonoBehaviour>);
        }

        public object getDefaultValue() {
            return new ListWrapper<MonoBehaviour>();
        }
    }
}