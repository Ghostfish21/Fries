using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    public class UnityObjListCustomData : CustomDataType {

        public string getDisplayName() {
            return "List<Unity.Object>";
        }

        public Type getType() {
            return typeof(UnityObjListWrapper);
        }

        public object getDefaultValue() {
            return new UnityObjListWrapper();
        }
    }
}