using System;
using System.Collections.Generic;

namespace Fries.Pool {
    public class ListPool<T> : APool<List<T>> {
        public ListPool(int size) : base(() => new List<T>(), size) { }
        protected override void deactivateCore(List<T> what) {
            what.Clear();
        }
        protected override void activateCore(List<T> what) {
            what.Clear();
        }
    }
}