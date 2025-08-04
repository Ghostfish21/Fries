using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
# if UNITY_EDITOR
using UnityEditor;
# endif

namespace Fries.Inspector.MethodFields {
    [Serializable]
    public class StringIntKvp {
        public string key;
        public int value;
        
        public StringIntKvp(string key, int value) {
            this.key = key;
            this.value = value;
        }
    }
    
    [Serializable]
    public class StaticMethod {
# if UNITY_EDITOR
        private MonoScript prevScript = null;
        public MonoScript targetScript;
# endif
        private void editorInit() {
# if UNITY_EDITOR
            targetType = targetScript.GetClass();
# endif
        }
        
        public Type targetType;
        public string selectedMethodName;
        
        private bool isInited = false; 
        [SerializeField]
        private List<StringIntKvp> argTypesSave;
        private Dictionary<string, int> argTypes;
        private MethodInfo[] cachedMethodInfos;
        private Delegate[] methods;
        
        public void init() {
            # if UNITY_EDITOR
            if (prevScript != targetScript) {
                prevScript = targetScript;
                selectedMethodName = "";
            }
            isInited = true;
            argTypes = new Dictionary<string, int>();
            argTypesSave = new List<StringIntKvp>();

            if (!targetScript || string.IsNullOrEmpty(selectedMethodName)) {
                cachedMethodInfos = null;
                isInited = false;
                return;
            }

            targetType ??= targetScript.GetClass();
            if (targetType == null) {
                cachedMethodInfos = null;
                isInited = false;
                return;
            }

            cachedMethodInfos = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => m.Name == selectedMethodName)
                .ToArray();
            methods = new Delegate[cachedMethodInfos.Length];

            if (cachedMethodInfos != null) {
                int i = 0;
                foreach (var cachedMethodInfo in cachedMethodInfos) {
                    Type[] argType = cachedMethodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
                    string[] argTypeStr = argType.Select(t => t.FullName).ToArray();
                    string typeStr = string.Join(" | ", argTypeStr);
                    argTypes[typeStr] = i;
                    argTypesSave.Add(new StringIntKvp(typeStr, i));
                    methods[i] = cachedMethodInfo.CreateDelegate(Expression.GetDelegateType(
                        argType.Concat(new[] { cachedMethodInfo.ReturnType }).ToArray()
                    ));
                    i++;
                }
            } else isInited = false;
            # endif
        }

        public void invoke(params object[] args) {
            if (!isInited) init();
            if (argTypes == null || argTypes.Count == 0) {
                argTypes = new();
                foreach (var tuple in argTypesSave) 
                    argTypes[tuple.key] = tuple.value;
            }
            
            string[] types = new string[args.Length];
            int i = 0;
            foreach (var o in args) {
                types[i] = o.GetType().FullName;
                i++;
            }
            int index = argTypes[string.Join(" | ", types)];
            if (index >= 0) methods[index].DynamicInvoke(args);
        }
        
        public T invoke<T>(params object[] args) {
            if (!isInited) init();
            if (argTypes == null || argTypes.Count == 0) {
                argTypes = new();
                foreach (var tuple in argTypesSave) 
                    argTypes[tuple.key] = tuple.value;
            }
            
            string[] types = new string[args.Length];
            int i = 0;
            foreach (var o in args) {
                types[i] = o.GetType().FullName;
                i++;
            }
            int index = argTypes[string.Join(" | ", types)];
            if (index >= 0) return (T)methods[index].DynamicInvoke(args);
            return (T)(object)null;
        }
    }
}