
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
    public class StaticMethod {
# if UNITY_EDITOR
        public MonoScript targetScript;
# endif
        public void editorInit() {
# if UNITY_EDITOR
            targetType = targetScript.GetClass();
# endif
        }
        
        public Type targetType;
        public string selectedMethodName;
        [SerializeReference]
        public Action onValueChanged;

        public StaticMethod() {
            onValueChanged = init;
        }
        
        private bool isInited = false;
        private Dictionary<Type[], int> argTypes;
        private MethodInfo[] cachedMethodInfos;
        private Delegate[] methods;
        
        private void init() {
            isInited = true;
            argTypes = new Dictionary<Type[], int>();

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

            if (cachedMethodInfos != null) {
                int i = 0;
                foreach (var cachedMethodInfo in cachedMethodInfos) {
                    Type[] argType = cachedMethodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
                    argTypes[argType] = i;
                    this.methods[i] = cachedMethodInfo.CreateDelegate(Expression.GetDelegateType(
                        argType.Concat(new[] { cachedMethodInfo.ReturnType }).ToArray()
                    ));
                    i++;
                }
            } else isInited = false;
        }

        public void invoke(params object[] args) {
            if (!isInited) init();
            
            Type[] types = new Type[args.Length];
            int i = 0;
            foreach (var o in args) {
                types[i] = o.GetType();
                i++;
            }
            int index = argTypes[types];
            if (index >= 0) methods[index].DynamicInvoke(args);
        }
        
        public T invoke<T>(params object[] args) {
            if (!isInited) init();
            
            Type[] types = new Type[args.Length];
            int i = 0;
            foreach (var o in args) {
                types[i] = o.GetType();
                i++;
            }
            int index = argTypes[types];
            if (index >= 0) return (T)methods[index].DynamicInvoke(args);
            return (T)(object)null;
        }
    }
}