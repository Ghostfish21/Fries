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

        private void registerInPoolActions(Type type, _Pool pool) {
            poolActions[type] = new PoolActions {
                pool = pool,
                deactivate = pool._deactivate,
                deactivateAll = pool._deactivateAll,
                activate = pool._activate,
                getActives = pool._getActives,
                activeSize = pool._activeSize,
                inactiveSize = pool._inactiveSize,
                resetter = pool.setResetter
            };
        }
        
        private void Awake() {
            createPoolCreators();
            
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
                registerInPoolActions(type, pool);

                MethodInfo mi = poolInfo.resetter.getSelectedMethod();
                if (mi == null) continue;
                poolActions[type].resetter(resetter);
                continue;
                void resetter(object o) => mi.Invoke(null, new[] { o });
            }
        }
        
        # region MonoBehaviour 池区
        public List<PoolInfo> poolInfos;
        
        public T Activate<T>() where T : MonoBehaviour {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                return (T)poolAction.activate();

            Debug.LogError($"Pool for type {typeof(T)} not found!");
            return null;
        }

        public List<T> GetActives<T>() where T : Component {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                return poolAction.getActives().Cast<T>().ToList();

            Debug.LogError($"Pool for type {typeof(T)} not found!");
            return null;
        }

        public void Deactivate<T>(T obj) where T : MonoBehaviour {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                poolAction.deactivate(obj);
            else Debug.LogError($"Pool for type {typeof(T)} not found!");
        }

        public void DeactivateAll<T>() where T : MonoBehaviour {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                poolAction.deactivateAll();
            else Debug.LogError($"Pool for type {typeof(T)} not found!");
        }

        public void SetResetter<T>(Action<T> resetter) where T : MonoBehaviour {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                poolAction.resetter(t => {
                    if (t is T t1) resetter(t1);
                    else Debug.LogError($"Resetter for type {t.GetType()} cannot cast into {typeof(T)}!");
                });
            
            else Debug.LogError($"Pool for type {typeof(T)} not found!");
        }

        public int ActiveSize<T>() where T : MonoBehaviour {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                return poolAction.activeSize();

            Debug.LogError($"Pool for type {typeof(T)} not found!");
            return -1;
        }
        
        public int InactiveSize<T>() where T : MonoBehaviour {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                return poolAction.inactiveSize();
            
            Debug.LogError($"Pool for type {typeof(T)} not found!");
            return -1;
        }
        # endregion
        
        # region System.Object 池区
        private Dictionary<Type, Func<Type[], _Pool>> poolCreators = new();

        private void assetTypeArrayLength(Type[] typeArray, int length) {
            if (typeArray.Length != length) throw new ArgumentException($"TypeArray's length must be {length}");
        }
        private void createPoolCreators() {
            poolCreators[typeof(List<>)] = typeArray => {
                assetTypeArrayLength(typeArray, 1);
                Type listPoolType = typeof(ListPool<>).MakeGenericType(typeArray[0]);
                return (_Pool)Activator.CreateInstance(listPoolType, 5);
            };
            poolCreators[typeof(HashSet<>)] = typeArray => {
                assetTypeArrayLength(typeArray, 1);
                Type hashSetPool = typeof(HashSet<>).MakeGenericType(typeArray[0]);
                return (_Pool)Activator.CreateInstance(hashSetPool, 5);
            };
            poolCreators[typeof(Dictionary<,>)] = typeArray => {
                assetTypeArrayLength(typeArray, 2);
                Type dictionaryPool = typeof(Dictionary<,>).MakeGenericType(typeArray[0], typeArray[1]);
                return (_Pool)Activator.CreateInstance(dictionaryPool, 5);
            };
        }

        private void initObjectPool(Type closedType) {
            if (!closedType.IsGenericType) 
                throw new ArgumentException($"Type {closedType.Name} must be a generic type!");
            if (closedType.ContainsGenericParameters)
                throw new ArgumentException($"Type {closedType.Name} must contain no open generic parameters!");
            
            Type genericTypeDef = closedType.GetGenericTypeDefinition();
            if (!poolCreators.TryGetValue(genericTypeDef, out var creator))
                throw new ArgumentException($"Type {genericTypeDef} is unsupported!");
            
            _Pool pool = creator(closedType.GetGenericArguments());
            registerInPoolActions(closedType, pool);
        }
        
        public T ActivateObject<T>() {
            Type t = typeof(T);
            if (poolActions.TryGetValue(t, out PoolActions poolAction))
                return (T)poolAction.activate();

            initObjectPool(t);
            return (T)poolActions[t].activate();
        }

        public List<T> GetActiveObjects<T>() {
            Type t = typeof(T);
            if (poolActions.TryGetValue(t, out PoolActions poolAction))
                return poolAction.getActives().Cast<T>().ToList();

            initObjectPool(t);
            return poolActions[t].getActives().Cast<T>().ToList();
        }

        public void DeactivateObject<T>(T obj) {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                poolAction.deactivate(obj);
            else Debug.LogError($"Pool for type {typeof(T)} not found!");
        }

        public void DeactivateAllObjects<T>() {
            if (poolActions.TryGetValue(typeof(T), out PoolActions poolAction))
                poolAction.deactivateAll();
            else Debug.LogError($"Pool for type {typeof(T)} not found!");
        }

        public void SetObjectResetter<T>(Action<T> resetter) {
            Type t = typeof(T);
            if (poolActions.TryGetValue(t, out PoolActions poolAction))
                poolAction.resetter(t => {
                    if (t is T t1) resetter(t1);
                    else Debug.LogError($"Resetter for type {t.GetType()} cannot cast into {t}!");
                });

            initObjectPool(t);
            poolActions[t].resetter(t1 => {
                if (t1 is T t2) resetter(t2);
                else Debug.LogError($"Resetter for type {t1.GetType()} cannot cast into {t}!");
            });
        }

        public int ObjectActiveSize<T>() {
            Type t = typeof(T);
            if (poolActions.TryGetValue(t, out PoolActions poolAction))
                return poolAction.activeSize();

            initObjectPool(t);
            return poolActions[t].activeSize();
        }
        
        public int ObjectInactiveSize<T>() {
            Type t = typeof(T);
            if (poolActions.TryGetValue(t, out PoolActions poolAction))
                return poolAction.inactiveSize();
            
            initObjectPool(t);
            return poolActions[t].inactiveSize();
        }
        # endregion
    }
}