using UnityEngine;
using System.Collections.Generic;

namespace Fries.Inspector.CustomDataRows {

    public class CustomData : MonoBehaviour {
        [SerializeReference] [SerializeField] private List<CustomDataItem> dataStore = new();

        private Dictionary<string, CustomDataItem> _dataDictionary;

        void OnValidate() {
            rebuildDictionary();
        }

        private void Awake() {
            gameObject.getComponent<CustomData>();
            rebuildDictionary(); // Ensure dictionary is built on awake for runtime
        }

        public T getData<T>(string key) {
            return _dataDictionary[key].getValue<T>();
        }

        private void rebuildDictionary() {
            _dataDictionary = new Dictionary<string, CustomDataItem>();
            HashSet<string> names = new HashSet<string>();
            List<CustomDataItem> validItems = new List<CustomDataItem>();

            foreach (var item in dataStore) {
                if (item == null) {
                    Debug.LogError(
                        "Custom Data's item is null. This will lead to data lose. Don't save the scene / prefab until you have backed up the .meta file.");
                    return;
                }
                if (!string.IsNullOrEmpty(item.name) && names.Add(item.name)) {
                    _dataDictionary.Add(item.name, item);
                    validItems.Add(item);
                }
                else if (!string.IsNullOrEmpty(item.name)) {
                    Debug.LogWarning(
                        $"Duplicate variable name '{item.name}' found in CustomData. Please ensure names are unique.");
                }
            }
        }
        
    }

}
