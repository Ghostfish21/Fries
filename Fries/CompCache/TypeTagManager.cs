using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Fries.CompCache {
    public static class TypeTagManager {
        private static Dictionary<GameObject, Dictionary<Type, HashSet<object>>> tagData = new();
        [Preserve]
        public static void addTag(this GameObject gameObject, Type tag, object instance) {
            if (!tagData.TryGetValue(gameObject, out var set)) {
                set = new Dictionary<Type, HashSet<object>>(); // 指定比较器，避免大小写/区域性问题
                tagData.Add(gameObject, set);
            }

            if (!set.ContainsKey(tag)) set[tag] = new HashSet<object>();
            set[tag].Add(instance);
        }
        [Preserve]
        public static void removeTag(this GameObject gameObject, Type tag, object instanceToRemove) {
            if (!tagData.TryGetValue(gameObject, out var set)) return;
            if (!set.TryGetValue(tag, out var innerSet)) return;
            innerSet.Remove(instanceToRemove);
        }
        [Preserve]
        public static void removeTypeTags(this GameObject gameObject) => tagData.Remove(gameObject);
       
        public static bool HasTag<T>(this GameObject gameObject) {
            Type tag = typeof(T);
            if (!tagData.TryGetValue(gameObject, out var set)) return false;
            if (!set.TryGetValue(tag, out var innerSet)) return false;
            if (innerSet.Count == 0) return false;
            return true;
        }
        
        public static HashSet<T> GetTaggedObjects<T>(this GameObject gameObject) {
            Type type = typeof(T);
            if (!tagData.TryGetValue(gameObject, out var set)) return null;
            if (!set.TryGetValue(type, out var innerSet)) return null;
            if (innerSet.Count == 0) return null;
            HashSet<T> ret = new HashSet<T>();
            foreach (var item in innerSet) ret.Add((T)item);
            return ret;
        }

        public static T GetTaggedObject<T>(this GameObject gameObject) {
            Type type = typeof(T);
            if (!tagData.TryGetValue(gameObject, out var set)) return default;
            if (!set.TryGetValue(type, out var innerSet)) return default;
            if (innerSet.Count == 0) return default;
            foreach (var o in innerSet) return (T)o;
            return default;
        }
    }
}