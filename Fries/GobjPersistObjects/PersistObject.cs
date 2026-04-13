using System;
using System.Collections.Generic;
using System.Reflection;
using Fries.PrefabParam;
using UnityEngine;

namespace Fries.GobjPersistObjects {
    public class PersistObject : MonoBehaviour {
        // 这里的 UniqueName 由人工在 Inspector 中填入
        [SerializeField] private string uniqueName = "";
        public string GetUniqueName() => prefabInstUid + "_" + uniqueName;
        public long prefabInstUid { get; set; } = -1;
        public string prefabName { get; private set; }
        
        [SerializeField] private bool isPersistent = false;
        public bool IsPersistent() => isPersistent;
        private Action<PersistObject> onStart = null;
        
        private void Awake() {
            var parameters = gameObject.GetParams();
            if (parameters == null) return;
            isPersistent = (bool)parameters[0];
            onStart = (Action<PersistObject>)parameters[1];
        }

        private void Start() {
            if (onStart != null) onStart(this);
        }

        internal void init(long prefabInstUid, string prefabName) {
            this.prefabInstUid = prefabInstUid;
            this.prefabName = prefabName;
        }
        
        public virtual Dictionary<string, (bool, object)> GetData() {
            Dictionary<string, (bool, object)> data = new();
            data["uniqueName"] = (false, GetUniqueName());
            data["prefabInstUid"] = (false, prefabInstUid);
            data["prefabName"] = (false, prefabName);
            // NOTE 以后如果要添加设置父级的流程，这里需要更改，因为涉及到非零零父级后会出现本地坐标
            data["position"] = (false, transform.position);
            data["rotation"] = (false, transform.eulerAngles);
            data["enabled"] = (false, enabled);
            data["active"] = (false, gameObject.activeSelf);
            GetExtraData(data);
            return data;
        }
        
        public virtual void SetData(Dictionary<string, (bool, object)> data) {
            uniqueName = (string)data["uniqueName"].Item2;
            prefabInstUid = (long)data["prefabInstUid"].Item2;
            string prefix = prefabInstUid + "_";
            uniqueName = uniqueName.Substring(prefix.Length);
            
            prefabName = (string)data["prefabName"].Item2;
            transform.position = (Vector3)data["position"].Item2;
            transform.eulerAngles = (Vector3)data["rotation"].Item2;
            
            enabled = (bool)data["enabled"].Item2;
            gameObject.SetActive((bool)data["active"].Item2);
            SetExtraData(data);
        }

        public virtual void GetExtraData(Dictionary<string, (bool, object)> data) { }
        public virtual void SetExtraData(Dictionary<string, (bool, object)> data) { }
        
        protected void _f<T>(T type, string key, Dictionary<string, (bool, object)> data) where T : PersistObject {
            FieldInfo fi = FieldInfoCache.get(typeof(T), key);
            fi.SetValue(this, data[key].Item2);
        }
        protected void _g<T>(Dictionary<string, (bool, object)> data, string key, T value) {
            data[key] = (false, value);
        }
        protected void _h<T>(Dictionary<string, (bool, object)> data, string key, List<T> value) {
            List<T> list = new(value);
            data[key] = (false, list);
        }
    }
}