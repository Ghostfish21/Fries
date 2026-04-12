using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Fries.GobjPersistObjects.Csv.CsvSaveFormat;
using UnityEngine;

namespace Fries.GobjPersistObjects {
    public class GobjCsvSave : ISaveSystem {
        private CsvSave csvSave;
        private Dictionary<string, (bool, object)> metaData = new();
        private bool isDirty = true;
        private bool isCorrupted = false;
        
        private readonly string saveName;
        
        public GobjCsvSave(string saveName) {
            this.saveName = saveName;
            loadFromDisk();
        }

        public void Reset() {
            csvSave.Clear();
            PutEntry("__METADATA", metaData);
            isDirty = true;
        }

        private const string defaultFileContent = "string/uniqueName\r\n__METADATA";
        private void loadFromDisk() {
            string pathName = Path.Combine(Application.persistentDataPath, $"{saveName}.gpo");
            
            string dir = Path.GetDirectoryName(pathName);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            bool fileNotExistFlag = false;
            if (!File.Exists(pathName)) {
                File.WriteAllText(pathName, defaultFileContent);
                fileNotExistFlag = true;
            }

            string content = defaultFileContent;
            if (!fileNotExistFlag) 
                content = File.ReadAllText(pathName, Encoding.UTF8);
            Deserialize(content.Trim());
        }

        public void PutMetaData(string key, object value) {
            if (isCorrupted) {
                Debug.LogError("The gpo save is corrupted and unload. No put can be performed.");
                return;
            }
            
            metaData[key] = (false, value);
            metaData[CsvSave.DefaultPrimaryKey] = (false, "__METADATA");
            csvSave.Put("__METADATA", metaData);
            isDirty = true;
        }

        public void PutEntry(string uid, Dictionary<string, (bool, object)> data) {
            if (uid == "__METADATA") 
                throw new ArgumentException("UID '__METADATA' is preserved for metadata!");
            if (isCorrupted) {
                Debug.LogError("The save is corrupted and unload. No put can be performed.");
                return;
            }
            csvSave.Put(uid, data);
            isDirty = true;
        }

        private string lastSerializedCsv;
        
        public string Serialize() {
            if (isCorrupted) {
                Debug.LogError("The save is corrupted and unload. No serialize can be performed.");
                return null;
            }
            if (!isDirty) return lastSerializedCsv;
            lastSerializedCsv = csvSave.Serialize();
            isDirty = false;
            return lastSerializedCsv;
        }

        public void Deserialize(string csvString) {
            csvSave = new CsvSave();
            if (csvSave.Deserialize(csvString)) {
                isDirty = false;
                lastSerializedCsv = csvString;
                if (csvSave.TryGetEntry("__METADATA", out var entry)) 
                    metaData = entry.CreateDataDict();
                isCorrupted = false;
            }
            else isCorrupted = true;
        }

        public void Flush() {
            string path2Save = Path.Combine(Application.persistentDataPath, $"{saveName}.gpo");
            string content = Serialize();
            if (content == null) return;
            File.WriteAllText(path2Save, content);
        }

        public void LoadScene(GpoManager gpoManager) {
            foreach (EntryData entry in csvSave.Entries.ToList()) {
                try {
                    string uid = entry.uid();
                    if (uid == "__METADATA") continue;
                    if (uid == "") {
                        Debug.LogError("Found a corrupted uid in GPO save! UID is ''!");
                        continue;
                    }

                    var data = entry.CreateDataDict();
                    gpoManager.CreatePersistObject(data);
                }
                catch (Exception e) {
                    Debug.LogError("Caught exception when loading scene! Exception: " + e);
                }
            }
        }

        public void RemoveEntry(string uid) {
            if (uid == "__METADATA") 
                throw new ArgumentException("UID '__METADATA' is preserved for metadata!");
            if (isCorrupted) {
                Debug.LogError("The save is corrupted and unload. No put can be performed.");
                return;
            }
            csvSave.Remove(uid);
            isDirty = true;
        }
    }
}