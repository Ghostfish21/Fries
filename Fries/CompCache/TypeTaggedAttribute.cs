using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

namespace Fries.CompCache {
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false)]
    public class TypeTag : Attribute {
        public readonly bool suppressWarning;
        public TypeTag(bool suppressWarning = false) => this.suppressWarning = suppressWarning;
    }

    public static class TypeTagHelper {
        [Preserve]
        public static void addTags(MonoBehaviour monoBehaviour, Type typeAtCurrentLevel) {
            if (monoBehaviour.GetType() != typeAtCurrentLevel) return;

            Type origin = monoBehaviour.GetType();
            Type type = origin;
            var attr = type.GetCustomAttribute(typeof(TypeTag));
            if (attr != null) monoBehaviour.gameObject.addTag(type, monoBehaviour);
            
            while (type.BaseType != null) {
                type = type.BaseType;
                attr = type.GetCustomAttribute(typeof(TypeTag));
                if (attr != null) monoBehaviour.gameObject.addTag(type, monoBehaviour);
            }
            
            foreach (var @interface in origin.GetInterfaces()) {
                attr = @interface.GetCustomAttribute(typeof(TypeTag));
                if (attr != null) monoBehaviour.gameObject.addTag(@interface, monoBehaviour);
            }
        }

        [Preserve]
        public static void removeTags(MonoBehaviour monoBehaviour, Type typeAtCurrentLevel) {
            if (monoBehaviour.GetType() != typeAtCurrentLevel) return;

            Type origin = monoBehaviour.GetType();
            Type type = origin;
            var attr = type.GetCustomAttribute(typeof(TypeTag));
            if (attr != null) monoBehaviour.gameObject.removeTag(type, monoBehaviour);
            
            while (type.BaseType != null) {
                type = type.BaseType;
                attr = type.GetCustomAttribute(typeof(TypeTag));
                if (attr != null) monoBehaviour.gameObject.removeTag(type, monoBehaviour);
            }
            
            foreach (var @interface in origin.GetInterfaces()) {
                attr = @interface.GetCustomAttribute(typeof(TypeTag));
                if (attr != null) monoBehaviour.gameObject.removeTag(@interface, monoBehaviour);
            }
        }
        
        [Preserve]
        public static void removeAllTags(GameObject gameObject) => gameObject.removeTypeTags();
    }
}