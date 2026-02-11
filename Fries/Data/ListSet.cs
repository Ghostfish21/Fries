namespace Fries.Data {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class ListSet<T> : IReadOnlyCollection<T>, ISet<T> {
        private readonly List<T> _items;
        private readonly IEqualityComparer<T> _comparer;

        public ListSet() : this(0) {
        }

        public ListSet(IEqualityComparer<T> comparer) : this(0, comparer) {
        }

        public ListSet(int capacity, IEqualityComparer<T> comparer = null) {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _items = capacity == 0 ? new List<T>() : new List<T>(capacity);
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public ListSet(IEnumerable<T> collection, IEqualityComparer<T> comparer = null)
            : this(0, comparer) {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            UnionWith(collection);
        }

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        /// <summary>按 Set 语义添加。已存在则返回 false。</summary>
        public bool Add(T item) {
            if (IndexOf(item) >= 0) return false;
            _items.Add(item);
            return true;
        }

        void ICollection<T>.Add(T item) => Add(item);

        public void Clear() => _items.Clear();

        public bool Contains(T item) => IndexOf(item) >= 0;

        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        public bool Remove(T item) {
            int idx = IndexOf(item);
            if (idx < 0) return false;
            _items.RemoveAt(idx);
            return true;
        }

        public List<T>.Enumerator GetEnumerator() => _items.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();

        // ---------------- ISet<T> ----------------

        public void UnionWith(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException(nameof(other));
            foreach (var x in other) Add(x);
        }

        public void IntersectWith(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException(nameof(other));

            var otherUniques = ToUniqueList(other);
            // 反向删，避免索引问题
            for (int i = _items.Count - 1; i >= 0; i--) {
                if (IndexOfInList(otherUniques, _items[i]) < 0)
                    _items.RemoveAt(i);
            }
        }

        public void ExceptWith(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException(nameof(other));

            var otherUniques = ToUniqueList(other);
            for (int i = _items.Count - 1; i >= 0; i--) {
                if (IndexOfInList(otherUniques, _items[i]) >= 0)
                    _items.RemoveAt(i);
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException(nameof(other));

            // 需要把 other 当作集合（去重），避免重复元素“翻转两次”
            var otherUniques = ToUniqueList(other);

            for (int i = 0; i < otherUniques.Count; i++) {
                var x = otherUniques[i];
                if (!Remove(x)) Add(x);
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException(nameof(other));

            var otherUniques = ToUniqueList(other);
            for (int i = 0; i < _items.Count; i++) {
                if (IndexOfInList(otherUniques, _items[i]) < 0)
                    return false;
            }

            return true;
        }

        public bool IsSupersetOf(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException(nameof(other));

            // other 可能有重复，按集合语义判断：other 的每个“唯一元素”都应在 this 中
            var otherUniques = ToUniqueList(other);
            for (int i = 0; i < otherUniques.Count; i++) {
                if (!Contains(otherUniques[i]))
                    return false;
            }

            return true;
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException(nameof(other));

            var otherUniques = ToUniqueList(other);
            if (_items.Count >= otherUniques.Count) return false;
            return IsSubsetOf(otherUniques);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException(nameof(other));

            var otherUniques = ToUniqueList(other);
            if (_items.Count <= otherUniques.Count) return false;
            return IsSupersetOf(otherUniques);
        }

        public bool Overlaps(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException(nameof(other));

            foreach (var x in other) {
                if (Contains(x)) return true;
            }

            return false;
        }

        public bool SetEquals(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException(nameof(other));

            var otherUniques = ToUniqueList(other);
            if (_items.Count != otherUniques.Count) return false;

            // 等价于：互为子集
            for (int i = 0; i < _items.Count; i++) {
                if (IndexOfInList(otherUniques, _items[i]) < 0)
                    return false;
            }

            return true;
        }

        // ---------------- helpers ----------------

        private int IndexOf(T item) {
            for (int i = 0; i < _items.Count; i++) {
                if (_comparer.Equals(_items[i], item))
                    return i;
            }

            return -1;
        }

        private int IndexOfInList(List<T> list, T item) {
            for (int i = 0; i < list.Count; i++) {
                if (_comparer.Equals(list[i], item))
                    return i;
            }

            return -1;
        }

        private List<T> ToUniqueList(IEnumerable<T> source) {
            var uniques = source is ICollection<T> c ? new List<T>(c.Count) : new List<T>();
            foreach (var x in source) {
                if (IndexOfInList(uniques, x) < 0)
                    uniques.Add(x);
            }

            return uniques;
        }
    }

}