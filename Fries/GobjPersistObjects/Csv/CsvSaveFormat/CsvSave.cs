using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using DataDict = System.Collections.Generic.Dictionary<string, (bool, object)>;

namespace Fries.GobjPersistObjects.Csv.CsvSaveFormat {
    public class CsvSave {
        public const string DefaultPrimaryKey = "uniqueName";
        public IEnumerable<EntryData> Entries => uid2DataMap.Values;
        
        private Dictionary<SchemaDesc, HashSet<string>> schemaMembers = new();
        private Dictionary<string, EntryData> uid2DataMap = new();

        public void Clear() {
            schemaMembers.Clear();
            uid2DataMap.Clear();
        }
        
        public void Put(string uid, DataDict data) {
            if (!data.TryGetValue(DefaultPrimaryKey, out var tuple)) 
                throw new ArgumentException($"Primary key not found for UID {uid}!");
            if ((string)tuple.Item2 != uid) 
                throw new ArgumentException($"Primary key mismatch for UID {uid}!");
            EntryData newData = new EntryData(data);
            Put(uid, newData);
        }

        public void Put(string uid, EntryData newData) {
            if (newData.observation.fields.Length == 0)
                throw new ArgumentException($"New Data of uid {uid} doesn't have observation!");
            bool hasPrimaryKey = false;
            FieldDefinition? uidDef = null;
            foreach (var field in newData.observation.fields) {
                FieldDefinition def = (FieldDefinition)field;
                if (def.fieldDecl.fieldName != DefaultPrimaryKey) continue;
                hasPrimaryKey = true;
                uidDef = def;
                break;
            }
            if (!hasPrimaryKey || uidDef == null) 
                throw new ArgumentException($"Primary key not found for UID {uid}!");
            if (uidDef.Value.value != uid) throw new ArgumentException($"Primary key mismatch for UID {uid}");
                
            if (uid2DataMap.TryGetValue(uid, out EntryData entryData)) {
                if (schemaMembers.TryGetValue(entryData.schema, out HashSet<string> uids)) {
                    uids.Remove(uid);
                    if (uids.Count == 0) schemaMembers.Remove(entryData.schema);
                }
            }
            
            schemaMembers.TryAdd(newData.schema, new HashSet<string>());
            schemaMembers[newData.schema].Add(uid);
            uid2DataMap[uid] = newData;
        }

        public string Serialize() {
            List<IRow> rows = new();
            foreach (var schema in schemaMembers.Keys) {
                if (schema.fields.Length == 0) {
                    Debug.LogError("Caught corrupted schema, fields length is 0!");
                    continue;
                }
                if (schema.fields[0] is FieldDeclDesc fieldDecl && fieldDecl.fieldName != DefaultPrimaryKey) {
                    Debug.LogError("Caught corrupted schema, first field must be Primary Key!");
                    continue;
                }
                
                rows.Add(schema);
                foreach (var uid in schemaMembers[schema]) {
                    EntryData entryData = uid2DataMap[uid];
                    rows.Add(entryData.observation);
                }
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < rows.Count; i++) {
                rows[i].writeInto(sb);
                if (i != rows.Count - 1) sb.Append("\r\n");
            }
            return sb.ToString();
        }

        public bool Deserialize(string input) {
            try {
                LineParser lp = new LineParser();
                foreach (var line in enumerateCsvRecords(input)) {
                    if (line.Trim() == "") continue;
                    EntryData? data = lp.parse(line);
                    if (data != null) Put(data.Value.uid(), data.Value);
                }
            }
            catch (Exception ex) {
                Debug.Log($"Deserialize failed! Csv Save is not loaded. \n{ex}");
                return false;
            }

            return true;
        }
        
        private static IEnumerable<string> enumerateCsvRecords(string input) {
            if (string.IsNullOrEmpty(input)) yield break;

            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < input.Length; i++) {
                char c = input[i];

                if (!inQuotes) {
                    if (c == '\r') {
                        yield return sb.ToString();
                        sb.Clear();
                        if (i + 1 < input.Length && input[i + 1] == '\n') i++; // consume \n
                        continue;
                    }

                    if (c == '\n') {
                        yield return sb.ToString();
                        sb.Clear();
                        continue;
                    }
                }

                if (c == '"') {
                    if (inQuotes) {
                        if (i + 1 < input.Length && input[i + 1] == '"') {
                            sb.Append('"');
                            sb.Append('"');
                            i++;
                            continue;
                        }

                        inQuotes = false;
                        sb.Append('"');
                        continue;
                    }

                    inQuotes = true;
                    sb.Append('"');
                    continue;
                }

                sb.Append(c);
            }

            if (inQuotes) throw new InvalidOperationException("Data is corrupted! The quote is unbalanced!");
            yield return sb.ToString();
        }


        public bool TryGetEntry(string uid, out EntryData data) {
            data = default;
            if (!uid2DataMap.TryGetValue(uid, out var entryData)) return false;
            data = entryData;
            return true;
        }

        public void Remove(string uid) {
            if (!uid2DataMap.Remove(uid, out var entryData)) return;
            var schema = entryData.schema;
            if (!schemaMembers.TryGetValue(schema, out var uids)) return;
            uids.Remove(uid);
            if (uids.Count == 0) schemaMembers.Remove(schema);
        }
    }
}