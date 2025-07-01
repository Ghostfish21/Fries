using System;

namespace Fries.Inspector.CustomDataRows {
    public class GlobalDataDeclarerCustomData : CustomDataType {
        public string getDisplayName() {
            return "GlobalDataDeclarer";
        }

        public Type getType() {
            return typeof(GlobalDataDeclarer);
        }

        public object getDefaultValue() {
            return new GlobalDataDeclarer();
        }
    }
}