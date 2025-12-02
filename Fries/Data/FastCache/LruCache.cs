using System.Collections.Generic;

namespace Fries.Data.FastCache {
    public class LruCache<K, V> {
        public const int NULL = -1;
        
        private Node<V>[] nodes;
        private int headArrayIndex;
        private int tailArrayIndex;
        private int freeArrayHeadIndex;
        
        private Dictionary<K, int> dictionary;

        private int capacity = -1;
        public LruCache(int capacity) {
            if (capacity <= 1) throw new System.ArgumentException("Capacity must be greater than 1!");
            this.capacity = capacity;
            clear(capacity);
        }
        
        // 返回空闲 Node 的 Index
        private int allocNode() {
            int allocedNodeIndex;
            
            // 如果空闲列表被消耗殆尽，则 Evict 链表中最后一个元素
            if (freeArrayHeadIndex == NULL) 
                allocedNodeIndex = tailArrayIndex;
            // 不然的话，记录当前 空闲链表值，更新他，并返回刚才记录的结果
            else {
                allocedNodeIndex = freeArrayHeadIndex;
                freeArrayHeadIndex = nodes[freeArrayHeadIndex].nextNodeArrayIndex;
            }

            // 初始化 Node 值，避免用户阅读到垃圾
            nodes[allocedNodeIndex].value = default;
            bringNodeToTop(allocedNodeIndex);
            
            return allocedNodeIndex;
        }

        private void bringNodeToTop(int allocedNodeIndex) {
            if (headArrayIndex == allocedNodeIndex) return;
            
            nodes[allocedNodeIndex].prevNodeArrayIndex = NULL;
            // 将 Node 的位置在 非空闲链表中 更新至第一位
            if (headArrayIndex == NULL) {
                headArrayIndex = allocedNodeIndex;
                tailArrayIndex = allocedNodeIndex;
                nodes[allocedNodeIndex].nextNodeArrayIndex = NULL;
            }
            else {
                int oldPrev = nodes[allocedNodeIndex].prevNodeArrayIndex;
                int oldNext = nodes[allocedNodeIndex].nextNodeArrayIndex;
                if (oldPrev != NULL && oldNext != NULL) {
                    nodes[oldPrev].nextNodeArrayIndex = oldNext;
                    nodes[oldNext].prevNodeArrayIndex = oldPrev;
                } 
                else if (oldPrev != NULL) {
                    nodes[oldPrev].nextNodeArrayIndex = NULL;
                    tailArrayIndex = oldPrev;
                }
                
                nodes[headArrayIndex].prevNodeArrayIndex = allocedNodeIndex;
                nodes[allocedNodeIndex].nextNodeArrayIndex = headArrayIndex;
                headArrayIndex = allocedNodeIndex;
            }
        }
        
        public void put(K key, V value) {
            // 如果 Dictionary 中，该 Key 已经存在，
            if (dictionary.ContainsKey(key)) {
                int nodeIndex = dictionary[key];
                bringNodeToTop(nodeIndex);
                nodes[nodeIndex].value = value;
            }
            // 如果 Dictionary 中，该 Key 尚未存在，就直接创建它
            else {
                int nodeIndex = allocNode();
                dictionary[key] = nodeIndex;
                nodes[nodeIndex].value = value;
            }
        }
        
        public V get(K key) {
            if (!dictionary.TryGetValue(key, out int nodeIndex)) return default;
            bringNodeToTop(nodeIndex);
            return nodes[nodeIndex].value;
        }

        public bool tryGetValue(K key, out V value) {
            if (!dictionary.TryGetValue(key, out int nodeIndex)) {
                value = default;
                return false;
            }
            bringNodeToTop(nodeIndex);
            value = nodes[nodeIndex].value;
            return true;
        }

        public void clear(int capacity = -1) {
            if (capacity == -1) capacity = this.capacity;
            
            nodes = new Node<V>[capacity];
            headArrayIndex = NULL;
            tailArrayIndex = NULL;
            
            for (int i = 0; i < nodes.Length; i++) {
                nodes[i].value = default;
                nodes[i].prevNodeArrayIndex = i - 1;
                nodes[i].nextNodeArrayIndex = i + 1;
            }
            nodes[^1].nextNodeArrayIndex = NULL;
            freeArrayHeadIndex = 0;
            
            dictionary = new Dictionary<K, int>(capacity);
        }
    }
}