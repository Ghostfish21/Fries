using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fries.PhysicsFunctions {
    public static class RaycastNonAlloc {
        // 池子大小 -> 桶
        private static Dictionary<int, Stack<RaycastHit[]>> resultArrayPool = new();

        private static RaycastHit[] getResArray(int capacity) {
            if (!resultArrayPool.TryGetValue(capacity, out var pool)) {
                pool = new Stack<RaycastHit[]>();
                resultArrayPool[capacity] = pool;
            }
            if (pool.Count == 0) pool.Push(new RaycastHit[capacity]);
            return pool.Pop();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void initialize() => resultArrayPool = new();

        public static int Try(Vector3 origin, Vector3 direction, out RaycastHit[] result, 
            int capacity = 4, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, int maxCapacity = 1024,
            bool sortedCloseToFar = false, Vector3 sortPos = default
        ) {
            
            if (capacity < 1) capacity = 1;
            
            if (direction.sqrMagnitude <= 1e-12f) {
                result = getResArray(capacity);
                return 0;
            }
            direction.Normalize();

            int cap = capacity;
            var arr = getResArray(cap);
            int count;

            while (true) {
                count = Physics.RaycastNonAlloc(
                    origin, direction, arr,
                    maxDistance, layerMask,
                    queryTriggerInteraction
                );
                if (count < cap) break;

                // 可能被截断 则扩容重试
                Return(arr);
                if (cap >= maxCapacity) {
                    arr = getResArray(cap);
                    count = Physics.RaycastNonAlloc(
                        origin, direction, arr,
                        maxDistance, layerMask,
                        queryTriggerInteraction
                    );
                    break;
                }

                cap = Mathf.Min(cap << 1, maxCapacity);
                arr = getResArray(cap);
            }

            result = arr;
            if (sortedCloseToFar) result.sortByDistanceTo(count, sortPos);
            return count;
        }

        public static void Return(RaycastHit[] result) {
            if (result == null) return;

            int cap = result.Length;
            if (cap <= 0) return;

            if (!resultArrayPool.TryGetValue(cap, out var pool)) {
                pool = new Stack<RaycastHit[]>();
                resultArrayPool[cap] = pool;
            }

            pool.Push(result);
        }
        
        private sealed class HitToPosComparer : IComparer<RaycastHit> {
            public Vector3 pos;

            public int Compare(RaycastHit a, RaycastHit b) {
                // 处理无效 hit（比如 collider 为空）放到后面
                bool av = a.collider;
                bool bv = b.collider;
                if (!av) return bv ? 1 : 0;
                if (!bv) return -1;

                float da = (a.point - pos).sqrMagnitude;
                float db = (b.point - pos).sqrMagnitude;

                // Array.Sort 需要 int：-1 / 0 / 1
                return da < db ? -1 : da > db ? 1 : 0;
            }
        }

        private static readonly HitToPosComparer comparer = new();

        private static void sortByDistanceTo(this RaycastHit[] hits, int count, Vector3 pos) {
            if (hits == null || count <= 1) return;
            count = Mathf.Min(count, hits.Length);
            comparer.pos = pos;
            Array.Sort(hits, 0, count, comparer);
        }
    }
}