using System;
using UnityEngine;

namespace Fries.GobjPersistObjects {
    [RequireComponent(typeof(PersistObject))]
    public class SceneGpoLoader : MonoBehaviour {
        private static long uidCounter = long.MinValue;
        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void reset() => uidCounter = long.MinValue;
        public static long GetCurrentUidCounter() => uidCounter;
        
        public PersistObject po { get; private set; }
        
        private void Awake() {
            po = GetComponent<PersistObject>();
            long uid = uidCounter;
            uidCounter++;
            string prefabName = po.gameObject.name;
            po.init(uid, prefabName);
            GpoManager.Inst.Register(this);
        }
    }
}