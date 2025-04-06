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
        public List<FbxItemPositionInfo> infos = new();

        [AButton("Import")] [IgnoreInInspector]
        public Action import;

        private void Reset() {
            import = () => {
                FbxMatcher fbxMatcher = gameObject.GetComponent<FbxMatcher>();
                fbxMatcher.foundFbxAssets.Nullable().ForEach(result => {
                    GameObject modelAsset = result.modelAsset;
                    GameObject root = GameObject.Find("Root");
                    DestroyImmediate(root, true);
                    root = new GameObject("Root");
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
                    Vector3 shortestP1 = 10000f.fff();
                    Vector3 shortestP2 = 10000f.fff();
                    Vector3 largestP1 = 10000f.fff();
                    Vector3 largestP2 = 10000f.fff();
                    float longest = 0;
                    float shortest = 100000;
                    Vector3 lastPt = 10000f.fff();
                    foreach (var vertex in mesh.vertices) {
                        Vector3 worldVertex = transform.TransformPoint(vertex);
                        totalX += worldVertex.x;
                        totalY += worldVertex.y;
                        totalZ += worldVertex.z;
                        if (lastPt == 10000f.fff()) {
                            lastPt = worldVertex;
                            i++;
                            continue;
                        }
                        float length = new Vector3(worldVertex.x - lastPt.x, worldVertex.y - lastPt.y,
                            worldVertex.z - lastPt.z).magnitude;
                        if (length > longest) {
                            longest = length;
                            largestP1 = worldVertex;
                            largestP2 = lastPt;
                        }
                        if (length < shortest) {
                            shortest = length;
                            shortestP1 = worldVertex;
                            shortestP2 = lastPt;
                        }
                        lastPt = worldVertex;
                        i++;
                    }
                    
                    Vector3 center = new Vector3(totalX / i, totalY / i, totalZ / i);
                    float dist1 = shortestP1.minus(center).magnitude;
                    float dist2 = shortestP2.minus(center).magnitude;
                    if (dist2 < dist1) 
                        (shortestP1, shortestP2) = (shortestP2, shortestP1);
                    
                    float dist3 = largestP1.minus(center).magnitude;
                    float dist4 = largestP2.minus(center).magnitude;
                    if (dist4 < dist3)
                        (largestP1, largestP2) = (largestP2, largestP1);

                    result.found.longestPt1 = largestP1;
                    result.found.longestPt2 = largestP2;
                    result.found.shortestPt1 = shortestP1;
                    result.found.shortestPt2 = shortestP2;
                    
                    Vector3 target = result.toFind.center;
                    Vector3 offset = target - center;
                    go.transform.position += offset;
                });
            };
        }
    }
    
    [CustomEditor(typeof(FbxCollectionImporter))]
    public class FbxCollectionImporterInst : AnInspector {}
}
# endif