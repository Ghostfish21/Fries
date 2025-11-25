using Fries.Inspector.TypeDrawer;
using UnityEngine;

namespace Fries {
    public class TestMonoBehaviour : MonoBehaviour {
        [SerializeReference]
        public StaticMethodSelector resetter = new(t=>true, mi => {
            return true;
        });
        public void debugLog(string message) {
            Debug.Log(message);
        }

        public void debugLog1() {
            Debug.Log(1);
        }
    }
}