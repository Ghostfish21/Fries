using System;
using UnityEngine;

namespace Fries.Inspector.ComponentWrapper {
    [Serializable]
    public class ComponentWrapper {
        public Component component;
        public static implicit operator ComponentWrapper(Component comp) {
            return new ComponentWrapper { component = comp };
        }
    }
}