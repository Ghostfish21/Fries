using System;
using System.Text;

namespace Fries.GobjPersistObjects.Csv.CsvSaveFormat {
    public readonly struct SchemaDesc : IRow, IEquatable<SchemaDesc> {
        private readonly ISlot[] _fields;

        public ReadOnlySpan<ISlot> fields => _fields;

        public SchemaDesc(FieldDeclDesc[] fields) {
            if (fields == null || fields.Length == 0) {
                _fields = Array.Empty<ISlot>();
                return;
            }

            var copy = new ISlot[fields.Length];
            Array.Copy(fields, copy, fields.Length);
            _fields = copy;
        }

        public void writeInto(StringBuilder sb) {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            this.writeInto(sb, _fields);
        }
        
        public bool Equals(SchemaDesc other) {
            var a = _fields;
            var b = other._fields;

            int aLen = a?.Length ?? 0;
            int bLen = b?.Length ?? 0;
            if (aLen != bLen) return false;
            if (aLen == 0) return true;

            for (int i = 0; i < aLen; i++) 
                if (!a[i].Equals(b[i])) return false;

            return true;
        }

        public override bool Equals(object obj) => obj is SchemaDesc other && Equals(other);

        public override int GetHashCode() {
            unchecked {
                int h = 17;
                var a = _fields;
                h = h * 31 + (a != null ? a.Length : 0);

                if (a == null) return h; 
                foreach (var t in a)
                    h = h * 31 + t.GetHashCode();
                return h;
            }
        }

        public static bool operator ==(SchemaDesc a, SchemaDesc b) => a.Equals(b);
        public static bool operator !=(SchemaDesc a, SchemaDesc b) => !a.Equals(b);
    }
}