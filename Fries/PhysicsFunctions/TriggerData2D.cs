using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fries.PhysicsFunctions {
    [RequireComponent(typeof(Collider2D))]
    public class TriggerData2D : MonoBehaviour {
        public float refreshRate = 2f;
        
        public List<string> checkTags;
        public List<GameObject> objects = new();
        private List<GameObject> checkingList = new();

        private bool shouldCheckObjects = false;
        private bool checkFlag = false;

        private void Awake() {
            TaskPerformer.TaskPerformer.inst()
                .scheduleRepeatingTask((Action)(() => { shouldCheckObjects = true; }), refreshRate);
        }
        

        private void OnTriggerEnter2D(Collider2D other) {
            if (!checkTags.Any(other.gameObject.hasTag)) return;
            objects.Add(other.gameObject);
        }

        private void OnTriggerStay2D(Collider2D other) {
            if (!checkFlag) return;
            if (!checkTags.Any(other.gameObject.hasTag)) return;
            checkingList.Add(other.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!checkTags.Any(other.gameObject.hasTag)) return;
            objects.Remove(other.gameObject);
        }

        private void FixedUpdate() {
            if (shouldCheckObjects) {
                checkFlag = true;
                shouldCheckObjects = false;
            }

            else if (checkFlag) {
                checkFlag = false;
                objects = checkingList;
                checkingList = new List<GameObject>();
            }
        }
    }
}