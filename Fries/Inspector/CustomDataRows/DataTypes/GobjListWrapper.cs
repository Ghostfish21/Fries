using System.Collections.Generic;
using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    public class GobjListWrapper : Unwrapper {
        public List<GameObject> list = new();
        public object unwrap() {
            return list;
        }
    }
}