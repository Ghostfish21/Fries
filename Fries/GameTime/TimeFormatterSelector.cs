using System;
using System.Reflection;
using Fries.GameTime;
using Fries.Inspector.TypeDrawer;

namespace Fries.Pool {
    [AttributeUsage(AttributeTargets.Method)]
    public class TimeFormatter : Attribute {}
    
    
    [Serializable]
    public class TimeFormatterSelector : StaticMethodSelector {
        public override bool typeFilter(Type type) => true;

        public override bool methodFilter(MethodInfo mi) {
            var attr = mi.GetCustomAttribute(typeof(TimeFormatter), false);
            if (attr == null) return false;
            if (mi.ReturnType != typeof(string)) return false;
            if (mi.GetParameters().Length != 1) return false;
            if (mi.GetParameters()[0].ParameterType != typeof(int)) return false;
            return true;
        }
    }
}