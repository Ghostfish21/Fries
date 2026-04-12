using System;
using System.Collections.Generic;
using System.Text;

namespace Fries.GobjPersistObjects.Csv.CsvSaveFormat {
    public struct LineParser {
        public EntryData? parse(string line) => parse(CsvSave.DefaultPrimaryKey, line);
        private SchemaDesc lastSchema;
        
        public EntryData? parse(string primaryKey, string line) {
            if (primaryKey == null) throw new ArgumentNullException(nameof(primaryKey));
            
            if (string.IsNullOrWhiteSpace(line)) return null;

            int index = line.IndexOf(",", StringComparison.Ordinal);
            string firstPart = line;
            if (index != -1) firstPart = line.Substring(0, index);
            if (firstPart.EndsWith("/" + primaryKey)) {
                parseSchema(line);
                return null;
            }

            return parseObservation(primaryKey, line);
        }

        private void parseSchema(string line) {
            string[] parts = line.Split(',');
            
            FieldDeclDesc[] fields = new FieldDeclDesc[parts.Length];
            int i = 0;
            foreach (var part in parts) {
                string[] comps = part.Split('/');
                string fieldType = comps[0];
                string fieldName = comps[1];
                bool isNetworked = fieldType.EndsWith("-E");
                if (isNetworked) fieldType = fieldType.Substring(0, fieldType.Length - 2);
                fields[i] = new FieldDeclDesc(fieldName, fieldType, isNetworked);
                i++;
            }

            lastSchema = new SchemaDesc(fields);
        }
        
        private EntryData parseObservation(string primaryKey, string line) {
            string[] parts = splitCsvFields(line);
            
            FieldDefinition[] fields = new FieldDefinition[parts.Length];
            int i = 0;
            foreach (var part in parts) {
                fields[i] = new FieldDefinition((FieldDeclDesc)lastSchema.fields[i], part);
                i++;
            }

            EntryData ed = new EntryData(primaryKey, lastSchema, new Observation(fields));
            return ed;
        }
        
        private static string[] splitCsvFields(string line) {
            if (line == null) return Array.Empty<string>();

            var fields = new List<string>(16);
            var sb = new StringBuilder();

            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++) {
                char c = line[i];

                if (inQuotes) {
                    if (c == '"') {
                        if (i + 1 < line.Length && line[i + 1] == '"') {
                            sb.Append('"');
                            i++;
                            continue;
                        }
                        inQuotes = false;
                        continue;
                    }

                    sb.Append(c);
                    continue;
                }

                if (c == ',') {
                    fields.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }

                if (c == '"') {
                    if (sb.Length == 0) {
                        inQuotes = true;
                        continue;
                    }
                }

                sb.Append(c);
            }

            fields.Add(sb.ToString());
            return fields.ToArray();
        }

    }
}