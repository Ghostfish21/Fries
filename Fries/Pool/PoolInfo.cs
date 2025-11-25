using System;
using System.Collections.Generic;
using System.Reflection;
using Fries.Inspector;
using Fries.Inspector.TypeDrawer;
using UnityEngine;

namespace Fries.Pool {
    [Serializable]
    public class PoolInfo : MonoBehaviour {
        public GameObject prefab;
        [SerializeReference]
        public TypeWrapper type = new();
        public Type getType() {
            List<Type> types = this.type.getTypes(out _);
            Type ret = null;
            foreach (var type1 in types) {
                if (!type1.IsSubclassOf(typeof(MonoBehaviour))) continue;
                ret = type1;
                break;
            }
            return ret;
        }
        public int initialCapacity;
        [SerializeReference]
        public ResetterSelector resetter = new();
    }
}