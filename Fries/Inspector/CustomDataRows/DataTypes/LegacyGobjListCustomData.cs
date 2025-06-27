using System;
using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    
    public class LegacyGobjListCustomData : CustomDataType {
        
        public string getDisplayName() {
            return "List<GameObject> (Legacy)";
        }

        public Type getType() {
            return typeof(GobjListWrapper);
        }

        public object getDefaultValue() {
            return new GobjListWrapper();
        }
    }
}