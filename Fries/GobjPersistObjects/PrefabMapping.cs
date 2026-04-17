using System;
using System.Collections.Generic;
using System.IO;

# if UNITY_EDITOR
using UnityEditor;
# endif

using UnityEngine;

namespace Fries.GobjPersistObjects {
    [CreateAssetMenu(fileName = "Prefab Mapping", menuName = "Gobj Persist Objects/Prefab Mapping")]
    public class PrefabMapping : ScriptableObject, ISerializationCallbackReceiver {
        public void OnBeforeSerialize() {
#if UNITY_EDITOR
            BuildDictionary();
#endif
        }

        public void OnAfterDeserialize() { }

        [SerializeField] [HideInInspector] private List<string> prefabPaths = new();
        [SerializeField] [HideInInspector] private List<string> prefabNames = new();
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
        public static (string, string) GetResourcesPathAndName(GameObject prefab, int i) {
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
            
            # if UNITY_EDITOR
            prefabPaths.Clear();
            prefabNames.Clear();
            # endif
            
            for (int i = 0; i < prefabs.Count; i++) {
                # if UNITY_EDITOR
                prefabPaths.Add("");
                prefabNames.Add("");
                # endif
                
                GameObject entry = prefabs[i];
                if (!entry) continue;
                
                # if UNITY_EDITOR
                (string prefabPath, string prefabName) = GetResourcesPathAndName(entry, i);
                prefabPaths[i] = prefabPath;
                prefabNames[i] = prefabName;
                # else 
                string prefabPath = prefabPaths[i];
                string prefabName = prefabNames[i];
                # endif

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