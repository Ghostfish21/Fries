using Fries.Inspector.TypeDrawer;
using UnityEngine;

namespace Fries {
    public class TestMonoBehaviour : MonoBehaviour {
        public void debugLog(string message) {
            Debug.Log(message);
        }

        public void debugLog1() {
            Debug.Log(1);
        }
    }
}