using System;
using UnityEngine;

namespace Fries.CompCache {
    [TypeTag(true)]
    public class A : MonoBehaviour {
        public bool dontTest;
        public bool testClass;
        public bool testInterface;
        public bool a;
        public bool b;
        public bool c;
        public bool d;

        private void Awake() {}

        private void OnDestroy() { }

        private void FixedUpdate() {
            dontTest = gameObject.hasTag(typeof(DontTest));
            testClass = gameObject.hasTag(typeof(TestClass));
            testInterface = gameObject.hasTag(typeof(TestInterface));
            a = gameObject.hasTag(typeof(A));
            b = gameObject.hasTag(typeof(B));
            c = gameObject.hasTag(typeof(C));
            d = gameObject.hasTag(typeof(D));
        }
    }
}