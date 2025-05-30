using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    public class GameObjectWrapper : Unwrapper {
        public GameObject gobj;
        public object unwrap() {
            return gobj;
        }
    }
}