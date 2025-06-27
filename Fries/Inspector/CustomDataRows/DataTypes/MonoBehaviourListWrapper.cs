using System.Collections.Generic;
using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    public class MonoBehaviourListWrapper : Unwrapper {
        public List<MonoBehaviour> list = new();
        public object unwrap() {
            return list;
        }
    }
}