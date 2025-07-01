using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    public class GlobalDataDeclarer : Unwrapper {
        public Object data;
        public object unwrap() {
            return data;
        }
    }
}