namespace Fries.Data.FastCache {
    public struct Node<K, V> {
        public K key;
        public V value;
        public int prevNodeArrayIndex;
        public int nextNodeArrayIndex;
    }
}