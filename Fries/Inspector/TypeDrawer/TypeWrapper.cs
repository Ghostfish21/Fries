using System;
using UnityEngine.Analytics;

namespace Fries.Inspector.TypeDrawer {
    [Serializable]
    public class TypeWrapper {
        public string assemblyName;
        public string typeNameIncludeNamespace;
        public string typeToString;
        private long prevFindTime;
        private Type type;
        
        public void setType(string assemblyName, string typeNameIncludeNamespace) {
            this.assemblyName = assemblyName;
            this.typeNameIncludeNamespace = typeNameIncludeNamespace;
            findType();
        }

        private void findType() {
            if (prevFindTime == AnalyticsSessionInfo.sessionId) return;
            prevFindTime = AnalyticsSessionInfo.sessionId;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                if (asm.GetName().Name != assemblyName) continue;
                type = asm.GetType(typeNameIncludeNamespace);
                break;
            }
            if (type == null) typeToString = "";
            typeToString = type?.ToString();
        }
        
        public Type getType() {
            if (type != null) findType();
            return type;
        }
    }
}