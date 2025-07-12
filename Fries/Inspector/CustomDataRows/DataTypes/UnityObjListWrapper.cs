using System.Collections.Generic;
using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    public class UnityObjListWrapper : Unwrapper {
        public List<Object> list = new();
        public object unwrap() {
            return list;
        }
    }
}