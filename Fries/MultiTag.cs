using System;
using System.Collections.Generic;
using Fries.Data;
using Fries.Pool;
using UnityEngine;

namespace Fries {
    public class MultiTag : MonoBehaviour {
        internal static Dictionary<GameObject, HashSet<string>> tagData = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void beforeSceneLoad() {
            tagData.Clear();
        }
        
        public List<string> tags;

        private void Awake() {
            tagData[gameObject] = new HashSet<string>();
            tags.ForEach(tag => {
                tagData[gameObject].Add(tag);
            });
            tags.Insert(0, "Tags are not editable in inspector during runtime");
        }
    }

    public static class MultiTagExts {
        public static void addTag(this GameObject gameObject, string tag) {
            if (!MultiTag.tagData.TryGetValue(gameObject, out var set)) {
                set = new HashSet<string>(StringComparer.Ordinal); // 指定比较器，避免大小写/区域性问题
                MultiTag.tagData.Add(gameObject, set);
            }
            set.Add(tag);
        }

        public static void removeTag(this GameObject gameObject, string tag) {
            if (!MultiTag.tagData.TryGetValue(gameObject, out var set)) return;
            set.Remove(tag);
        }

        public static bool hasTag(this GameObject gameObject, string tag) {
            if (!MultiTag.tagData.TryGetValue(gameObject, out var set)) return false;
            if (set.Contains(tag)) return true;
            return false;
        }
    }
}