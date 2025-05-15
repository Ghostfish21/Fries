using System;

namespace Fries.Inspector.CustomDataRows {
    public class GlobalInstDeclarerCustomData : CustomDataType {
        public string getDisplayName() {
            return "GlobalInstDeclarer";
        }

        public Type getType() {
            return typeof(GlobalInstDeclarer);
        }

        public object getDefaultValue() {
            return new GlobalInstDeclarer();
        }
    }
}