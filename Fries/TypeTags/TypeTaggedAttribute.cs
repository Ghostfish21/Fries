using System;
using System.Reflection;
using UnityEngine;

namespace Fries.TypeTags {
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false)]
    public class TypeTag : Attribute { }

    public static class TypeTagHelper {
        public static void addTags(MonoBehaviour monoBehaviour, Type typeAtCurrentLevel) {
            if (monoBehaviour.GetType() != typeAtCurrentLevel) return;
            
            Type type = monoBehaviour.GetType();
            var attr = type.GetCustomAttribute(typeof(TypeTag));
            if (attr != null) monoBehaviour.gameObject.addTag(type);
            
            while (type.BaseType != null) {
                type = type.BaseType;
                attr = type.GetCustomAttribute(typeof(TypeTag));
                if (attr != null) monoBehaviour.gameObject.addTag(type);
            }
            
            foreach (var @interface in type.GetInterfaces()) {
                attr = @interface.GetCustomAttribute(typeof(TypeTag));
                if (attr != null) monoBehaviour.gameObject.addTag(@interface);
            }
        }

        public static void removeTags(MonoBehaviour monoBehaviour) => monoBehaviour.gameObject.removeTypeTags();
    }
}