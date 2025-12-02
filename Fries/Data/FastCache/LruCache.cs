using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Fries.Data.FastCache {
    public class LruCache<K, V> {
        public const int NULL = -1;
        
        private Node<K, V>[] nodes;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int allocNode(K newKey, V newValue, out bool isEvicted, out K keyEvicted) {
            int allocedNodeIndex;
            isEvicted = false;
            keyEvicted = default;
            
            // 如果空闲列表被消耗殆尽，则 Evict 链表中最后一个元素
            if (freeArrayHeadIndex == NULL) {
                isEvicted = true;
                keyEvicted = nodes[tailArrayIndex].key;
                allocedNodeIndex = tailArrayIndex;
            }
            // 不然的话，记录当前 空闲链表值，更新他，并返回刚才记录的结果
            else {
                allocedNodeIndex = freeArrayHeadIndex;
                freeArrayHeadIndex = nodes[freeArrayHeadIndex].nextNodeArrayIndex;
                nodes[allocedNodeIndex].prevNodeArrayIndex = NULL;
                nodes[allocedNodeIndex].nextNodeArrayIndex = NULL;
            }

            // 初始化 Node 值，避免用户阅读到垃圾
            nodes[allocedNodeIndex].value = default;
            nodes[allocedNodeIndex].key = newKey;
            bringNodeToTop(allocedNodeIndex);
            
            nodes[allocedNodeIndex].value = newValue;
            return allocedNodeIndex;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void bringNodeToTop(int allocedNodeIndex) {
            if (headArrayIndex == allocedNodeIndex) return;
            
            // 将 Node 的位置在 非空闲链表中 更新至第一位
            if (headArrayIndex == NULL) {
                headArrayIndex = allocedNodeIndex;
                tailArrayIndex = allocedNodeIndex;
                nodes[allocedNodeIndex].nextNodeArrayIndex = NULL;
                nodes[allocedNodeIndex].prevNodeArrayIndex = NULL;
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
                nodes[allocedNodeIndex].prevNodeArrayIndex = NULL;
                headArrayIndex = allocedNodeIndex;
            }
        }
        
        public void put(K key, V value) {
            // 如果 Dictionary 中，该 Key 已经存在，
            if (dictionary.TryGetValue(key, out var nodeIndex1)) {
                bringNodeToTop(nodeIndex1);
                nodes[nodeIndex1].value = value;
            }
            // 如果 Dictionary 中，该 Key 尚未存在，就直接创建它
            else {
                int nodeIndex = allocNode(key, value, out bool isEvicted, out K keyEvicted);
                if (isEvicted) dictionary.Remove(keyEvicted);
                dictionary[key] = nodeIndex;
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
            
            nodes = new Node<K, V>[capacity];
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