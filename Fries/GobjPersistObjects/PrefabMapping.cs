using System.Collections.Generic;
using UnityEngine;

namespace Fries.GobjPersistObjects {
    [CreateAssetMenu(fileName = "Prefab Mapping", menuName = "Gobj Persist Objects/Prefab Mapping")]
    public class PrefabMapping : ScriptableObject {
        [SerializeField] private List<PrefabEntry> prefabEntries;
        
        private Dictionary<string, string> prefabDict;

        private void OnEnable() => BuildDictionary();

#if UNITY_EDITOR
        private void OnValidate() => BuildDictionary();
#endif

        private void BuildDictionary() {
            prefabDict = new Dictionary<string, string>();

            if (prefabEntries == null) return;

            foreach (var entry in prefabEntries) {
                if (entry == null) continue;

                if (string.IsNullOrEmpty(entry.prefabName)) {
                    Debug.LogWarning($"In prefab mapping {name}, prefab {entry.prefabName} has empty prefabName, skipping...", this);
                    continue;
                }

                if (string.IsNullOrEmpty(entry.prefabPath)) {
                    Debug.LogWarning($"In prefab mapping {name}, prefab {entry.prefabName} doesn't have prefabPath, skipping...", this);
                    continue;
                }

                if (prefabDict.ContainsKey(entry.prefabName)) 
                    Debug.LogWarning($"In prefab mapping {name}, detected duplicated prefabName: {entry.prefabName}, the later value will overwrite the former value.", this);

                prefabDict[entry.prefabName] = entry.prefabPath;
            }
        }
        
        public bool TryGetPath(string prefabName, out string prefabPath) {
            if (prefabDict == null) BuildDictionary();
            if (prefabDict == null) {
                prefabPath = null;
                return false;
            }
            return prefabDict.TryGetValue(prefabName, out prefabPath);
        }
        
        public bool Contains(string prefabName) {
            if (prefabDict == null) BuildDictionary();
            if (prefabDict == null) return false; 
            return prefabDict.ContainsKey(prefabName);
        }

        public void Rebuild() => BuildDictionary();
    }
}