using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fries.PhysicsFunctions {
    public static class OverlapBoxNonAlloc {
        // 池子大小 -> 桶
        private static Dictionary<int, Stack<Collider[]>> resultArrayPool = new();

        private static Collider[] getResArray(int capacity) {
            if (!resultArrayPool.TryGetValue(capacity, out var pool)) {
                pool = new Stack<Collider[]>();
                resultArrayPool[capacity] = pool;
            }
            if (pool.Count == 0) pool.Push(new Collider[capacity]);
            return pool.Pop();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void initialize() => resultArrayPool = new();

        public static int Try(Vector3 center, Vector3 halfExtents, Quaternion rotation, 
            out Collider[] result, int capacity = 4, int layerMask = Physics.AllLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, int maxCapacity = 1024,
            bool sortedCloseToFar = false, Vector3 sortPos = default
        ) {
            if (capacity < 1) capacity = 1;
            if (capacity >= maxCapacity) capacity = maxCapacity;

            int cap = capacity;
            var arr = getResArray(cap);
            int count;

            while (true) {
                count = Physics.OverlapBoxNonAlloc(
                    center, halfExtents, arr, rotation, 
                    layerMask, queryTriggerInteraction);
                
                if (count < cap) break;

                // 可能被截断 则扩容重试
                if (cap >= maxCapacity) break;
                
                Return(arr);

                cap = Mathf.Min(cap << 1, maxCapacity);
                arr = getResArray(cap);
            }

            result = arr;
            if (sortedCloseToFar) result.sortByDistanceTo(count, sortPos);
            return count;
        }

        public static void Return(Collider[] result) {
            if (result == null) return;

            int cap = result.Length;
            if (cap <= 0) return;

            if (!resultArrayPool.TryGetValue(cap, out var pool)) {
                pool = new Stack<Collider[]>();
                resultArrayPool[cap] = pool;
            }

            pool.Push(result);
        }
        
        private sealed class HitToPosComparer : IComparer<Collider> {
            public Vector3 pos;

            public int Compare(Collider a, Collider b) {
                if (!a) return b ? 1 : 0;
                if (!b) return -1;

                float da = (a.transform.position - pos).sqrMagnitude;
                float db = (b.transform.position - pos).sqrMagnitude;

                // Array.Sort 需要 int：-1 / 0 / 1
                return da < db ? -1 : da > db ? 1 : 0;
            }
        }

        private static readonly HitToPosComparer comparer = new();

        private static void sortByDistanceTo(this Collider[] hits, int count, Vector3 pos) {
            if (hits == null || count <= 1) return;
            count = Mathf.Min(count, hits.Length);
            comparer.pos = pos;
            Array.Sort(hits, 0, count, comparer);
        }
    }
}