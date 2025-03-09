using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Fries {
    public static class LinQ {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action) {
            foreach (var item in ie) action(item);
        }
        
        public static void For<T>(this IEnumerable<T> ie, Action<int, T> action) {
            int i = 0;
            foreach (var item in ie) {
                action(i, item);
                i++;
            }
        }

        public static T RandomElement<T>(this IList<T> list) {
            int ri = Random.Range(0, list.Count);
            return list[ri];
        }
    }
}