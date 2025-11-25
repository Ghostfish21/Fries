using System;
using System.Reflection;
using Fries.Inspector;
using Fries.Inspector.TypeDrawer;
using UnityEngine;

namespace Fries.Pool {
    [Serializable]
    public class PoolInfo : MonoBehaviour {
        public GameObject prefab;
        public MonoBehaviour type;
        public int initialCapacity;
        [SerializeReference]
        public StaticMethodSelector resetter = new(_ => true, mi => {
            var attr = mi.GetCustomAttribute(typeof(ResetterAttribute), false);
            if (attr != null) return true;
            return false;
        });
    }
}