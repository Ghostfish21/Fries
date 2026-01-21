using System;
using System.Collections.Generic;

namespace Fries.Pool {
    public class HashSetPool<T> : APool<HashSet<T>> {
        public HashSetPool(int size) : base(()=>new HashSet<T>(), size) { }

        protected override void deactivateCore(HashSet<T> what) {
            what.Clear();
        }

        protected override void activateCore(HashSet<T> what) {
            what.Clear();
        }
    }
}