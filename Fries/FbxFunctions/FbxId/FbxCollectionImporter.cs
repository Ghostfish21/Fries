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
                fbxMatcher.foundFbxAssets.Nullable().ForEach(result => {
                    GameObject modelAsset = result.modelAsset;
                    GameObject root = GameObject.Find("Root");
                    if (root == null) root = new GameObject("Root");
                    root.transform.position = new Vector3(0, 0, 0);
                    root.transform.localScale = new Vector3(1, 1, 1);
                    root.transform.eulerAngles = new Vector3(0, 0, 0);
                    
                    GameObject go = Instantiate(modelAsset, root.transform);
                    float scaleFactor = result.toFind.largestLength / result.found.largestLength;
                    go.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

                    Quaternion additionalRotation = Quaternion.Euler(result.rotateAngle.y, result.rotateAngle.x, result.rotateAngle.z);
                    go.transform.rotation = additionalRotation * go.transform.rotation;

                    Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
                    float totalX = 0;
                    float totalY = 0;
                    float totalZ = 0;
                    int i = 0;
                    foreach (var vertex in mesh.vertices) {
                        Vector3 worldVertex = transform.TransformPoint(vertex);
                        totalX += worldVertex.x;
                        totalY += worldVertex.y;
                        totalZ += worldVertex.z;
                        i++;
                    }

                    Vector3 center = new Vector3(totalX / i, totalY / i, totalZ / i);
                    Vector3 target = result.toFind.center;
                    Vector3 offset = target - center;
                    go.transform.position += offset;
                });
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