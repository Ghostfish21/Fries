# if UNITY_EDITOR

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Fries.Inspector.MethodFields {
    [Obsolete]
    [Serializable]
    public class StaticMethod {
        [SerializeField] private MonoScript targetScript;
        [SerializeField] private string selectedMethodName;
        
        private bool isInited = false;
        private Type[][] argTypes;
        private MethodInfo[] cachedMethodInfos;
        private Delegate[] methods;

        private Type[] getArgTypes() {
            return Type.EmptyTypes;
        }

        private bool checkArgTypes(object[] args) {
            return false;
        }
        
        private void init() {
            isInited = true;
            if (!targetScript || string.IsNullOrEmpty(selectedMethodName)) {
                cachedMethodInfos = null;
                argTypes = Array.Empty<Type[]>();
                isInited = false;
                return;
            }

            Type targetType = targetScript.GetClass();
            if (targetType == null) {
                cachedMethodInfos = null;
                argTypes = Array.Empty<Type[]>();
                isInited = false;
                return;
            }

            // Find static methods with the given name
            MethodInfo[] methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => m.Name == selectedMethodName)
                .ToArray();

            // For simplicity, we'll pick the first one if multiple overloads exist.
            // A more robust solution would involve storing parameter types to distinguish overloads.
            cachedMethodInfos = methods;
            argTypes = new Type[methods.Length][];

            if (cachedMethodInfos != null) {
                int i = 0;
                foreach (var cachedMethodInfo in cachedMethodInfos) {
                    argTypes[i] = cachedMethodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
                    // Create a delegate for static methods
                    this.methods[i] = cachedMethodInfo.CreateDelegate(Expression.GetDelegateType(
                        argTypes[i].Concat(new[] { cachedMethodInfo.ReturnType }).ToArray()
                    ));
                    i++;
                }
            } else {
                argTypes = Array.Empty<Type[]>();
                isInited = false;
                return;
            }
        }

        public void invoke(object[] args) {
            return;
        }
        
        public T invoke<T>(object[] args) {
            return (T)(object)null;
        }
    }
}

# endif