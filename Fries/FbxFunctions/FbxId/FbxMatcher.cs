# if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Fries.Inspector;
using UnityEditor;
using UnityEngine;

namespace Fries.FbxFunctions.FbxId {
    [Serializable]
    public class FbxIdInfo {
        public string fbxPath;
        public string meshName;
        public float largestLength;
        public double[,] vertices;
        public Vector3 center;
        public Vector3 shortestPt1;
        public Vector3 shortestPt2;
        public Vector3 longestPt1;
        public Vector3 longestPt2;
        public float[] idArray;
    }

    [Serializable]
    public class FbxSearchResult {
        public bool isValid;
        public float likeliness;
        public FbxIdInfo toFind;
        public FbxIdInfo found;
        public Vector3 rotateAngle;
        public GameObject modelAsset;
    }
    
    public class FbxMatcher : MonoBehaviour {
        public TextAsset idDatabase;
        public TextAsset cmpTemp;

        private Dictionary<float[], FbxIdInfo> matchDatabase;
        private List<FbxIdInfo> cmpDatabase;
        private List<FbxIdInfo> toFind;

        public List<FbxSearchResult> foundFbxAssets;
        public List<FbxSearchResult> unfoundFbxAssets;
        
        [AButton("Reload Fbx Data")] [IgnoreInInspector]
        public Action reloadFbxData;
        
        [AButton("Search")] [IgnoreInInspector]
        public Action search;

        public FbxSearchResult getModelForCmpKey(string key) {
            foreach (var result in foundFbxAssets.Where(result => result.toFind.meshName == key)) {
                return result;
            }
            return null;
        }

        private void OnValidate() {
            List<FbxSearchResult> found = new();
            unfoundFbxAssets.Nullable().ForEach(result => {
                if (!result.isValid) return;
                result.likeliness = 100;
                found.Add(result);
            });
            found.Nullable().ForEach(result => {
                unfoundFbxAssets.Remove(result);
                foundFbxAssets.Add(result);
            });
        }

        private void Reset() {
            reloadFbxData = () => {
                matchDatabase = new();
                cmpDatabase = load(idDatabase.text);
                cmpDatabase.ForEach(fii => {
                    matchDatabase[fii.idArray] = fii;
                });
                toFind = load(cmpTemp.text);
            };

            search = () => {
                foundFbxAssets = new();
                unfoundFbxAssets = new();
                try {
                    int i = 0;
                    foreach (var fbxToFind in toFind) {
                        EditorUtility.DisplayProgressBar(
                            $"Search #{i} Fbx: {fbxToFind.meshName}",
                            $"Sub Progression: 0 / {cmpDatabase.Count}",
                            (float)i / toFind.Count
                        );

                        if (matchDatabase.ContainsKey(fbxToFind.idArray)) {
                            string dataPath1 = Application.dataPath;
                            string fbxPath1 = matchDatabase[fbxToFind.idArray].fbxPath.Replace("\\", "/");
                            if (fbxPath1.StartsWith(dataPath1))
                                fbxPath1 = "Assets" + fbxPath1.Substring(dataPath1.Length);
                            GameObject fbxModelFile1 = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath1);

                            foundFbxAssets.Add(new FbxSearchResult {
                                toFind = fbxToFind,
                                found = matchDatabase[fbxToFind.idArray],
                                likeliness = 1,
                                modelAsset = fbxModelFile1
                            });
                            continue;
                        }

                        SortedList<float, FbxIdInfo> results = new SortedList<float, FbxIdInfo>();
                        int j = 0;
                        foreach (var fbxInDatabase in cmpDatabase) {
                            EditorUtility.DisplayProgressBar(
                                $"Search #{i} Fbx: {fbxToFind.meshName}",
                                $"Sub Progression: {j} / {cmpDatabase.Count}",
                                (float)i / toFind.Count
                            );
                            float likeliness = compareId(fbxToFind.idArray, fbxInDatabase.idArray);
                            if (results.ContainsKey(likeliness)) results.Remove(likeliness);
                            results.Add(likeliness, fbxInDatabase);
                            j++;
                        }

                        string dataPath = Application.dataPath;
                        string fbxPath = results.Values[^1].fbxPath.Replace("\\", "/");
                        if (fbxPath.StartsWith(dataPath))
                            fbxPath = "Assets" + fbxPath.Substring(dataPath.Length);
                        GameObject fbxModelFile = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);

                        if (results.Keys[^1] < 98) {
                            unfoundFbxAssets.Add(new FbxSearchResult {
                                isValid = false,
                                toFind = fbxToFind,
                                found = results.Values[^1],
                                likeliness = results.Keys[^1],
                                modelAsset = fbxModelFile
                            });
                        }
                        else {
                            foundFbxAssets.Add(new FbxSearchResult {
                                isValid = true,
                                toFind = fbxToFind,
                                found = results.Values[^1],
                                likeliness = results.Keys[^1],
                                modelAsset = fbxModelFile
                            });
                        }

                        i++;
                    }
                }
                finally {
                    EditorUtility.ClearProgressBar();
                }
            };
        }

        private List<FbxIdInfo> load(string raw) {
            List<FbxIdInfo> result = new();
            string[] fbxes = raw.Split("\r\n\r\n\r\n");
            foreach (var fbxRaw in fbxes) {
                if (fbxRaw.Nullable().Length < 10 && string.IsNullOrEmpty(fbxRaw.Nullable().Trim())) continue;
                string[] comps = fbxRaw.Split("|");
                string[] idArrayRaw = comps[6].Split(" ");
                float[] idArray = new float[idArrayRaw.Length];
                idArrayRaw.ForEach((i, idSingleRaw) => {
                    try {
                        idArray[i] = float.Parse(idSingleRaw);
                    }
                    catch (Exception e) {
                        throw e;
                    }
                });

                string centerString = comps[3];
                string[] centerRaw = centerString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Vector3 center = new Vector3(float.Parse(centerRaw[0]), float.Parse(centerRaw[1]), float.Parse(centerRaw[2]));

                Vector3 sp1 = new(-100000,-100000,-100000), sp2 = new(-100000,-100000,-100000), lp1 = new(-100000,-100000,-100000), lp2 = new Vector3(-100000,-100000,-100000);
                string traitPoints = comps[4];
                string[] traitPointsRaw = traitPoints.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                traitPointsRaw.ForEach((i, traitPointRaw) => {
                    if (traitPointRaw.Nullable().Trim() == "") return;
                    string[] xyz = traitPointRaw.Split(",");
                    Vector3 v = new Vector3(float.Parse(xyz[0]), float.Parse(xyz[1]), float.Parse(xyz[2]));
                    if (i == 0) sp1 = v;
                    if (i == 1) sp2 = v;
                    if (i == 2) lp1 = v;
                    if (i == 3) lp2 = v;
                });

                string vertexString = comps[5];
                string[] vertexEntries = vertexString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                int vertexCount = vertexEntries.Length;
                double[,] array = new double[vertexCount, 3];
                for (int i = 0; i < vertexCount; i++) {
                    string[] coords = vertexEntries[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < 3; j++) 
                        array[i, j] = double.Parse(coords[j], CultureInfo.InvariantCulture);
                }

                FbxIdInfo fii = new FbxIdInfo {
                    fbxPath = comps[0],
                    meshName = comps[1],
                    largestLength = float.Parse(comps[2]),
                    vertices = array,
                    center = center,
                    shortestPt1 = sp1,
                    shortestPt2 = sp2,
                    longestPt1 = lp1,
                    longestPt2 = lp2,
                    idArray = idArray
                };
                result.Add(fii);
            }

            return result;
        }

        private float compareId(float[] id1, float[] id2) {
            // 若数组长度不同，则将较短数组通过插值扩展到与较长数组相同的长度
            if (id1.Length < id2.Length) 
                id1 = interpolateAndExtendShorterArray(id1, id2.Length);
            else if (id2.Length < id1.Length) 
                id2 = interpolateAndExtendShorterArray(id2, id1.Length);

            int n = id1.Length;
            float sum1 = 0, sum2 = 0;
            for (int i = 0; i < n; i++) {
                sum1 += id1[i];
                sum2 += id2[i];
            }

            float mean1 = sum1 / n;
            float mean2 = sum2 / n;

            float numerator = 0;
            float denom1 = 0;
            float denom2 = 0;
            for (int i = 0; i < n; i++) {
                float diff1 = id1[i] - mean1;
                float diff2 = id2[i] - mean2;
                numerator += diff1 * diff2;
                denom1 += diff1 * diff1;
                denom2 += diff2 * diff2;
            }

            float denominator = Mathf.Sqrt(denom1 * denom2);
            float corr = denominator == 0 ? 0 : numerator / denominator;

            // 将相关系数映射到 [0,100]，保证相似度为正
            float similarityPercentage = ((corr + 1) / 2) * 100;
            return similarityPercentage;
        }

        private float[] interpolateAndExtendShorterArray(float[] data, int newLength) {
            float[] result = new float[newLength];
            int oldLength = data.Length;

            if (newLength == 1) {
                result[0] = data[0];
                return result;
            }

            // 计算比例系数，确保原数组第一个和最后一个点对应新数组的首尾
            float scale = (oldLength - 1) / (float)(newLength - 1);
            for (int i = 0; i < newLength; i++) {
                float pos = i * scale;
                int index = (int)pos;
                float frac = pos - index;
                if (index + 1 < oldLength)
                    result[i] = data[index] * (1 - frac) + data[index + 1] * frac;
                else
                    result[i] = data[index];
            }

            return result;
        }
    }

    [CustomEditor(typeof(FbxMatcher))]
    public class FbxMatcherInsp : AnInspector {
        
    }
}
# endif