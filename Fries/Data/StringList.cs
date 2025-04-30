using System;
using System.Collections;
using System.Collections.Generic;

namespace Fries.Data {
    public class StringList<T> : IList<T> {
        private Func<T, string> serialize;
        private Func<string, T> deserialize;
        private List<T> elements = new();

        public StringList(string raw, Func<T, string> serializer, Func<string, T> deserializer) {
            this.serialize = serializer;
            this.deserialize = deserializer;
            
            if (raw.Nullable().Trim() != "") {
                int counter = 0;
                int startIndex = 0;
                int index = 0;
                foreach (var c in raw) {
                    if (c == '{') counter++;
                    else if (c == '}') counter--;

                    if (c == ',') {
                        string s = raw.Substring(startIndex, index - startIndex);
                        if (s.Length < 2)
                            throw new ArgumentException("Input raw is not a correct construct string for String List, elements must have body!");
                        s = s.Substring(1, s.Length - 2);
                        elements.Add(deserialize(s));
                    }
                    
                    index++;
                }
            }
        }

        public string export() {
            return "{" + string.Join("},{", elements) + "}";
        }

        public IEnumerator<T> GetEnumerator() {
            return elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(T item) {
            elements.Add(item);
        }

        public void Clear() {
            elements.Clear();
        }

        public bool Contains(T item) {
            return elements.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            elements.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            return elements.Remove(item);
        }

        public int Count => elements.Count;
        public bool IsReadOnly => throw new NotSupportedException("Elements' isReadOnly state is private.");

        public int IndexOf(T item) {
            return elements.IndexOf(item);
        }

        public void Insert(int index, T item) {
            elements.Insert(index, item);
        }

        public void RemoveAt(int index) {
            elements.RemoveAt(index);
        }

        public T this[int index] {
            get => elements[index];
            set => elements[index] = value;
        }
    }
}