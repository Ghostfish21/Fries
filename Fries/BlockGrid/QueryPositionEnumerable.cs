using System;
using System.Collections.Generic;
using Fries.Data;
using UnityEngine;

namespace Fries.BlockGrid {

    // 固定 TValue = Dictionary<int, ListSet<Facing>>
    internal readonly struct QueryPositionEnumerable {
        private readonly BlockMapQuerier _q;
        private readonly Dictionary<Vector3Int, Dictionary<int, ListSet<Facing>>> _map;

        public QueryPositionEnumerable(
            BlockMapQuerier q,
            Dictionary<Vector3Int, Dictionary<int, ListSet<Facing>>> map
        ) {
            _q = q;
            _map = map;
        }

        public Enumerator GetEnumerator() => new Enumerator(_q, _map);

        public struct Enumerator {
            private const int MODE_EMPTY = -1;
            private const int MODE_ALL = 0;
            private const int MODE_SINGLE = 1;
            private const int MODE_RANGE_FILTER_DICT = 2; // 遍历字典，做 AABB 过滤
            private const int MODE_RANGE_SCAN_POINTS = 3; // 扫描包围盒点位，TryGetValue

            private readonly Dictionary<Vector3Int, Dictionary<int, ListSet<Facing>>> _map;

            private Dictionary<Vector3Int, Dictionary<int, ListSet<Facing>>>.Enumerator _e;
            private readonly int _mode;

            // Current：遍历字典的键值对（而不是 Key）
            private KeyValuePair<Vector3Int, Dictionary<int, ListSet<Facing>>> _current;
            public KeyValuePair<Vector3Int, Dictionary<int, ListSet<Facing>>> Current => _current;

            // single
            private bool _singleDone;
            private readonly KeyValuePair<Vector3Int, Dictionary<int, ListSet<Facing>>> _singleKvp;

            // range bounds
            private readonly int _minX;
            private readonly int _minY;
            private readonly int _minZ;
            private readonly int _maxX;
            private readonly int _maxY;
            private readonly int _maxZ;

            // scan-points cursor
            private bool _scanInit;
            private int _scanX, _scanY, _scanZ;

            public Enumerator(
                BlockMapQuerier q,
                Dictionary<Vector3Int, Dictionary<int, ListSet<Facing>>> map
            ) {
                _map = map;

                _e = default;
                _mode = MODE_EMPTY;

                _current = default;

                _singleDone = false;
                _singleKvp = default;

                _minX = _minY = _minZ = 0;
                _maxX = _maxY = _maxZ = 0;

                _scanInit = false;
                _scanX = _scanY = _scanZ = 0;

                if (map == null || map.Count == 0) return;

                var usingPos = q.usingAtOrRangePos;

                // 0) 不限制位置：直接遍历整个字典（天然保证元素存在）
                if (usingPos == null) {
                    _mode = MODE_ALL;
                    _e = map.GetEnumerator();
                    return;
                }

                // 1) AT：只返回该点（但必须在字典里存在）
                if (usingPos.Value == BlockMapQuerier.AT) {
                    if (!q.at.HasValue)
                        throw new Exception("At is missing value! This is an internal error!");

                    var key = q.at.Value;
                    if (map.TryGetValue(key, out var val)) {
                        _mode = MODE_SINGLE;
                        _singleKvp = new KeyValuePair<Vector3Int, Dictionary<int, ListSet<Facing>>>(key, val);
                    } else {
                        _mode = MODE_EMPTY; // 不存在就什么都不遍历
                    }
                    return;
                }

                // 2) RANGE：两种策略择快
                if (!q.at.HasValue || !q.to.HasValue)
                    throw new Exception("PosRange is missing value! This is an internal error!");

                Vector3Int a = q.at.Value;
                Vector3Int b = q.to.Value;

                _minX = Mathf.Min(a.x, b.x);
                _maxX = Mathf.Max(a.x, b.x);
                _minY = Mathf.Min(a.y, b.y);
                _maxY = Mathf.Max(a.y, b.y);
                _minZ = Mathf.Min(a.z, b.z);
                _maxZ = Mathf.Max(a.z, b.z);

                long dx = (long)_maxX - _minX + 1;
                long dy = (long)_maxY - _minY + 1;
                long dz = (long)_maxZ - _minZ + 1;

                if (dx <= 0 || dy <= 0 || dz <= 0) {
                    _mode = MODE_EMPTY;
                    return;
                }

                long volume = dx * dy * dz;          // 包围盒点位数量
                long count = map.Count;

                // 经验阈值（可按你项目数据分布调整）：
                // - volume 很小：扫点位 TryGetValue 更快（尤其字典很大时）
                // - volume 很大：遍历字典做过滤更快
                bool scanPointsFaster = volume <= (count * 2);

                if (scanPointsFaster) {
                    _mode = MODE_RANGE_SCAN_POINTS;
                    // cursor 会在 MoveNext 里初始化
                } else {
                    _mode = MODE_RANGE_FILTER_DICT;
                    _e = map.GetEnumerator();
                }
            }

            public bool MoveNext() {
                switch (_mode) {
                    case MODE_ALL: {
                        if (!_e.MoveNext()) return false;
                        _current = _e.Current;
                        return true;
                    }

                    case MODE_SINGLE: {
                        if (_singleDone) return false;
                        _singleDone = true;
                        _current = _singleKvp; // 已经保证存在
                        return true;
                    }

                    case MODE_RANGE_FILTER_DICT: {
                        while (_e.MoveNext()) {
                            var kv = _e.Current;
                            var p = kv.Key;
                            if (p.x < _minX || p.x > _maxX) continue;
                            if (p.y < _minY || p.y > _maxY) continue;
                            if (p.z < _minZ || p.z > _maxZ) continue;
                            _current = kv; // 来自字典枚举，必定存在
                            return true;
                        }
                        return false;
                    }

                    case MODE_RANGE_SCAN_POINTS: {
                        if (_map == null) return false;

                        if (!_scanInit) {
                            _scanInit = true;
                            _scanX = _minX;
                            _scanY = _minY;
                            _scanZ = _minZ;
                        }

                        while (true) {
                            if (_scanX > _maxX) return false;

                            var key = new Vector3Int(_scanX, _scanY, _scanZ);

                            // 先推进游标，再决定是否产出（避免漏掉最后一个点）
                            AdvanceScanCursor();

                            if (_map.TryGetValue(key, out var val)) {
                                _current = new KeyValuePair<Vector3Int, Dictionary<int, ListSet<Facing>>>(key, val);
                                return true;
                            }
                        }
                    }

                    default:
                        return false;
                }
            }

            private void AdvanceScanCursor() {
                _scanZ++;
                if (_scanZ <= _maxZ) return;

                _scanZ = _minZ;
                _scanY++;
                if (_scanY <= _maxY) return;

                _scanY = _minY;
                _scanX++;
            }

            public void Dispose() {
                // 只有用到字典枚举器的模式需要 Dispose
                if (_mode == MODE_ALL || _mode == MODE_RANGE_FILTER_DICT)
                    _e.Dispose();
            }
        }
    }
}
