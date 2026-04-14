using System;
using System.Collections.Generic;
using System.Linq;
using Fries.PrefabParam;
using UnityEngine;

namespace Fries.GobjPersistObjects {
    [DefaultExecutionOrder(-1000)]
    public class GpoManager : MonoBehaviour {
        private static PersistObjectLoadOperation LoadOperation;
        public static void StartLoadOperation() => LoadOperation = new PersistObjectLoadOperation();
        public static void StopLoadOperation() {
            LoadOperation.Close();
            LoadOperation = null;
        }
        public static void CreateOnLoadCompleteAction(Action action) {
            if (LoadOperation == null) 
                throw new InvalidOperationException("LoadOperation is not initialized! Please call GpoManager.StartLoadOperation() before importing any gpo data!");
            
            LoadOperation.actions.Add(action);
        }
        
        # region 单例
        private static GpoManager _inst;
        public static GpoManager Inst => _inst;

        private void Awake() {
            if (_inst) {
                Destroy(gameObject);
                return;
            }
            _inst = this;
            prefabMapping.Rebuild();
            DontDestroyOnLoad(gameObject);
        }
        # endregion

        # region Prefab 路径映射
        [SerializeField] private PrefabMapping prefabMapping;
        public PrefabMapping GetPrefabMapping() => prefabMapping;
        # endregion
        
        # region 数据存储结构与物体构造
        private Dictionary<long, GameObject> uid2Gobj = new();
        private Dictionary<GameObject, long> gobj2Uid = new();
        private Dictionary<string, PersistObject> uniqueNames = new();
        private Dictionary<GameObject, Dictionary<string, PersistObject>> persistObjMap = new();

        public GameObject GetGobj(long prefabInstUid) => uid2Gobj[prefabInstUid];
        public PersistObject GetPersistObject(string uniqueName) => uniqueNames[uniqueName];
        public T GetPersistObject<T>(string uniqueName) where T : PersistObject => (T)uniqueNames[uniqueName];
        
        public void ResetData() {
            uid2Gobj.Clear();
            gobj2Uid.Clear();
            persistObjMap.Clear();
            uniqueNames.Clear();
        }
        
        public PersistObject CreatePersistObject(Dictionary<string, (bool, object)> dataDict) {
            string uniqueName = (string)dataDict["uniqueName"].Item2;
            string prefabName = (string)dataDict["prefabName"].Item2;
            long prefabUid = (long)dataDict["prefabInstUid"].Item2;
            if (!uniqueNames.ContainsKey(uniqueName) || !uniqueNames[uniqueName]) 
                if (!Create(prefabUid, prefabName)) return null;
            PersistObject po = persistObjMap[uid2Gobj[prefabUid]][uniqueName];
            po.Import(dataDict);
            return po;
        }

        public void Register(SceneGpoLoader sgpo) {
            uid2Gobj[sgpo.po.prefabInstUid] = sgpo.po.gameObject;
            gobj2Uid[sgpo.po.gameObject] = sgpo.po.prefabInstUid;
            Dictionary<string, PersistObject> pobjs = new();
            foreach (PersistObject pObj in sgpo.po.gameObject.GetComponentsInChildren<PersistObject>()) {
                pObj.init(sgpo.po.prefabInstUid, sgpo.po.prefabName);
                pobjs[pObj.GetUniqueName()] = pObj;
                uniqueNames[pObj.GetUniqueName()] = pObj;
            }
            persistObjMap[sgpo.po.gameObject] = pobjs;
        }
        
        public GameObject Create(long prefabUid, string prefabName, Transform parent = null, Vector3? position = null, 
            Vector3? eulerAngles = null, bool isWorldSpace = false, Action<PersistObject> onStart = null, bool isPersistent = true) {
            
            bool result = GetPrefabMapping().TryGetPath(prefabName, out string prefabPath);
            if (!result) {
                Debug.LogError($"Prefab '{prefabName}' is not found in GPO Prefab Mapping!");
                return null;
            }
            
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (!prefab) {
                Debug.LogError($"GOBJ Prefab '{prefabPath}' is not found! PrefabName: {prefabName}");
                return null;
            }

            if (position != null || eulerAngles != null || parent) {
                onStart ??= _ => { };
                onStart += inst => {
                    if (parent) inst.transform.SetParent(parent);
                    if (isWorldSpace) {
                        if (position != null) inst.transform.position = position.Value;
                        if (eulerAngles != null) inst.transform.eulerAngles = eulerAngles.Value;
                    } else {
                        if (position != null) inst.transform.localPosition = position.Value;
                        if (eulerAngles != null) inst.transform.localEulerAngles = eulerAngles.Value;
                    }
                };
            }
            
            GameObject gObj = (isPersistent, onStart).Run(shortTermId => {
                GameObject go = Instantiate(prefab, parent);
                PersistObject gpo = go.GetComponent<PersistObject>();
                gpo.init(prefabUid, prefabName);
                go.PassParams(shortTermId);
                return go;
            });
            
            uid2Gobj[prefabUid] = gObj;
            gobj2Uid[gObj] = prefabUid;
            Dictionary<string, PersistObject> pobjs = new();
            foreach (PersistObject pObj in gObj.GetComponentsInChildren<PersistObject>()) {
                pObj.init(prefabUid, prefabName);
                pobjs[pObj.GetUniqueName()] = pObj;
                uniqueNames[pObj.GetUniqueName()] = pObj;
            }
            persistObjMap[gObj] = pobjs;
            
            return gObj;
        }

        public void Delete(long prefabUid) {
            if (!uid2Gobj.TryGetValue(prefabUid, out var gObj)) return;
            Destroy(gObj);
            uid2Gobj.Remove(prefabUid);
            gobj2Uid.Remove(gObj);
            persistObjMap.Remove(gObj);
            foreach (var pObj in gObj.GetComponentsInChildren<PersistObject>()) 
                uniqueNames.Remove(pObj.GetUniqueName());
        }
        # endregion
        
        # region 载入与导出的基础方法

        public Dictionary<string, Dictionary<string, (bool, object)>> ExportData() {
            Dictionary<string, Dictionary<string, (bool, object)>> data = new();
            
            foreach (var gobj in persistObjMap.Keys) {
                if (!gobj) continue;
                Dictionary<string, PersistObject> pobjs = persistObjMap[gobj];
                foreach (var kvp in pobjs) {
                    if (!kvp.Value.IsPersistent()) continue;
                    var dataDict = kvp.Value.Export();
                    if (dataDict == null) continue;
                    data[kvp.Key] = dataDict;
                }
            }
            
            return data;
        }

        public void ImportData(Dictionary<string, Dictionary<string, (bool, object)>> data) {
            StartLoadOperation();
            
            HashSet<long> prefabInstUids = new();
            
            foreach (var kvp in data) {
                Dictionary<string, (bool, object)> dataDict = kvp.Value;
                if (dataDict == null) continue;
                string uniqueName = (string)dataDict["uniqueName"].Item2;

                long prefabInstUid = (long)dataDict["prefabInstUid"].Item2;
                prefabInstUids.Add(prefabInstUid);
                // 检查如果该 prefab inst 已经存在，就给现有的物体更新数值
                // 如果该 prefab inst 不存在，就新建它然后更新数值
                if (!uid2Gobj.TryGetValue(prefabInstUid, out var gobj) || !gobj) {
                    gobj = Create(prefabInstUid, (string)dataDict["prefabName"].Item2);
                    if (!gobj) continue;
                }

                Dictionary<string, PersistObject> pobjs = persistObjMap[gobj];
                PersistObject pobj = pobjs[uniqueName];
                pobj.Import(dataDict);
            }

            foreach (var uid in uid2Gobj.Keys.ToList()) 
                if (!prefabInstUids.Contains(uid)) Delete(uid);
            
            StopLoadOperation();
        }
        
        # endregion
        
        # region 存档
        [SerializeField] private bool isSaveEnabled = true;
        public string saveName = "Untitled";
        private GobjCsvSave csvSave;

        public void LoadFromDisk() {
            if (!isSaveEnabled) return; 
            csvSave = new GobjCsvSave(saveName);
        }
        
        public void LoadScene() {
            if (!isSaveEnabled) return;
            csvSave.LoadScene(this);
        }

        public void SaveToSave() {
            if (!isSaveEnabled) return;
            Dictionary<string, Dictionary<string, (bool, object)>> saveData = ExportData();
            csvSave.Reset();
            foreach (var kvp in saveData) csvSave.PutEntry(kvp.Key, kvp.Value);
        }

        public void SaveToDisk() {
            if (!isSaveEnabled) return;
            csvSave.Flush();
        }

        private void OnApplicationQuit() {
            if (!isSaveEnabled) return;
            SaveToSave();
            SaveToDisk();
        }
        # endregion
    }
}