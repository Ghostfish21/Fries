# if UNITY_EDITOR
using UnityEditor;
using System;
using System.Collections;
using System.Diagnostics;
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
                    Debug.Assert(list != null, nameof(list) + " != null");
                    if (i < 0 || i >= list.Count) return null;
                    value = list[i]; 
                    parentType = value.GetType();
                    continue;
                }
                FieldInfo fi = parentType.GetField(comp);
                if (fi == null) return null;
                value = fi.GetValue(value);
                if (value == null) return null;
                parentType = value.GetType();
            }

            return value as SerializableSysObject;
        }
        
        public static bool hasAnnotation(this SerializedProperty sp, Type type) {
            if (sp == null || type == null)
                return false;

            // 通过辅助方法获取 FieldInfo
            FieldInfo field = sp.getFieldInfo();
            if (field == null) return false;

            // 调用针对 FieldInfo 的扩展方法进行判断
            return field.hasAnnotation(type);
        }

        public static FieldInfo getFieldInfo(this SerializedProperty property) {
            if (property == null) return null;

            Type parentType = property.serializedObject.targetObject.GetType();
            string[] comps = property.propertyPath.Split(".");
            object value = property.serializedObject.targetObject;
            FieldInfo fi = null;
            foreach (var comp in comps) {
                if (comp == "Array") continue;
                if (comp.Contains("data[")) {
                    int i = int.Parse(comp.Replace("data[", "").Replace("]", ""));
                    IList list = value as IList;
                    Debug.Assert(list != null, nameof(list) + " != null");
                    value = list[i]; 
                    parentType = value.GetType();
                    continue;
                }
                fi = parentType.GetField(comp);
                if (fi == null) return null;
                value = fi.GetValue(value);
                if (value == null) return null;
                parentType = value.GetType();
            }

            return fi;
        }
    }
}
# endif