using UnityEngine;

namespace Fries {
    public class SetInactive : MonoBehaviour {
        public const int AWAKE = -2;
        public const int START = -1;
        
        public int timing = -2;
        private void Awake() {
            if (timing < AWAKE) timing = AWAKE;
            if (timing != AWAKE) return;
            gameObject.SetActive(false);
            Destroy(this);
        }
        
        private void Start() {
            if (timing != START) return;
            gameObject.SetActive(false);
            Destroy(this);
        }

        private int updateCount = 0;
        private void Update() {
            if (updateCount == timing) {
                gameObject.SetActive(false);
                Destroy(this);
            }
            updateCount++;
        }
    }
}