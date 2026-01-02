using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Fries.CompCache {
    public static class TypeTagManager {
        private static Dictionary<GameObject, Dictionary<Type, int>> tagData = new();
        [Preserve]
        public static void addTag(this GameObject gameObject, Type tag) {
            if (!tagData.TryGetValue(gameObject, out var set)) {
                set = new Dictionary<Type, int>(); // 指定比较器，避免大小写/区域性问题
                tagData.Add(gameObject, set);
            }
            int count = set.GetValueOrDefault(tag, 0);
            set[tag] = count + 1;
        }
        [Preserve]
        public static void removeTag(this GameObject gameObject, Type tag) {
            if (!tagData.TryGetValue(gameObject, out var set)) return;
            int count = set.GetValueOrDefault(tag, 0);
            if (count == 0) {
                set.Remove(tag);
                return;
            }
            set[tag] = count - 1;
        }
        [Preserve]
        public static void removeTypeTags(this GameObject gameObject) => tagData.Remove(gameObject);
        [Preserve]
        public static bool hasTag(this GameObject gameObject, Type tag) {
            if (!tagData.TryGetValue(gameObject, out var set)) return false;
            if (!set.TryGetValue(tag, out int count)) return false;
            if (count == 0) return false;
            return true;
        }
    }
}