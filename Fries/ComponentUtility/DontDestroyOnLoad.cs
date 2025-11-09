using System;
using UnityEngine;

namespace Fries {
    public class DontDestroyOnLoad : MonoBehaviour {
        private void Awake() {
            DontDestroyOnLoad(gameObject);
            Destroy(this);
        }
    }
}