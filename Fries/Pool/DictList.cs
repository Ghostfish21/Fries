using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fries.Pool {
    public class DictList<T> : IList<T> {

        private Dictionary<T, bool> dictionary = new();
        
        public IEnumerator<T> GetEnumerator() {
            return dictionary.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(T item) {
            dictionary.TryAdd(item, true);
        }

        public void Clear() {
            dictionary.Clear();
        }

        public bool Contains(T item) {
            return item != null && dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            dictionary.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            return item != null && dictionary.Remove(item);
        }

        public int Count => dictionary.Count;
        public bool IsReadOnly => false;
        
        public int IndexOf(T item) {
            throw new NotSupportedException("Dictionary-backed list does not support indexing.");
        }

        public void Insert(int index, T item) {
            throw new NotSupportedException("Dictionary-backed list does not support insertion at an index.");
        }

        public void RemoveAt(int index) {
            throw new NotSupportedException("Dictionary-backed list does not support indexed removal.");
        }

        public T this[int index] {
            get => dictionary.First().Key;
            set => throw new NotSupportedException("Dictionary-backed list does not support indexing.");
        }
    }
}