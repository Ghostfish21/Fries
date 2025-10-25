using System;
using UnityEngine;

namespace Fries.Pool {
    public class CompPool<T> : APool<T> where T : Component {
        private Transform root;
        
        public CompPool(Action<T> compSetup, Action<T> resetter, Transform root, GameObject prefab, int size = 5) : base(() => {
            T comp = GameObject.Instantiate(prefab, root).GetComponent<T>();
            comp.gameObject.SetActive(false);
            compSetup(comp);
            return comp;
        }, size) {
            this.root = root;
            this.resetter = resetter;
        }
        
        public CompPool(Action<T> resetter, Transform root, GameObject prefab, int size = 5) : base(() => {
            T comp = GameObject.Instantiate(prefab, root).GetComponent<T>();
            comp.gameObject.SetActive(false);
            return comp;
        }, size) {
            this.root = root;
            this.resetter = resetter;
        }
        
        public CompPool(Transform root, GameObject prefab, int size = 5) : base(() => {
            T comp = GameObject.Instantiate(prefab, root).GetComponent<T>();
            comp.gameObject.SetActive(false);
            return comp;
        }, size) {
            this.root = root;
            this.resetter = _ => {};
        }

        protected override void deactivateCore(T what) {
            what.gameObject.SetActive(false);
            what.transform.SetParent(root);
        }

        protected override void activateCore(T what) {
            what.gameObject.SetActive(true);
        }
    }
}