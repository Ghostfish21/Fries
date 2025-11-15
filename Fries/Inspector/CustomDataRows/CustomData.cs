using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.Analytics;

namespace Fries.Inspector.CustomDataRows {

    public static class CustomDataExt {
        public static T getData<T>(this MonoBehaviour mono, string key) { return mono.getComponent<CustomData>().getData<T>(key); }
        public static T getData<T>(this GameObject gobj, string key) { return gobj.getComponent<CustomData>().getData<T>(key); }

        public static bool hasData(this MonoBehaviour mono, string key, string type) { return mono.getComponent<CustomData>().hasData(key, type); }
        public static bool hasData(this GameObject gobj, string key, string type) { return gobj.getComponent<CustomData>().hasData(key, type); }
        
        public static bool hasData(this MonoBehaviour mono, string key) { return mono.getComponent<CustomData>().hasData(key); }
        public static bool hasData(this GameObject gobj, string key) { return gobj.getComponent<CustomData>().hasData(key); }
        
        public static bool hasData<T>(this MonoBehaviour mono, string key) { return mono.getComponent<CustomData>().hasData<T>(key); }
        public static bool hasData<T>(this GameObject gobj, string key) { return gobj.getComponent<CustomData>().hasData<T>(key); }

        public static bool hasRuntimeData(this MonoBehaviour mono, string key) {
            var cd = mono.getComponent<CustomData>();
            if (!cd) return false;
            return cd.hasRuntimeData(key);
        }
        public static bool hasRuntimeData(this GameObject gobj, string key) {
            var cd = gobj.getComponent<CustomData>();
            if (!cd) return false;
            return cd.hasRuntimeData(key);
        }
        
        public static void setRuntimeData(this MonoBehaviour mono, string key, object value) { mono.getComponent<CustomData>().setRuntimeData(key, value); }
        public static void setRuntimeData(this GameObject gobj, string key, object value) { gobj.getComponent<CustomData>().setRuntimeData(key, value); }
        
        public static T getRuntimeData<T>(this MonoBehaviour mono, string key) { return mono.getComponent<CustomData>().getRuntimeData<T>(key); }
        public static T getRuntimeData<T>(this GameObject gobj, string key) { return gobj.getComponent<CustomData>().getRuntimeData<T>(key); }

        public static T getRuntimeDataOrNull<T>(this MonoBehaviour mono, string key) {
            var cd = mono.getComponent<CustomData>();
            if (!cd) return (T)(object)null;
            return cd.getRuntimeDataOrNull<T>(key);
        }
        public static T getRuntimeDataOrNull<T>(this GameObject gobj, string key) {
            var cd = gobj.getComponent<CustomData>();
            if (!cd) return (T)(object)null;
            return cd.getRuntimeDataOrNull<T>(key);
        }

        public static T getRuntimeDataOrDefault<T>(this MonoBehaviour mono, string key, T defaultValue) {
            var cd = mono.getComponent<CustomData>();
            if (!cd) return defaultValue;
            return cd.getRuntimeDataOrDefault<T>(key, defaultValue);
        }
        public static T getRuntimeDataOrDefault<T>(this GameObject gobj, string key, T defaultValue) {
            var cd = gobj.getComponent<CustomData>();
            if (!cd) return defaultValue;
            return cd.getRuntimeDataOrDefault(key, defaultValue);
        }
    }
    
    [DefaultExecutionOrder(int.MinValue + 50)]
    public class CustomData : MonoBehaviour {
        private static Dictionary<string, MonoBehaviour> globalInstances = new();
        private static Dictionary<string, Object> globalData = new();
        private static Dictionary<Object, long> dataSession = new();
        
        // A - 程序首次运行 Reset
        // B - 程序正常声明 SetGlobalData 并记录 B
        // C - 程序结束
        // D - 程序声明 SetGlobalData 并记录 D
        // E - 程序首次运行 Reset
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void reset() {
            long curSessionId = AnalyticsSessionInfo.sessionId;
            
            // Global Instances 不支持外部通过代码在任意时间加入新值，所以不需要做 timeStamp 检查
            globalInstances = new();
            
            // Global Data 在使用 setGlobalData 从外界向内部添加新值的时候会记录添加值的时间
            Dictionary<string, Object> lGlobalData = new();
            Dictionary<Object, long> lDataSession = new();
            foreach (var kvp in globalData) {
                if (!dataSession.TryGetValue(kvp.Value, out var sessionId)) {
                    Debug.LogWarning($"Global Data {kvp.Key} is missing valid Session Id, this data will not be included during reset!");
                    continue;
                }
                if (sessionId != curSessionId) continue;
                lGlobalData[kvp.Key] = kvp.Value;
                lDataSession[kvp.Value] = sessionId;
            }
            
            globalData = lGlobalData;
            dataSession = lDataSession;
        }

        public static T getGlobalInst<T>(string key) where T : MonoBehaviour {
            if (!globalInstances.ContainsKey(key)) return null;
            return (T)globalInstances[key];
        }

        public static T getGlobalData<T>(string key) where T : Object {
            if (!globalData.TryGetValue(key, out var value)) return null;
            if (dataSession.TryGetValue(value, out var sessionId) && sessionId != AnalyticsSessionInfo.sessionId) 
                Debug.LogError("Global Data " + key + " is from a previous session!");
            return (T)value;
        }
        
        public static void setGlobalData(string key, Object value) {
            globalData[key] = value;
            dataSession[value] = AnalyticsSessionInfo.sessionId;
        }

        [SerializeReference] [SerializeField] private List<CustomDataItem> dataStore = new();
        [SerializeField] private List<CustomDataRuntimeItem> runtimeDataStore = new();
        private Dictionary<string, CustomDataRuntimeItem> _runtimeDataDictionary = new();
        
        private Dictionary<string, CustomDataItem> _dataDictionary;

        void OnValidate() {
            rebuildDictionary();
        }

        private void Awake() {
            gameObject.getComponent<CustomData>();
            rebuildDictionary(); // Ensure dictionary is built on awake for runtime
            foreach (var cdi in _dataDictionary.Values) {
                if (cdi.type == typeof(GlobalInstDeclarer).ToString()) 
                    globalInstances[cdi.name] = cdi.getValue<GlobalInstDeclarer>().monoBehaviour;
                if (cdi.type == typeof(GlobalDataDeclarer).ToString()) {
                    var obj = cdi.getValue<GlobalDataDeclarer>().data;
                    globalData[cdi.name] = obj;
                    dataSession[obj] = AnalyticsSessionInfo.sessionId;
                }

                if (cdi.shouldCopyToRuntime) {
                    object obj = cdi.value;
                    if (obj is Unwrapper unwrapper) obj = unwrapper.unwrap();
                    setRuntimeData(cdi.name, obj);
                }
            }
        }

        public T getData<T>(string key) {
            if (_dataDictionary[key].value == null) return (T)(object)null;
            if (_dataDictionary[key].value is T)
                return _dataDictionary[key].getValue<T>();
            if (_dataDictionary[key].value is Unwrapper unwrapper) {
                var v = unwrapper.unwrap();
                return (T)v;
            }

            throw new InvalidEnumArgumentException("No valid data is found! Please check key and Generic Type!");
        }

        public bool hasData(string key) {
            if (!_dataDictionary.ContainsKey(key)) return false;
            return true;
        }
        public bool hasData(string key, string type) {
            if (!_dataDictionary.ContainsKey(key)) return false;
            if (_dataDictionary[key].type == type) return true;
            return false;
        }
        public bool hasData<T>(string key) {
            if (!_dataDictionary.ContainsKey(key)) return false;
            if (_dataDictionary[key].value is T)
                return true;
            if (_dataDictionary[key].value is Unwrapper unwrapper) {
                var v = unwrapper.unwrap();
                if (v is T) return true;
            }

            return false;
        }

        public bool hasRuntimeData(string key) {
            return _runtimeDataDictionary.ContainsKey(key);
        }
        public void setRuntimeData(string key, object value) {
            if (!hasRuntimeData(key)) {
                string type = "Undefined";
                if (value != null) type = value.GetType().ToString();
                var cdri = new CustomDataRuntimeItem(key, type, value);
                runtimeDataStore.Add(cdri);
                _runtimeDataDictionary[key] = cdri;
            }
            else {
                string type = _runtimeDataDictionary[key].type;
                if (value != null) type = value.GetType().ToString();
                _runtimeDataDictionary[key].reset(type, value);
            }
        }

        public T getRuntimeDataOrNull<T>(string key) {
            return getRuntimeDataOrDefault<T>(key, (T)(object)null);
        }
        public T getRuntimeDataOrDefault<T>(string key, T defaultValue) {
            if (hasRuntimeData(key)) return getRuntimeData<T>(key);
            return defaultValue;
        }
        public T getRuntimeData<T>(string key) {
            return (T)_runtimeDataDictionary[key].value;
        }

        public void rebuildDictionary() {
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
