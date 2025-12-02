namespace Fries.Data.FastCache {
    public struct Node<V> {
        public V value;
        public int prevNodeArrayIndex;
        public int nextNodeArrayIndex;
    }
}