using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fries.Pool {
    public abstract class APool<T> {
        private readonly DictList<T> inactives = new();
        private readonly DictList<T> active = new();
        public Func<T> generator { set; private get; } // 生成全新的对象
        public Action<T> resetter { set; protected get; } // 还原数值
        
        protected APool(Func<T> generator, int size) {
            this.generator = generator;
            
            for (int i = 0; i < size; i++) {
                T t = generator();
                inactives.Add(t);
            }
        }

        public void deactivateAll() {
            foreach (var t in active.ToList()) deactivate(t);
        }
        
        public void deactivate(T what) {
            if (!active.Contains(what)) return;
            
            deactivateCore(what);
            resetter(what);
            active.Remove(what);
            inactives.Add(what);
        }

        // 对于 GameObject 而言是将其不可视化
        protected abstract void deactivateCore(T what);

        public T activate() {
            if (inactives.Count == 0) 
                inactives.Add(generator());

            T what = inactives[0];
            activateCore(what);
            resetter(what);
            inactives.Remove(what);
            active.Add(what);

            return what;
        }

        // 对于 GameObject 而言是将其可视化
        protected abstract void activateCore(T what);

        public int activeSize() {
            return active.Count;
        }

        public int inactiveSize() {
            return inactives.Count;
        }

        public List<T> getActives() {
            List<T> list = new();
            foreach (var obj in active) 
                list.Add(obj);
            return list;
        }
        
    }
}