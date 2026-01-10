using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fries.PrefabParam {
    public static class Utils {
        public static void PassParams(this GameObject inst, int shortTermId) {
            if (PrefabParams.Singleton.shortTermParamsCount() == 0) return;

            var tuple = PrefabParams.Singleton.peekShortTermParams();
            if (shortTermId != tuple.paramsId) return;

            PrefabParams.Singleton.setLongTermParams(inst.GetInstanceID(), new List<object>(tuple.parameters));
        }

        public static GameObject Run(this ITuple tuple, Func<int, GameObject> action) {
            GameObject ret = null;

            List<object> objList = new();
            for (int i = 0; i < tuple.Length; i++)
                objList.Add(tuple[i]);

            int id = PrefabParams.getShortTermParamsId();
            PrefabParams.Singleton.pushShortTermParams(id, objList);
            
            try { ret = action?.Invoke(id); }
            catch (Exception e) { Debug.LogError($"Caught exception when running action: {e}"); }
            
            if (PrefabParams.Singleton.tryPopShortTermParams(id)) return ret;
            Debug.LogError(
                $"Detected corrupted stack in PrefabParams when trying to cleanup! The target short-term params is not found!");
            return null;
        }

        public static GameObject Run(this object[] tuple, Func<int, GameObject> action) {
            GameObject ret = null;

            List<object> objList = tuple.ToList();
            int id = PrefabParams.getShortTermParamsId();
            PrefabParams.Singleton.pushShortTermParams(id, objList);
            
            try { ret = action?.Invoke(id); }
            catch (Exception e) { Debug.LogError($"Caught exception when running action: {e}"); }
            
            if (PrefabParams.Singleton.tryPopShortTermParams(id)) return ret;
            Debug.LogError(
                $"Detected corrupted stack in PrefabParams when trying to cleanup! The target short-term params is not found!");
            return null;
        }

        public static void AddParams(this ITuple tuple, bool insertAtStart = false, int? instId = null) {
            if (PrefabParams.Singleton.shortTermParamsCount() == 0) {
                Debug.LogError("There is no available short-term parameters at this point!");
                return;
            }

            List<object> tupleList = null;
            if (insertAtStart) {
                tupleList = new List<object>();
                for (int i = 0; i < tuple.Length; i++)
                    tupleList.Add(tuple[i]);
            }

            if (instId == null) {
                var shortTerm = PrefabParams.Singleton.peekShortTermParams();
                if (insertAtStart) {
                    tupleList.AddRange(shortTerm.parameters);
                    shortTerm.parameters = tupleList;
                }
                else {
                    for (int i = 0; i < tuple.Length; i++)
                        shortTerm.parameters.Add(tuple[i]);
                }
            }
            else {
                List<object> parameters = PrefabParams.Singleton.getLongTermParams(instId.Value);
                parameters ??= new List<object>();
                if (insertAtStart) {
                    tupleList.AddRange(parameters);
                    PrefabParams.Singleton.setLongTermParams(instId.Value, tupleList);
                }
                else {
                    for (int i = 0; i < tuple.Length; i++)
                        parameters.Add(tuple[i]);
                    PrefabParams.Singleton.setLongTermParams(instId.Value, parameters);
                }
            }
        }

        public static GameObject InitPrefab(GameObject prefab, Transform parent, params object[] param) {
            return param.Run(shortTermId => {
                GameObject go = Object.Instantiate(prefab, parent);
                go.PassParams(shortTermId);
                return go;
            });
        }

        public static List<object> GetParams(this GameObject inst, PhaseEnum phase) {
            return PrefabParams.getParams(inst.GetInstanceID(), phase);
        }
        public static bool HasParams(this GameObject inst, PhaseEnum phase) {
            return PrefabParams.hasParams(inst.GetInstanceID(), phase);
        }
        public static List<object> GetParams(this GameObject inst, [CallerMemberName] string caller = null) {
            if (caller == "Awake") return inst.GetParams(PhaseEnum.Awake);
            if (caller == "Start") return inst.GetParams(PhaseEnum.Start);
            return null;
        }
        public static bool HasParams(this GameObject inst, [CallerMemberName] string caller = null) {
            if (caller == "Awake") return inst.HasParams(PhaseEnum.Awake);
            if (caller == "Start") return inst.HasParams(PhaseEnum.Start);
            return false; // bool 没法返回 null
        }
    }
}