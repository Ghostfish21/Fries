using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fries {
    [RequireComponent(typeof(Collider))]
    public class TriggerData : MonoBehaviour {
        public string checkTag;
        public List<GameObject> objects = new();
        private List<GameObject> checkingList = new();

        private bool shouldCheckObjects = false;
        private bool checkFlag = false;

        private void Awake() {
            TaskPerformer.TaskPerformer.inst()
                .scheduleRepeatingTask((Action)(() => { shouldCheckObjects = true; }), 2f);
        }

        private void OnTriggerEnter(Collider other) {
            if (!other.gameObject.hasTag(checkTag)) return;
            objects.Add(other.gameObject);
        }

        private void OnTriggerStay(Collider other) {
            if (!checkFlag) return;
            if (!other.gameObject.hasTag(checkTag)) return;
            checkingList.Add(other.gameObject);
        }

        private void OnTriggerExit(Collider other) {
            if (!other.gameObject.hasTag(checkTag)) return;
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