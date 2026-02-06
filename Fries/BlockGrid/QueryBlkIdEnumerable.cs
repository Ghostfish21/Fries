using System;
using System.Collections.Generic;
using Fries.Data;

namespace Fries.BlockGrid {
    internal readonly struct BlkIdEnumerable {
        private readonly BlockMapQuerier _q;
        private readonly Dictionary<int, ListSet<Facing>> _dict;
        
        public BlkIdEnumerable(BlockMapQuerier q, Dictionary<int, ListSet<Facing>> dict) {
            _q = q;
            _dict = dict;
        }

        public Enumerator GetEnumerator() => new Enumerator(_q, _dict);

        public struct Enumerator {
            private Dictionary<int, ListSet<Facing>>.Enumerator _e;

            // mode: -1 empty, 0 all, 1 single, 2 range(loop dict), 3 range(loop range)
            private readonly int _mode;

            private readonly int _singleKey;
            private readonly ListSet<Facing> _singleValue;
            private readonly bool _singleValid;
            private bool _singleDone;

            private readonly int _lo;
            private readonly int _hi;

            private readonly Dictionary<int, ListSet<Facing>> _dict;
            private int _next;

            private KeyValuePair<int, ListSet<Facing>> _current;
            public KeyValuePair<int, ListSet<Facing>> Current => _current;

            public Enumerator(BlockMapQuerier q, Dictionary<int, ListSet<Facing>> dict) {
                _e = default;

                _mode = -1;

                _singleKey = 0;
                _singleValue = null;
                _singleValid = false;
                _singleDone = false;

                _lo = _hi = 0;

                _dict = dict;
                _next = 0;

                _current = default;

                if (dict == null) return;

                var usingType = q.usingAtOrRangeBlkType;
                if (usingType == null) {
                    _mode = 0;
                    _e = dict.GetEnumerator();
                    return;
                }

                if (usingType.Value == BlockMapQuerier.AT) {
                    if (!q.blkTypeAt.HasValue)
                        throw new Exception("BlkTypeAt is missing value! This is an internal error!");

                    int key = q.blkTypeAt.Value;
                    if (!dict.TryGetValue(key, out var value)) {
                        _mode = -1;
                        return;
                    }

                    _mode = 1;
                    _singleKey = key;
                    _singleValue = value;
                    _singleValid = true;
                    return;
                }

                // RANGE
                if (!q.blkTypeAt.HasValue || !q.blkTypeTo.HasValue)
                    throw new Exception("BlkType range is missing value! This is an internal error!");

                int lo = q.blkTypeAt.Value;
                int hi = q.blkTypeTo.Value;
                if (lo > hi) (lo, hi) = (hi, lo);

                _lo = lo;
                _hi = hi;

                // Auto: scan dict vs loop range
                // scan dict: O(dict.Count)
                // loop range: O(rangeLen) with TryGetValue
                long rangeLen = (long)hi - lo + 1;
                long dictCount = dict.Count;
                var chosen = rangeLen > 0 && rangeLen <= dictCount * 2L
                    ? BlockMapQuerier.LoopRange
                    : BlockMapQuerier.LoopDict;

                if (chosen == BlockMapQuerier.LoopRange) {
                    _mode = 3;
                    _next = _lo;
                }
                else {
                    _mode = 2;
                    _e = dict.GetEnumerator();
                }
            }

            public bool MoveNext() {
                switch (_mode) {
                    case 0: // all pairs
                        if (_e.MoveNext()) {
                            _current = _e.Current;
                            return true;
                        }
                        return false;

                    case 1: // single pair
                        if (!_singleValid || _singleDone) return false;
                        _singleDone = true;
                        _current = new KeyValuePair<int, ListSet<Facing>>(_singleKey, _singleValue);
                        return true;

                    case 2: // range(filter pairs by key)
                        while (_e.MoveNext()) {
                            var kv = _e.Current;
                            int k = kv.Key;
                            if (k < _lo || k > _hi) continue;
                            _current = kv;
                            return true;
                        }
                        return false;

                    case 3: // range(loopRange) => TryGetValue
                        while (_next <= _hi) {
                            int k = _next++;
                            if (!_dict.TryGetValue(k, out var v)) continue;
                            _current = new KeyValuePair<int, ListSet<Facing>>(k, v);
                            return true;
                        }
                        return false;

                    default:
                        return false;
                }
            }

            public void Dispose() {
                if (_mode == 0 || _mode == 2) _e.Dispose();
            }
        }
    }
}
