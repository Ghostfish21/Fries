using System;
using System.Collections.Generic;

namespace Fries.Pool {
    public class DictionaryPool<K,V> : APool<Dictionary<K,V>> {
        public DictionaryPool(int size) : base(() => new Dictionary<K, V>(), size) {
        }

        protected override void deactivateCore(Dictionary<K, V> what) {
            what.Clear();
        }

        protected override void activateCore(Dictionary<K, V> what) {
            what.Clear();
        }
    }
}