namespace Fries.Data.FastCache {
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    namespace Fries.Data.FastCache {
        public class FastLruCache<K, V> where K : struct, IEquatable<K> {
            public const int NULL = -1;

            // 核心数据结构：Entry 既是 LRU 链表节点，也是 Hash 链表节点
            // 结构体对齐：4(prev) + 4(next) + 4(hashNext) + 32(Key) + 1(Value) + Padding ≈ 48 bytes
            // 正好能塞进一个 CPU Cache Line (64 bytes)，这对性能极其有利！
            public struct Entry {
                public int lruPrev; // LRU 链表前驱
                public int lruNext; // LRU 链表后继
                public int hashNext; // Hash 冲突链表后继 (替代 Dictionary 的 buckets)
                public K key;
                public V value;
            }

            private int[] buckets; // 哈希桶，存储 Entry 的 Index
            private Entry[] entries; // 实际存储数据的数组

            private int headIndex;
            private int tailIndex;
            private int freeHeadIndex;

            private readonly int capacity;
            private readonly IEqualityComparer<K> comparer;

            public FastLruCache(int capacity) {
                if (capacity <= 1) throw new ArgumentException("Capacity must > 1");
                this.capacity = capacity;
                this.comparer = EqualityComparer<K>.Default;

                // 桶的大小建议设为 Capacity 的 1.5 倍左右以减少冲突，或者取素数
                // 这里为了简单和内存紧凑，设为 Capacity
                buckets = new int[capacity];
                entries = new Entry[capacity];

                Initialize();
            }

            // 重要优化：使用 'in' 关键字避免 32字节 的 struct 拷贝
            public void Put(in K key, V value) {
                // 1. 计算哈希桶
                int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                int bucketIndex = hashCode % buckets.Length;

                // 2. 查找是否已存在 (遍历 Hash 链表)
                int current = buckets[bucketIndex];
                while (current != NULL) {
                    if (comparer.Equals(entries[current].key, key)) {
                        // 命中：更新值，移到 LRU 头部
                        entries[current].value = value;
                        MoveToHead(current);
                        return;
                    }

                    current = entries[current].hashNext;
                }

                // 3. 不存在：分配新节点
                int entryIndex;
                if (freeHeadIndex != NULL) {
                    // 有空闲节点
                    entryIndex = freeHeadIndex;
                    freeHeadIndex = entries[entryIndex].lruNext; // free list 用 lruNext 串联
                }
                else {
                    // 满了，驱逐 LRU 尾部
                    entryIndex = tailIndex;
                    RemoveFromHashTable(entryIndex); // 从哈希表中移除旧 Key 的索引
                    // 注意：这里不需要 RemoveFromLRU，因为我们马上复用它并 MoveToHead
                }

                // 4. 写入新数据
                entries[entryIndex].key = key;
                entries[entryIndex].value = value;
                entries[entryIndex].hashNext = buckets[bucketIndex]; // 头插法插入 Hash 链表
                buckets[bucketIndex] = entryIndex;

                // 5. 维护 LRU
                // 如果是复用的尾节点，它已经在链表中，只需移动
                // 如果是新节点，它不在 LRU 链表中，需要插入
                if (entryIndex == tailIndex && freeHeadIndex == NULL) {
                    MoveToHead(entryIndex);
                }
                else {
                    AddToHead(entryIndex);
                }
            }

            public bool TryGetValue(in K key, out V value) {
                int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                int bucketIndex = hashCode % buckets.Length;

                int current = buckets[bucketIndex];
                while (current != NULL) {
                    // 这里的 Equals 是结构体比较，性能取决于 K 的实现
                    if (comparer.Equals(entries[current].key, key)) {
                        value = entries[current].value;
                        MoveToHead(current);
                        return true;
                    }

                    current = entries[current].hashNext;
                }

                value = default;
                return false;
            }

            // 强内联优化
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void MoveToHead(int index) {
                if (index == headIndex) return;

                // Detach
                int prev = entries[index].lruPrev;
                int next = entries[index].lruNext;

                if (prev != NULL) entries[prev].lruNext = next;
                if (next != NULL) entries[next].lruPrev = prev;
                else tailIndex = prev; // 是尾节点

                // Attach
                entries[index].lruNext = headIndex;
                entries[index].lruPrev = NULL;
                if (headIndex != NULL) entries[headIndex].lruPrev = index;
                headIndex = index;

                if (tailIndex == NULL) tailIndex = index; // 只有一个节点时
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void AddToHead(int index) {
                entries[index].lruNext = headIndex;
                entries[index].lruPrev = NULL;
                if (headIndex != NULL) entries[headIndex].lruPrev = index;
                headIndex = index;
                if (tailIndex == NULL) tailIndex = index;
            }

            // 当 LRU 驱逐节点时，必须将其从 Hash 链表中解绑
            // 这是去字典化后唯一的额外开销：需要重新 Hash 一次
            private void RemoveFromHashTable(int index) {
                ref Entry entryToRemove = ref entries[index];
                int hashCode = comparer.GetHashCode(entryToRemove.key) & 0x7FFFFFFF;
                int bucketIndex = hashCode % buckets.Length;

                int current = buckets[bucketIndex];
                int prev = NULL;

                while (current != NULL) {
                    if (current == index) {
                        // 找到了，断开链接
                        if (prev == NULL) {
                            // 是链表头
                            buckets[bucketIndex] = entries[current].hashNext;
                        }
                        else {
                            entries[prev].hashNext = entries[current].hashNext;
                        }

                        return;
                    }

                    prev = current;
                    current = entries[current].hashNext;
                }
            }

            public void Clear() {
                Initialize();
            }

            private void Initialize() {
                headIndex = NULL;
                tailIndex = NULL;
                freeHeadIndex = 0;

                // 重置桶
                Array.Clear(buckets, 0, buckets.Length);
                for (int i = 0; i < buckets.Length; i++) buckets[i] = NULL;

                // 重置 Entry 链表 (Free List)
                for (int i = 0; i < capacity - 1; i++) {
                    entries[i].lruNext = i + 1;
                    entries[i].lruPrev = NULL; // Debug 用，其实不需要
                    entries[i].hashNext = NULL; // 必须重置
                }

                entries[capacity - 1].lruNext = NULL;
                entries[capacity - 1].hashNext = NULL;
            }
        }
    }
}