using System;
using System.Text;

namespace Fries.GobjPersistObjects.Csv.CsvSaveFormat {
    public readonly struct FieldDefinition : ISlot {
        public readonly FieldDeclDesc fieldDecl;
        public readonly string value;

        public FieldDefinition(FieldDeclDesc fieldDecl, string value) {
            this.fieldDecl = fieldDecl;
            this.value = value;
        }

        public void writeInto(StringBuilder sb) {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            this.writeInto(sb, value);
        }
    }
}