using System.Reflection;

namespace Fries.Pool {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public struct PoolActions {
        public _Pool pool;
        public Action<object> deactivate;
        public Action deactivateAll;
        public Func<object> activate;
        public Func<List<object>> getActives;
        public Func<int> activeSize;
        public Func<int> inactiveSize;
        public Action<Action<object>> resetter;
    }

    public class EverythingPool : MonoBehaviour {
        private readonly Dictionary<Type, PoolActions> poolActions = new();

        public List<PoolInfo> poolInfos;

        private void Awake() {
            foreach (var poolInfo in poolInfos) {
                Transform poolRoot = poolInfo.transform;
                GameObject poolGobjPrefab = poolInfo.prefab;
                int poolSize = poolInfo.initialCapacity;
                Type type = poolInfo.getType();
                if (type == null) {
                    Debug.LogError($"Type of pool {poolInfo.name} is null! This can due to PoolInfo's type field isn't set, or the target script contains no MonoBehaviour type!");
                    return;
                }
                
                _Pool pool = poolGobjPrefab.toPool(type, poolRoot, poolSize);
                poolActions[type] = new PoolActions {
                    pool = pool,
                    deactivate = mono => pool._deactivate(mono),
                    deactivateAll = pool._deactivateAll,
                    activate = () => pool._activate(),
                    getActives = () => pool._getActives(),
                    activeSize = pool._activeSize,
                    inactiveSize = pool._inactiveSize,
                    resetter = t => pool.setResetter(t)
                };

                MethodInfo mi = poolInfo.resetter.getSelectedMethod();
                if (mi == null) continue;
                poolActions[type].resetter(resetter);
                continue;
                void resetter(object o) => mi.Invoke(null, new[] { o });
            }
        }

        public T activate<T>() where T : MonoBehaviour {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                return (T)poolAction.activate();

            Debug.LogError($"Pool for type {typeof(T)} not found!");
            return null;
        }

        public List<T> getActives<T>() where T : Component {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                return poolAction.getActives().Cast<T>().ToList();

            Debug.LogError($"Pool for type {typeof(T)} not found!");
            return null;
        }

        public void deactivate<T>(T obj) where T : MonoBehaviour {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                poolAction.deactivate(obj);
            else Debug.LogError($"Pool for type {typeof(T)} not found!");
        }

        public void deactivateAll<T>() where T : MonoBehaviour {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                poolAction.deactivateAll();
            else Debug.LogError($"Pool for type {typeof(T)} not found!");
        }

        public void setResetter<T>(Action<T> resetter) where T : MonoBehaviour {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                poolAction.resetter(t => {
                    if (t is T t1) resetter(t1);
                    else Debug.LogError($"Resetter for type {t.GetType()} cannot cast into {typeof(T)}!");
                });
            
            else Debug.LogError($"Pool for type {typeof(T)} not found!");
        }

        public int activeSize<T>() where T : MonoBehaviour {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                return poolAction.activeSize();

            Debug.LogError($"Pool for type {typeof(T)} not found!");
            return -1;
        }
        
        public int inactiveSize<T>() where T : MonoBehaviour {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                return poolAction.inactiveSize();
            
            Debug.LogError($"Pool for type {typeof(T)} not found!");
            return -1;
        }
    }
}