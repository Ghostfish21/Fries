using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Analytics;

namespace Fries.Inspector.TypeDrawer {
    [Serializable]
    public class TypeWrapper {
        public string assemblyName;
        public List<string> typeNamesIncludeNamespace;
        public string typeToString;
        private long prevFindTime;
        private Type type;

        // 这个方法仅支持在 Editor 中运行
        public void fillInByFile(string assemblyName, string fullPath2File) {
# if UNITY_EDITOR
            string scriptContent = File.ReadAllText(fullPath2File);
            
# else
            throw new Exception("The fillInByFile method only works in Editor!");
# endif
        }
        
        private void setType(string assemblyName, List<string> typeNamesIncludeNamespace) {
            this.assemblyName = assemblyName;
            // this.typeNameIncludeNamespace = typeNameIncludeNamespace;
            findType();
        }

        private void findType() {
            // if (prevFindTime == AnalyticsSessionInfo.sessionId) return;
            // prevFindTime = AnalyticsSessionInfo.sessionId;
            // foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
            //     if (asm.GetName().Name != assemblyName) continue;
            //     type = asm.GetType(typeNameIncludeNamespace);
            //     break;
            // }
            // if (type == null) typeToString = "";
            // typeToString = type?.ToString();
        }
        
        public Type getType() {
            if (type != null) findType();
            return type;
        }
    }
}