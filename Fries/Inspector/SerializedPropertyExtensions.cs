# if UNITY_EDITOR
using UnityEditor;
using System;
using System.Reflection;

namespace Fries.Inspector {
    public static class SerializedPropertyExtensions {
        /// <summary>
        /// 获取 SerializedProperty 对应的实际值
        /// </summary>
        /// <param name="property">SerializedProperty 实例</param>
        /// <returns>实际的对象值</returns>
        public static object GetValue(this SerializedProperty property) {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            // 获取 SerializedObject 的目标对象
            object obj = property.serializedObject.targetObject;

            // 处理 propertyPath，替换数组访问中的 ".Array.data[" 为 "["
            string path = property.propertyPath.Replace(".Array.data[", "[");
            string[] elements = path.Split('.');

            foreach (var element in elements) {
                if (element.Contains("[")) {
                    // 处理数组或列表的元素访问
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else {
                    // 处理普通字段或属性
                    obj = GetValue(obj, element);
                }

                if (obj == null)
                    break;
            }

            return obj;
        }

        /// <summary>
        /// 通过反射获取对象的字段或属性值
        /// </summary>
        private static object GetValue(object source, string name) {
            if (source == null)
                return null;

            Type type = source.GetType();

            // 尝试获取字段
            FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
                return field.GetValue(source);

            // 尝试获取属性
            PropertyInfo prop = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null)
                return prop.GetValue(source, null);

            return null;
        }

        /// <summary>
        /// 通过反射获取对象的数组或列表中的元素
        /// </summary>
        private static object GetValue(object source, string name, int index) {
            var enumerable = GetValue(source, name) as System.Collections.IEnumerable;
            if (enumerable == null)
                return null;

            var enumerator = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++) {
                if (!enumerator.MoveNext())
                    return null;
            }
            return enumerator.Current;
        }
    }
}
# endif