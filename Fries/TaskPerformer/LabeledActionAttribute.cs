using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Fries.TaskPerformer {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class LabeledActionAttribute : Attribute {
        public static Dictionary<string, Func<object[], object>> labeledActions { get; } = new();

        [InitializeOnLoadMethod]
        public static void loadDefault() {
            load();
        }
        
        public static void load(string[] loadAssembly = null) {
            // 尝试加载 Assembly-CSharp
            try {
                Assembly assemblyCSharp = Assembly.Load("Assembly-CSharp");
                if (assemblyCSharp != null) 
                    registerActions(assemblyCSharp);
                
                // 加载当前程序集
                Assembly selfAssembly = Assembly.GetExecutingAssembly();
                if (selfAssembly != assemblyCSharp) 
                    registerActions(Assembly.GetExecutingAssembly());
            } catch (Exception ex) {
                Debug.LogWarning($"Failed to load assembly!\n{ex}");
            }

            // 加载 loadAssembly 中指定的程序集
            if (loadAssembly == null) return;
            foreach (var assemblyName in loadAssembly.Nullable()) {
                try {
                    Assembly asm = Assembly.Load(assemblyName);
                    if (asm != null) registerActions(asm);
                } catch {
                    Debug.LogWarning($"Failed to load assembly {assemblyName}!");
                }
            }
        }
        
        private static void registerActions(Assembly assembly = null) {
            if (assembly == null) {
                Debug.LogError("Specific Assembly is null!");
                return;
            }
            
            foreach (Type type in assembly.GetTypes()) {
                // 只扫描静态方法（包括 public 和 non-public）
                MethodInfo[] staticMethods =
                    type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (MethodInfo method in staticMethods) {
                    var attribute = method.GetCustomAttribute<LabeledActionAttribute>();
                    if (attribute != null) {
                        // 检查方法参数：必须只有一个参数且类型为 object[]
                        ParameterInfo[] parameters = method.GetParameters();
                        if (parameters.Length != 1 || parameters[0].ParameterType != typeof(object[])) {
                            throw new InvalidOperationException(
                                $"Method {method.DeclaringType?.FullName}.{method.Name} was marked as LabeledAction, but has illegal argument(s). Single object[] argument is required.");
                        }

                        // 检查返回类型：必须为 void
                        if (method.ReturnType != typeof(object)) {
                            throw new InvalidOperationException(
                                $"Method {method.DeclaringType?.FullName}.{method.Name} was marked as LabeledAction, but has illegal return type. Object is required to be return.");
                        }

                        // 创建 Func<object[], object> 委托
                        Func<object[], object> action = (Func<object[], object>)Delegate.CreateDelegate(typeof(Func<object[], object>), method);
                        labeledActions[attribute.Label] = action;
                    }
                }
            }
        }

        public string Label { get; }

        public LabeledActionAttribute(string label) {
            Label = label;
        }
    }
}