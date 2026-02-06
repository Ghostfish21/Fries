using System;
using System.Collections.Generic;
using Fries.Data;

namespace Fries.Pool {
    public class ListSetPool<T> : APool<ListSet<T>> {
        public ListSetPool(int size) : base(()=>new ListSet<T>(), size) { }

        protected override void deactivateCore(ListSet<T> what) {
            what.Clear();
        }

        protected override void activateCore(ListSet<T> what) {
            what.Clear();
        }
    }
}