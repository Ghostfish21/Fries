using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    public class GlobalInstDeclarer : Unwrapper {
        public MonoBehaviour monoBehaviour;
        public object unwrap() {
            return monoBehaviour;
        }
    }
}