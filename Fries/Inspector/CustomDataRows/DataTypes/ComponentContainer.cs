using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    public class ComponentContainer : Unwrapper {
        public Component component;
        public object unwrap() {
            return component;
        }
    }
}