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

        public static GameObject initPrefab(GameObject prefab, Transform parent, params object[] param) {
            GameObject go = GameObject.Instantiate(prefab, parent);
            PrefabParameters pp = inst();
            pp.parameters.TryAdd(go.GetInstanceID(), param);
            return go;
        }
        
        public static GameObject initNetworkPrefab(GameObject prefab, Transform parent, params object[] param) {
            GameObject go = GameObject.Instantiate(prefab, parent);
            PrefabParameters pp = inst();
            pp.parameters.TryAdd(go.GetInstanceID(), param);
            
            // 通过反射获取 NetworkObject 类型（注意：请根据实际情况修改程序集名称）
            Type networkObjectType = Type.GetType("Unity.Netcode.NetworkObject, Unity.Netcode");
            if (networkObjectType == null) {
                Debug.LogError("Unable to access Unity.Netcode.NetworkObject type!");
                return go;
            }
            // 获取预制体上挂载的 NetworkObject 组件
            Component netObj = go.GetComponent(networkObjectType);
            if (netObj == null) {
                Debug.LogError("No NetworkObject found on prefab, please inspect the prefab settings!");
                return go;
            }
            // 通过反射获取 Spawn 方法（无参数）
            MethodInfo spawnMethod = networkObjectType.GetMethod("Spawn", BindingFlags.Public | BindingFlags.Instance);
            if (spawnMethod == null) {
                Debug.LogError("Unable to get Spawn method through reflection!");
                return go;
            }
            // 调用 Spawn 方法
            spawnMethod.Invoke(netObj, null);
            
            return go;
        }
    
        public static object[] getParameters(GameObject go) {
            PrefabParameters pp = inst();
            pp.parameters.TryGetValue(go.GetInstanceID(), out object[] param);
            return param;
        }

        public static bool hasParameters(GameObject go) {
            PrefabParameters pp = inst();
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
