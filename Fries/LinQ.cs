using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Fries.Data;
using Fries.Pool;
using Random = UnityEngine.Random;

namespace Fries {
    public class Break {
        public bool b = false;
        public void @break() {
            b = true;
        }
    }
    
    public static class LinQ {
        public static T[] Join<T>(this T[] array, T[] toAdd) {
            T[] newArray = new T[array.Length + toAdd.Length];
            (array, toAdd).ForEach<T>((i, e) => {
                newArray[i] = e;
            });

            return newArray;
        }

        public static void ForEach<T>(this ITuple tuple, Action<T> action) {
            for (int i = 0; i < tuple.Length; i++) {
                action((T)tuple[i]);
            }
        }
        
        public static void ForEach<T>(this ITuple tuple, Action<int, T> action) {
            for (int i = 0; i < tuple.Length; i++) {
                action(i, (T)tuple[i]);
            }
        }
        
        public static void ForEach<T>(this ITuple tuple, Action<int, T, Break> action) {
            Break b = new Break();
            for (int i = 0; i < tuple.Length; i++) {
                action(i, (T)tuple[i], b);
                if (b.b) break;
            }
        }

        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action) {
            foreach (var item in ie) action(item);
        }
        
        public static void ForEach<T>(this IEnumerable<T> ie, Action<int, T, Break> action) {
            int i = 0;
            Break b = new Break();
            foreach (var item in ie) {
                action(i, item, b);
                if (b.b) break;
                i++;
            }
        }
        
        public static void ForEach<T>(this IEnumerable<T> ie, Action<int, T> action) {
            int i = 0;
            foreach (var item in ie) {
                action(i, item);
                i++;
            }
        }
        
        public static void ForRange(this int from, int exclusiveTo, Action<int> action) {
            for (int i = from; i < exclusiveTo; i++) {
                action(i);
            }
        }
        
        public static void ForRange(this (int from, int exclusiveTo) param, Action<int> action) {
            param.from.ForRange(param.exclusiveTo, action);
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

        public static List<T> Nullable<T>(this List<T> list) {
            if (list == null) return new List<T>();
            return list;
        }
        
        public static DictList<T> Nullable<T>(this DictList<T> list) {
            if (list == null) return new DictList<T>();
            return list;
        }
        
        public static T[] Nullable<T>(this T[] array) {
            if (array == null) return Array.Empty<T>();
            return array;
        }

        public static string Nullable(this string str) {
            if (str == null) return "";
            return str;
        }
    }
}