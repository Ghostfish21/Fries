using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Fries.Pool {
    public static class PoolExt {
        public static object toPool(this GameObject prefab, Type type, Transform root, int size) {
            var mi = typeof(PoolExt).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "toPool"
                            && m.IsGenericMethodDefinition
                            && m.GetParameters().Length == 3);

            if (!typeof(Component).IsAssignableFrom(type)) throw new ArgumentException("Type must be assignable from Component");

            var g = mi.MakeGenericMethod(type);
            return g.Invoke(null, new object[] { prefab, root, size });
        }
        
        public static CompPool<T> toPool<T>(this GameObject prefab, Transform root, int size = 5) where T : Component => new(root, prefab, size);
        public static CompPool<T> toPool<T>(this GameObject prefab, Action<T> resetter, Transform root, int size = 5) where T : Component => new(resetter, root, prefab, size);
        public static CompPool<T> toPool<T>(this GameObject prefab, Action<T> compSetup, Action<T> resetter, Transform root, int size = 5) where T : Component => new(compSetup, resetter, root, prefab, size);
    }
}