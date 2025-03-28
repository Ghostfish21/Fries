﻿using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Fries {
    public static class LinQ {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action) {
            foreach (var item in ie) action(item);
        }
        
        public static void ForEach<T>(this IEnumerable<T> ie, Action<int, T> action) {
            int i = 0;
            foreach (var item in ie) {
                action(i, item);
                i++;
            }
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