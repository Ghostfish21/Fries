using System.Collections.Generic;
using Fries.Pool;
using UnityEngine;

namespace Fries {
    public class MultiTag : MonoBehaviour {
        internal static Dictionary<GameObject, DictList<string>> tagData = new();
        
        public List<string> tags;

        private void Awake() {
            tagData[gameObject] = new DictList<string>();
            tags.ForEach(tag => {
                tagData[gameObject].Add(tag);
            });
            tags.Clear();
            tags.Add("Tags are not editable in inspector during runtime");
        }
    }

    public static class MultiTagExts {
        public static void addTag(this GameObject gameObject, string tag) {
            if (!MultiTag.tagData.ContainsKey(gameObject)) 
                MultiTag.tagData[gameObject] = new DictList<string>();
            MultiTag.tagData[gameObject].Add(tag);
        }

        public static void removeTag(this GameObject gameObject, string tag) {
            if (!MultiTag.tagData.ContainsKey(gameObject)) return;
            if (MultiTag.tagData[gameObject].Contains(tag)) 
                MultiTag.tagData[gameObject].Remove(tag);
        }

        public static bool hasTag(this GameObject gameObject, string tag) {
            if (!MultiTag.tagData.ContainsKey(gameObject)) return false;
            if (MultiTag.tagData[gameObject].Contains(tag)) return true;
            return false;
        }
    }
}