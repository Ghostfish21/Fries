using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    public class LegacyMonoBehaviourListCustomData : CustomDataType {

        public string getDisplayName() {
            return "List<MonoBehaviour> (Legacy)";
        }

        public Type getType() {
            return typeof(MonoBehaviourListWrapper);
        }

        public object getDefaultValue() {
            return new MonoBehaviourListWrapper();
        }
    }
}