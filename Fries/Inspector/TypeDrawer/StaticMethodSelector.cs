using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Fries.Inspector.TypeDrawer {
    [Serializable]
    public class StaticMethodSelector {
        [SerializeReference] [SerializeField] private TypeWrapper script;
        public Func<Type, bool> typeFilter;
		public Func<MethodInfo, bool> methodFilter;

        public StaticMethodSelector(Func<Type, bool> typeFilter, Func<MethodInfo, bool> methodFilter) {
            this.typeFilter = typeFilter;
            this.methodFilter = methodFilter;
        }

        private string[] defaultName => new[] {"None"};
        public string[] typeNames { get; private set; } = {"None"};
        public int selectedType = 0;
        [SerializeField] private string selectedTypeName;

        [SerializeField] private string prevSelectedTypeName;
        public void recordSelectedTypeName() {
            if (typeNames == null || typeNames.Length == 0) return;
            selectedTypeName = typeNames[selectedType];
            if (selectedTypeName != prevSelectedTypeName) 
                reloadMethodNames = true;
            prevSelectedTypeName = selectedTypeName;
        }
        public void refreshTypeNameArray() {
            script ??= new TypeWrapper();
            List<Type> types = script.getTypes(out bool isValueChanged);
            if (types.Count == 0) selectedType = 0;
            if (!isValueChanged) return;
            reloadMethodNames = true;
            typeNames = SystemUtils.concat(defaultName, types.Select(t => t.Name).ToArray());
            
            if (selectedTypeName == typeNames[selectedType]) return;
            int index = Array.IndexOf(typeNames, selectedTypeName);
            if (index == -1) selectedType = 0;
            else selectedType = index;
        }
        
        public string[] methodNames { get; private set; } = {"None"};
        private bool reloadMethodNames = false;
        public int selectedMethod = 0;
        [SerializeField] private string methodName;
        public void recordSelectedMethodName() {
            if (methodNames == null || methodNames.Length == 0) return;
            methodName = methodNames[selectedMethod];
        }

        public void refreshMethodNameArray() {
            if (selectedType == 0) {
                methodNames = defaultName;
                selectedMethod = 0;
                return;
            }
            if (!reloadMethodNames) return;
            List<Type> types = script.getTypes(out _);
            MethodInfo[] mi = types[selectedType - 1].GetMethods(BindingFlags.Static | BindingFlags.Public);
            methodNames = new string[mi.Length + 1];
            methodNames[0] = "None";
            for (int i = 0; i < mi.Length; i++) {
                string paramString = mi[i].GetParameters().Select(p => p.ParameterType.Name).Aggregate((a, b) => a + ", " + b);
                methodNames[i + 1] = mi[i].Name + "("+ paramString + ")";
            }
            
            if (methodName == methodNames[selectedMethod]) return;
            int index = Array.IndexOf(methodNames, methodName);
            if (index == -1) selectedMethod = 0;
            else selectedMethod = index;
        }

        public Type getSelectedType() {
            refreshTypeNameArray();
            return script.getTypes(out _)[selectedType - 1];
        }
        public MethodInfo getSelectedMethod() {
            refreshTypeNameArray();
            refreshMethodNameArray();
            return getSelectedType().GetMethods(BindingFlags.Static | BindingFlags.Public)[selectedMethod - 1];
        }
    }
}