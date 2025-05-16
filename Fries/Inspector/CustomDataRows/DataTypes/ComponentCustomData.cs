using System;

namespace Fries.Inspector.CustomDataRows {
    public class ComponentCustomData : CustomDataType {
        public string getDisplayName() {
            return "Component";
        }

        public Type getType() {
            return typeof(ComponentContainer);
        }

        public object getDefaultValue() {
            return new ComponentContainer();
        }
    }
}