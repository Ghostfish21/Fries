using System;

namespace Fries.Inspector.CustomDataRows {
    public class StringCustomData : CustomDataType {
        public string getDisplayName() {
            return "String";
        }

        public Type getType() {
            return typeof(StringWrapper);
        }

        public object getDefaultValue() {
            return new StringWrapper();
        }
    }
}