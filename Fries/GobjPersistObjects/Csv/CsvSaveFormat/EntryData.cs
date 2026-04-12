using System;
using System.Collections.Generic;
using Fries.GobjPersistObjects.Csv.Serialize;
using DataDict = System.Collections.Generic.Dictionary<string, (bool, object)>;

namespace Fries.GobjPersistObjects.Csv.CsvSaveFormat {
    public readonly struct EntryData {
        public readonly string primaryKey;
        public readonly SchemaDesc schema;
        public readonly Observation observation;

        public string uid() {
            foreach (FieldDefinition observationField in observation.fields) {
                if (observationField.fieldDecl.fieldName == CsvSave.DefaultPrimaryKey)
                    return observationField.value;
            }

            throw new InvalidOperationException("Cannot get uid, because the field is not present!");
        }

        public EntryData(DataDict dataDict) : this(CsvSave.DefaultPrimaryKey, dataDict) {}
        public EntryData(string primaryKey, DataDict dataDict) {
            this.primaryKey = primaryKey;
            
            List<string> sortedKeys = new();
            foreach (var k in dataDict.Keys)
                if (k != primaryKey) sortedKeys.Add(k);
            sortedKeys.Sort(StringComparer.Ordinal);
            sortedKeys.Insert(0, primaryKey);
            
            FieldDeclDesc[] fields = new FieldDeclDesc[sortedKeys.Count];
            FieldDefinition[] defs = new FieldDefinition[sortedKeys.Count];
            int i = 0;
            foreach (var key in sortedKeys) {
                (bool, object) tuple = dataDict[key];
                string type = SerializeHelper.GetTypeName(tuple.Item2);
                fields[i] = new FieldDeclDesc(key, type, tuple.Item1);
                defs[i] = new FieldDefinition(fields[i], SerializeHelper.Serialize(tuple.Item2));
                i++;
            }
            schema = new SchemaDesc(fields);
            observation = new Observation(defs);
        }

        public EntryData(string primaryKey, SchemaDesc schema, Observation observation) {
            this.primaryKey = primaryKey;
            this.schema = schema;
            this.observation = observation;
        }

        public DataDict CreateDataDict() {
            DataDict dict = new();
            for (int i = 0; i < schema.fields.Length; i++) {
                FieldDeclDesc fd = (FieldDeclDesc)schema.fields[i];
                FieldDefinition def = (FieldDefinition)observation.fields[i];
                
                string type = fd.fieldType;
                bool networked = fd.isNetworked;
                string key = fd.fieldName;
                string value = def.value;

                dict[key] = (networked, SerializeHelper.Deserialize(type, value));
            }

            return dict;
        }
    }
}