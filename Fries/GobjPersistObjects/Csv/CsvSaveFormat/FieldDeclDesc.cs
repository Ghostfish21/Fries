using System;
using System.Text;
using static System.StringComparer;

namespace Fries.GobjPersistObjects.Csv.CsvSaveFormat {

    public readonly struct FieldDeclDesc : ISlot, IEquatable<FieldDeclDesc> {
        public readonly string fieldName;
        public readonly string fieldType;
        public readonly bool isNetworked;

        public FieldDeclDesc(string fieldName, string fieldType, bool isNetworked) {
            this.fieldName = fieldName;
            this.fieldType = fieldType;
            this.isNetworked = isNetworked;
        }

        public void writeInto(StringBuilder sb) {
            string typeName = fieldType;
            if (isNetworked) typeName += "-E";
            string content = typeName + "/" + fieldName;
            this.writeInto(sb, content);
        }

        public bool Equals(FieldDeclDesc other) {
            return isNetworked == other.isNetworked
                   && string.Equals(fieldName, other.fieldName, StringComparison.Ordinal)
                   && string.Equals(fieldType, other.fieldType, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) => obj is FieldDeclDesc other && Equals(other);

        public override int GetHashCode() {
            unchecked {
                int h = 17;
                h = h * 31 + (fieldName != null ? Ordinal.GetHashCode(fieldName) : 0);
                h = h * 31 + (fieldType != null ? Ordinal.GetHashCode(fieldType) : 0);
                h = h * 31 + (isNetworked ? 1 : 0);
                return h;
            }
        }
        
        public static bool operator ==(FieldDeclDesc a, FieldDeclDesc b) => a.Equals(b);
        public static bool operator !=(FieldDeclDesc a, FieldDeclDesc b) => !a.Equals(b);
    }
}