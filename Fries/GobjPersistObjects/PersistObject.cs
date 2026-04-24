using System;
using System.Collections.Generic;
using System.Reflection;
using Fries.CompCache;
using Fries.PrefabParam;
using UnityEngine;

namespace Fries.GobjPersistObjects {
    [TypeTag]
    public class PersistObject : MonoBehaviour {
        // 这里的 UniqueName 由人工在 Inspector 中填入
        [SerializeField] private string uniqueName = "";
        public bool syncParent = false;
        public string GetUniqueName() => prefabInstUid + "_" + uniqueName;
        public long prefabInstUid { get; set; } = -1;
        public string prefabName { get; private set; }
        
        [SerializeField] private bool isPersistent = false;
        public bool IsPersistent() => isPersistent;
        private Action<PersistObject> onStart = null;
        
        protected virtual void Awake() {
            var parameters = gameObject.GetParams();
            if (parameters == null) return;
            isPersistent = (bool)parameters[0];
            onStart = (Action<PersistObject>)parameters[1];
        }

        protected virtual void Start() {
            if (onStart != null) onStart(this);
        }

        protected virtual void OnDestroy() { }

        internal void init(long prefabInstUid, string prefabName) {
            this.prefabInstUid = prefabInstUid;
            this.prefabName = prefabName;
        }

        public Dictionary<string, (bool, object)> Export() {
            Dictionary<string, (bool, object)> data;
            try { data = GetData(); }
            catch (Exception e) {
                Debug.LogError("Failed to export data! Exception: " + e);
                return null;
            }

            try { GetExtraData(data); }
            catch (Exception e) { Debug.LogError("Failed to export extra data! Exception: " + e); }

            return data;
        }
        public void Import(Dictionary<string, (bool, object)> data) {
            try { SetData(data); }
            catch (Exception e) { Debug.LogError("Failed to import data! Exception: " + e); }

            try { SetExtraData(data); } 
            catch (Exception e) { Debug.LogError("Failed to import extra data! Exception: " + e); }
        }
        
        public virtual Dictionary<string, (bool, object)> GetData() {
            Dictionary<string, (bool, object)> data = new();
            data["uniqueName"] = (false, GetUniqueName());
            data["prefabInstUid"] = (false, prefabInstUid);
            data["prefabName"] = (false, prefabName);
            if (syncParent) {
                bool hasParent = transform.parent && transform.parent.GetComponent<PersistObject>();
                data["hasParent"] = (false, hasParent);
                if (hasParent) {
                    data["parentPrefabInstUid"] = (false, transform.parent.GetComponent<PersistObject>().prefabInstUid);
                    data["localPosition"] = (false, transform.localPosition);
                    data["localRotation"] = (false, transform.localEulerAngles);
                }
            }
            data["position"] = (false, transform.position);
            data["rotation"] = (false, transform.eulerAngles);
            data["enabled"] = (false, enabled);
            data["active"] = (false, gameObject.activeSelf);
            return data;
        }
        
        public virtual void SetData(Dictionary<string, (bool, object)> data) {
            uniqueName = (string)data["uniqueName"].Item2;
            prefabInstUid = (long)data["prefabInstUid"].Item2;
            string prefix = prefabInstUid + "_";
            uniqueName = uniqueName.Substring(prefix.Length);
            
            prefabName = (string)data["prefabName"].Item2;

            if (syncParent) {
                bool hasParent = false;

                if (!data.TryGetValue("hasParent", out var value))
                    Debug.LogError($"Missing parent data for {gameObject.name}! This can happen when PersistObject doesn't have syncParent toggled on Exporting. But the syncParent was toggled on when importing.", gameObject);
                else hasParent = (bool) value.Item2;
                
                if (hasParent) {
                    long parentUid = (long)data["parentPrefabInstUid"].Item2;
                    GameObject parent = GpoManager.Inst.GetGobj(parentUid);
                    if (parent) {
                        transform.SetParent(parent.transform);
                        transform.localPosition = (Vector3)data["localPosition"].Item2;
                        transform.localEulerAngles = (Vector3)data["localRotation"].Item2;
                    }
                    else {
                        GpoManager.CreateOnLoadCompleteAction(() => {
                            GameObject parent1 = GpoManager.Inst.GetGobj(parentUid);
                            if (parent1) {
                                transform.SetParent(parent1.transform);
                                transform.localPosition = (Vector3)data["localPosition"].Item2;
                                transform.localEulerAngles = (Vector3)data["localRotation"].Item2;
                            }
                            else Debug.LogError($"Parent prefab instance '{parentUid}' is not found! This is an internal error!");
                        });
                    }
                }
                else {
                    transform.SetParent(null);
                    transform.position = (Vector3)data["position"].Item2;
                    transform.eulerAngles = (Vector3)data["rotation"].Item2;
                }
            }

            else {
                transform.position = (Vector3)data["position"].Item2;
                transform.eulerAngles = (Vector3)data["rotation"].Item2;
            }

            enabled = (bool)data["enabled"].Item2;
            gameObject.SetActive((bool)data["active"].Item2);
        }

        public virtual void GetExtraData(Dictionary<string, (bool, object)> data) { }
        public virtual void SetExtraData(Dictionary<string, (bool, object)> data) { }
        
        protected void _f<T>(T type, string key, Dictionary<string, (bool, object)> data) where T : PersistObject {
            FieldInfo fi = FieldInfoCache.get(typeof(T), key);
            
            Type t = fi.FieldType;
            if (t.IsGenericType) t = fi.FieldType.GetGenericTypeDefinition();
            
            object val = data[key].Item2;
            var preDeserializer = PreSerializeHelper.GetDeserializer(t);
            if (preDeserializer != null) val = preDeserializer(fi.FieldType, val);
            
            fi.SetValue(this, val);
        }
        protected void _i<T,E>(T type, List<E> elemTypeProvider, string key, Dictionary<string, (bool, object)> data) where T : PersistObject {
            List<E> list = new();
            
            FieldInfo fi = FieldInfoCache.get(typeof(T), key);

            foreach (var elem in (List<E>)data[key].Item2) 
                list.Add(elem);
            
            fi.SetValue(this, list);
        }
        
        protected void _g<T>(Dictionary<string, (bool, object)> data, string key, T value) {
            Type t = typeof(T);
            if (t.IsGenericType) t = t.GetGenericTypeDefinition();
            
            object val = value;
            var preSerializer = PreSerializeHelper.GetSerializer(t);
            if (preSerializer != null) val = preSerializer(value);
            
            data[key] = (false, val);
        }
        protected void _h<T>(Dictionary<string, (bool, object)> data, string key, List<T> value) {
            List<T> list = new(value);
            data[key] = (false, list);
        }
    }
}