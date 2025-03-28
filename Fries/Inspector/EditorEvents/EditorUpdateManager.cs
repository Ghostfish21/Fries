using System;
using System.Collections.Generic;
using System.Reflection;
using Fries;
# if UNITY_EDITOR
using UnityEditor;
# endif
using UnityEngine;

namespace DialogueSystem {
    # if UNITY_EDITOR
    [InitializeOnLoad]
    # endif
    public class EditorUpdateManager {

        private static EditorUpdateManager eum;
        public static EditorUpdateManager inst => eum;
        
        static EditorUpdateManager() {
            eum = new EditorUpdateManager();
        }
        
        public EditorUpdateManager(string[] loadAssembly = null) {
            // 尝试加载 Assembly-CSharp
            try {
                Assembly assemblyCSharp = Assembly.Load("Assembly-CSharp");
                if (assemblyCSharp != null) 
                    loadCommandsFromAssembly(assemblyCSharp);
                
                // 加载当前程序集
                Assembly selfAssembly = Assembly.GetExecutingAssembly();
                if (selfAssembly != assemblyCSharp) 
                    loadCommandsFromAssembly(Assembly.GetExecutingAssembly());
            } catch (Exception ex) {
                Debug.LogWarning($"Failed to load assembly!\n{ex}");
            }

            // 加载 loadAssembly 中指定的程序集
            if (loadAssembly == null) return;
            foreach (var assemblyName in loadAssembly.Nullable()) {
                try {
                    Assembly asm = Assembly.Load(assemblyName);
                    if (asm != null) 
                        loadCommandsFromAssembly(asm);
                } catch {
                    Debug.LogWarning($"Failed to load assembly {assemblyName}!");
                }
            }
        }
        
        private void loadCommandsFromAssembly(Assembly assembly) {
            foreach (Type type in assembly.GetTypes()) {
                // 获取类型中所有方法（公有、非公有，静态与实例方法）
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)) {
                    // 获取所有标记了 EditorUpdateAttribute 的特性
                    var attributes = method.GetCustomAttributes(typeof(EditorUpdateAttribute), false);
                    foreach (EditorUpdateAttribute attr in attributes) {
                        // 检查方法参数：必须没有参数
                        ParameterInfo[] parameters = method.GetParameters();
                        if (parameters.Length != 0)
                            continue;

                        try {
                            Action action;
                            if (method.IsStatic) {
                                // 静态方法无需实例化
                                action = (Action)Delegate.CreateDelegate(typeof(Action), method);
                            } else {
                                // 实例方法需要先创建实例
                                object instance = Activator.CreateInstance(type);
                                action = (Action)Delegate.CreateDelegate(typeof(Action), instance, method);
                            }

                            # if UNITY_EDITOR
                            EditorApplication.update += () => {action();};
                            # endif
                        } catch (Exception ex) {
                            Debug.LogError($"Found exception while registering Editor Update function!\n{ex}");
                        }
                    }
                }
            }
        }
    }
}