using System;
using System.Text;

namespace Fries.GobjPersistObjects.Csv.CsvSaveFormat {
    public readonly struct Observation : IRow {
        private readonly ISlot[] _fields;
        public readonly SchemaDesc schema;
        
        public ReadOnlySpan<ISlot> fields => _fields;

        public Observation(FieldDefinition[] fields) {
            if (fields == null || fields.Length == 0) {
                _fields = Array.Empty<ISlot>();
                schema = new SchemaDesc();
                return;
            }

            var copy = new ISlot[fields.Length];
            Array.Copy(fields, copy, fields.Length);
            _fields = copy;
            
            FieldDeclDesc[] fieldDecls = new FieldDeclDesc[fields.Length];
            for (int i = 0; i < fields.Length; i++) 
                fieldDecls[i] = fields[i].fieldDecl;
            schema = new SchemaDesc(fieldDecls);
        }
        
        public void writeInto(StringBuilder sb) {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            this.writeInto(sb, _fields);
        }
    }
}