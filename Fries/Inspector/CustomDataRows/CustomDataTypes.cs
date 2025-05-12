using System;
using System.Collections.Generic;
# if UNITY_EDITOR
using UnityEditor;
# endif
using UnityEngine;

namespace Fries.Inspector.CustomDataRows {
    public static class CustomDataTypes {
        internal static Dictionary<string, CustomDataType> cachedTypes = new();

        [RuntimeInitializeOnLoadMethod]
        # if UNITY_EDITOR
        [InitializeOnLoadMethod]
        # endif
        internal static void init() {
            ReflectionUtils.loopAssemblies(assembly => {
                Type[] types = assembly.GetTypes();
                foreach (var type in types) {
                    bool flag = typeof(CustomDataType).IsAssignableFrom(type);
                    if (!flag) continue;
                    if (type == typeof(CustomDataType)) continue;
                    CustomDataType t = (CustomDataType)Activator.CreateInstance(type);
                    cachedTypes[t.getType().ToString()] = t;
                }
            });
        }
    }
}