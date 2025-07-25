using System;
using System.Collections.Concurrent;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Fries {
    public class PrefabParameters {
        
        private static PrefabParameters prefabParameters;
        public static PrefabParameters inst() {
            if (prefabParameters == null) prefabParameters = new PrefabParameters();
            return prefabParameters;
        }
    
        private static int idCounter = 1;
        private const float xRandom = 327.234183f;
        private const float yRandom = 242.234626f;
        private const float zRandom = 197.645223f;

        private readonly ConcurrentDictionary<int, object[]> parameters;
    
        public PrefabParameters() {
            parameters = new ConcurrentDictionary<int, object[]>();
        }

        public static void setParameters(int instanceId, params object[] param) {
            PrefabParameters pp = inst();
            pp.parameters.TryAdd(instanceId, param);
        }

        public static void addParameter(int instanceId, object[] param) {
            PrefabParameters pp = inst();
            if (pp.parameters.ContainsKey(instanceId)) {
                pp.parameters.TryGetValue(instanceId, out object[] existingParams);
                object[] newParams = existingParams.Join(new object[] { param });
                pp.parameters.TryUpdate(instanceId, newParams, existingParams);
            }
            else pp.parameters.TryAdd(instanceId, param);
        }
        
        private object[] paramRegister = null;
        public static GameObject initPrefab(GameObject prefab, Transform parent, params object[] param) {
            PrefabParameters pp = inst();
            pp.paramRegister = param;
            GameObject go = GameObject.Instantiate(prefab, parent);
            pp.paramRegister = null;
            pp.parameters.TryAdd(go.GetInstanceID(), param);
            return go;
        }
    
        public static object[] getParameters(GameObject go) {
            PrefabParameters pp = inst();
            if (pp.paramRegister != null) return pp.paramRegister;
            pp.parameters.TryGetValue(go.GetInstanceID(), out object[] param);
            return param;
        }

        public static bool hasParameters(GameObject go) {
            PrefabParameters pp = inst();
            if (pp.paramRegister != null) return true;
            if (pp.parameters.ContainsKey(go.GetInstanceID())) return true;
            return false;
        }
    
    }

    public static class MonoBehaviourExt {
        public static object[] getParams(this MonoBehaviour mono) {
            return PrefabParameters.getParameters(mono.gameObject);
        }

        public static T getParam<T>(this MonoBehaviour mono, int index) {
            object obj = PrefabParameters.getParameters(mono.gameObject)[index];
            if (obj is T variable) 
                return variable;

            // 试图转换
            try { return (T)Convert.ChangeType(obj, typeof(T)); }
            catch (Exception ex) {
                throw new InvalidCastException($"Can not cast the value to type: {typeof(T)}", ex);
            }
        }

        public static bool hasParam(this MonoBehaviour mono) {
            return PrefabParameters.hasParameters(mono.gameObject);
        }
    }

    public static class GameObjectExt {
        public static GameObject instantiate(this GameObject gameObject, Transform parent, params object[] param) {
            return PrefabParameters.initPrefab(gameObject, parent, param);
        }
    }
}
