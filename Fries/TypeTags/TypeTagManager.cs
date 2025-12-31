using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fries {
    public static class TypeTagManager {
        internal static Dictionary<GameObject, HashSet<Type>> tagData = new();
        
        public static void addTag(this GameObject gameObject, Type tag) {
            if (!tagData.TryGetValue(gameObject, out var set)) {
                set = new HashSet<Type>(); // 指定比较器，避免大小写/区域性问题
                tagData.Add(gameObject, set);
            }
            set.Add(tag);
        }

        public static void removeTag(this GameObject gameObject, Type tag) {
            if (!tagData.TryGetValue(gameObject, out var set)) return;
            set.Remove(tag);
        }

        public static void removeTypeTags(this GameObject gameObject) => TypeTagManager.tagData.Remove(gameObject);

        public static bool hasTag(this GameObject gameObject, Type tag) {
            if (!tagData.TryGetValue(gameObject, out var set)) return false;
            if (set.Contains(tag)) return true;
            return false;
        }
    }
}