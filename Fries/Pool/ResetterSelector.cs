using System;
using System.Reflection;
using Fries.Inspector.TypeDrawer;

namespace Fries.Pool {
    [Serializable]
    public class ResetterSelector : StaticMethodSelector {
        public override bool typeFilter(Type type) => true;

        public override bool methodFilter(MethodInfo mi) {
            var attr = mi.GetCustomAttribute(typeof(ResetterAttribute), false);
            if (attr == null) return false;
            if (mi.ReturnType != typeof(void)) return false;
            if (mi.GetParameters().Length != 1) return false;
            if (mi.GetParameters()[0].ParameterType != typeof(object)) return false;
            return true;
        }
    }
}