using System;
using System.Collections.Generic;
using System.IO;

# if UNITY_EDITOR
using UnityEditor;
# endif

using UnityEngine;

namespace Fries.GobjPersistObjects {
    [CreateAssetMenu(fileName = "Prefab Mapping", menuName = "Gobj Persist Objects/Prefab Mapping")]
    public class PrefabMapping : ScriptableObject {
        [SerializeField] private List<GameObject> prefabs;
        
        private Dictionary<string, string> prefabDict;

        private void OnEnable() => BuildDictionary();

#if UNITY_EDITOR
        private void OnValidate() => BuildDictionary();

        private static string GetResourcesRelativePath(GameObject prefab) {
            if (!prefab) {
                Debug.LogError("Prefab is null!");
                return null;
            }

            string fullPath = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(fullPath)) {
                Debug.LogError("The GameObject is not valid: " + prefab.name + " - " + prefab.GetInstanceID() + " - " + prefab.scene.name);
                return null;
            }

            string marker = "Resources/";
            int index = fullPath.IndexOf(marker, StringComparison.Ordinal);
            if (index < 0) {
                Debug.LogError("This Prefab is not located under Resources folder: " + fullPath);
                return null;
            }

            string relativePath = fullPath.Substring(index + marker.Length);
            relativePath = Path.ChangeExtension(relativePath, null); // 去掉 .prefab

            return relativePath;
        }
        public static (string, string) GetResourcesPathAndName(GameObject prefab) {
            string resourcesPath = GetResourcesRelativePath(prefab);
            if (string.IsNullOrEmpty(resourcesPath))
                return (null, null);

            string prefabName = Path.GetFileName(resourcesPath);
            return (resourcesPath, prefabName);
        }
#endif

        private void BuildDictionary() {
            prefabDict = new Dictionary<string, string>();

            if (prefabs == null) return;

            foreach (var entry in prefabs) {
                if (!entry) continue;
                (string prefabPath, string prefabName) = GetResourcesPathAndName(entry);

                if (string.IsNullOrEmpty(prefabName)) {
                    Debug.LogWarning($"In prefab mapping {name}, prefab {prefabName} has empty prefabName, skipping...", this);
                    continue;
                }

                if (string.IsNullOrEmpty(prefabPath)) {
                    Debug.LogWarning($"In prefab mapping {name}, prefab {prefabName} doesn't have prefabPath, skipping...", this);
                    continue;
                }

                if (prefabDict.ContainsKey(prefabName)) 
                    Debug.LogWarning($"In prefab mapping {name}, detected duplicated prefabName: {prefabName}, the later value will overwrite the former value.", this);

                prefabDict[prefabName] = prefabPath;
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