# if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Fries.Inspector;
using UnityEditor;
using UnityEngine;

namespace Fries.FbxFunctions.FbxId {
    [Serializable]
    public class FbxItemPositionInfo {
        public string name;
        public Vector3 pos;
    }
    
    [RequireComponent(typeof(FbxMatcher))]
    public class FbxCollectionImporter : MonoBehaviour {
        public TextAsset positionFile;

        public List<FbxItemPositionInfo> infos = new();

        [AButton("Import")] [IgnoreInInspector]
        public Action import;

        private void Reset() {
            import = () => {
                FbxMatcher fbxMatcher = gameObject.GetComponent<FbxMatcher>();
                foreach (var info in infos) {
                    FbxSearchResult result = fbxMatcher.getModelForCmpKey(info.name);
                    if (result == null) continue;
                    GameObject modelAsset = result.modelAsset;
                    GameObject go = Instantiate(modelAsset, info.pos, Quaternion.identity);
                    float scaleFactor = result.toFind.largestLength / result.found.largestLength;
                    go.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

                    Quaternion additionalRotation = Quaternion.Euler(result.toFind.angles.y-result.found.angles.y, result.toFind.angles.z-result.found.angles.z, 0);
                    go.transform.rotation = additionalRotation * go.transform.rotation;
                }
            };
        }

        private void OnValidate() {
            infos.Clear();
            if (positionFile == null) return;
            string[] positions = positionFile.text.Split("\r\n\r\n\r\n");
            positions.ForEach(str => {
                if (str == "") return;
                string[] comps = str.Split("|");
                string name = comps[0];
                string[] posRaw = comps[1].Split(",");
                Vector3 pos = new Vector3(float.Parse(posRaw[0]), float.Parse(posRaw[1]), float.Parse(posRaw[2]));
                infos.Add(new FbxItemPositionInfo {
                    name = name,
                    pos = pos
                });
            });
        }
    }
    
    [CustomEditor(typeof(FbxCollectionImporter))]
    public class FbxCollectionImporterInst : AnInspector {}
}
# endif