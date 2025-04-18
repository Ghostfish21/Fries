# if UNITY_EDITOR
using UnityEditor;
# endif
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Fries.Inspector {
    public static class ReflectionUtils {
        # if UNITY_EDITOR
        public static SerializableSysObject getSsoValue(this SerializedProperty property) {
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
        
        public static object getValue(this SerializedProperty property) {
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
        
            return value;
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
        # endif
        
        public static void loopAssemblies(Action<Assembly> action, string[] loadAssembly = null) {
            // 尝试加载 Assembly-CSharp
            try {
                Assembly assemblyCSharp = Assembly.Load("Assembly-CSharp");
                if (assemblyCSharp != null) 
                    action(assemblyCSharp);
                
                // 加载当前程序集
                Assembly selfAssembly = Assembly.GetExecutingAssembly();
                if (selfAssembly != assemblyCSharp) 
                    action(Assembly.GetExecutingAssembly());
            } catch (Exception ex) {
                Debug.LogWarning($"Failed to load assembly!\n{ex}");
            }

            // 加载 loadAssembly 中指定的程序集
            if (loadAssembly == null) return;
            foreach (var assemblyName in loadAssembly.Nullable()) {
                try {
                    Assembly asm = Assembly.Load(assemblyName);
                    if (asm != null)
                        action(asm);
                }
                catch {
                    Debug.LogWarning($"Failed to load assembly {assemblyName}!");
                }
            }
        }

        public static bool checkSignature(this MethodInfo mi, Type returnType, params Type[] paramTypes) {
            if (mi.ReturnType != returnType) return false;
            var parameters = mi.GetParameters();
            if (parameters.Length != paramTypes.Length) return false;
            bool shouldReturnFalse = false;
            parameters.ForEach((i, p, b) => {
                if (p.ParameterType == paramTypes[i]) return;
                shouldReturnFalse = true;
                b.@break();
            });
            if (shouldReturnFalse) return false;
            return true;
        }

        public static Delegate toDelegate(this MethodInfo method, object targetInstance = null) {
            // 1. 获取方法的参数类型列表
            var paramTypes = method.GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();

            // 2. 动态生成一个 Action<...> 类型
            var actionType = Expression.GetActionType(paramTypes);

            return targetInstance == null
                ? Delegate.CreateDelegate(actionType, method)
                : Delegate.CreateDelegate(actionType, targetInstance, method);
        }
    }
}