using System;
using System.Collections.Generic;
using Fries.Data;

namespace Fries.BlockGrid {
    internal readonly struct FacingEnumerable {
        private readonly BlockMapQuerier _q;
        private readonly ListSet<Facing> _facings;

        public FacingEnumerable(BlockMapQuerier q, ListSet<Facing> facings) {
            _q = q;
            _facings = facings;
        }

        public Enumerator GetEnumerator() => new(_q, _facings);

        public struct Enumerator {
            private readonly ListSet<Facing> _facings;

            // 这里用 fully-qualified，避免任何命名冲突/误判
            private List<Facing>.Enumerator _allEnum;
            private List<Facing>.Enumerator _rangeEnum;

            // mode: -1 empty, 0 all, 1 range(set), 2 single
            private readonly int _mode;

            private Facing _current;
            public readonly Facing Current => _current;

            private readonly Facing _single;
            private readonly bool _singleValid;
            private bool _singleDone;

            public Enumerator(BlockMapQuerier q, ListSet<Facing> facings) {
                _facings = facings;

                _allEnum = default;
                _rangeEnum = default;
                _mode = -1;

                _current = default;
                _single = default;
                _singleValid = false;
                _singleDone = false;

                if (facings == null) return;

                if (q.usingFacingSet == null) {
                    _mode = 0;
                    _allEnum = facings.GetEnumerator(); // ✅ 这里类型就是 List<Facing>.Enumerator
                    return;
                }

                if (q.usingFacingSet.Value == BlockMapQuerier.RANGE) {
                    _mode = 1;
                    _rangeEnum = q.facingSet.GetEnumerator(); // ✅ 同上
                    return;
                }

                // AT
                _mode = 2;
                if (!q.facing.HasValue)
                    throw new Exception("Facing is missing value! This is an internal error!");
                _single = q.facing.Value;
                _singleValid = facings.Contains(_single);
            }

            public bool MoveNext() {
                switch (_mode) {
                    case 0: // all
                        if (_allEnum.MoveNext()) {
                            _current = _allEnum.Current;
                            return true;
                        }

                        return false;

                    case 1: // range
                        while (_rangeEnum.MoveNext()) {
                            var f = _rangeEnum.Current;
                            if (!_facings.Contains(f)) continue;
                            _current = f;
                            return true;
                        }

                        return false;

                    case 2: // single
                        if (_singleDone) return false;
                        _singleDone = true;
                        if (!_singleValid) return false;
                        _current = _single;
                        return true;

                    default:
                        return false;
                }
            }

            public void Dispose() {
                _allEnum.Dispose();
                _rangeEnum.Dispose();
            }
        }
    }
}