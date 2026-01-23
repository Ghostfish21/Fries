using System;
using System.Collections.Generic;

namespace Fries.Pool {
    public class SortedListPool<K,V> : APool<SortedList<K,V>> {
        public SortedListPool(int size) : base(() => new SortedList<K, V>(), size) {
        }

        protected override void deactivateCore(SortedList<K, V> what) {
            what.Clear();
        }

        protected override void activateCore(SortedList<K, V> what) {
            what.Clear();
        }
    }
}