# if UNITY_EDITOR
using UnityEditor;
# endif
using UnityEngine;

namespace Fries.GobjPersistObjects {
    [RequireComponent(typeof(PersistObject))]
    public class SceneGpoLoader : MonoBehaviour {
        [SerializeField] private string prefabName;
#if UNITY_EDITOR
        private void OnValidate() {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            prefabName = source ? source.name : gameObject.name;
        }
#endif
        
        private static long uidCounter = long.MinValue;
        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void reset() => uidCounter = long.MinValue;
        public static long GetCurrentUidCounter() => uidCounter;
        public static long GetNewUid() {
            long uid = uidCounter;
            uidCounter++;
            return uid;
        }
        
        public PersistObject po { get; private set; }
        
        private void Awake() {
            po = GetComponent<PersistObject>();
            long uid = uidCounter;
            uidCounter++;
            # if UNITY_EDITOR
            OnValidate();
            # endif
            po.init(uid, prefabName);
            GpoManager.Inst.Register(this);
        }
    }
}