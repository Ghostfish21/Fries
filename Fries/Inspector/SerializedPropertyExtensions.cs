# if UNITY_EDITOR
using UnityEditor;
using System;
using System.Collections;
using System.Reflection;
using Fries.Inspector.GameObjectBoxField;

namespace Fries.Inspector {
    public static class SerializedPropertyExtensions {
        public static SerializableSysObject getValue(this SerializedProperty property) {
            Type parentType = property.serializedObject.targetObject.GetType();
            string[] comps = property.propertyPath.Split(".");
            object value = property.serializedObject.targetObject;
            foreach (var comp in comps) {
                if (comp == "Array") continue;
                if (comp.Contains("data[")) {
                    int i = int.Parse(comp.Replace("data[", "").Replace("]", ""));
                    IList list = value as IList;
                    value = list[i]; 
                    parentType = value.GetType();
                    continue;
                }
                FieldInfo fi = parentType.GetField(comp);
                value = fi.GetValue(value);
                parentType = value.GetType();
            }

            return (SerializableSysObject)value;
        }
    }
}
# endif