using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fries.FbxFunctions.ModelRecognition {
    public class ModelMatchResult {
        public ModelFingerprint searched;
        public ModelFingerprint found;

        public float distance;

        public Vector3 offset;
        public Vector3 eulerAngle;
        public Vector3 scale;
    }
    
    public class FingerprintDatabase : MonoBehaviour {
        public List<ModelFingerprint> fingerprintInsts;

        public List<ModelFingerprint> toSearch;

        public List<ModelMatchResult> searchResult;

        private void OnValidate() {
            foreach (var searchedContent in toSearch) {
                float closestDist = float.MaxValue;
                ModelFingerprint closestFingerprint = null;

                foreach (var fingerprint in fingerprintInsts) {
                    float dist = (searchedContent.fingerPrintVector - fingerprint.fingerPrintVector).magnitude;
                    if (dist < closestDist) {
                        closestDist = dist;
                        closestFingerprint = fingerprint;
                    }
                }

                ModelMatchResult mr = new ModelMatchResult() {
                    searched = searchedContent,
                    found = closestFingerprint,
                };
                if (closestDist <= 1) {
                    Vector3[] transformArg = closestFingerprint.recreateTransform(searchedContent);
                    if (transformArg != null) {
                        mr.distance = closestDist;
                        mr.offset = transformArg[0];
                        mr.eulerAngle = transformArg[1];
                        mr.scale = transformArg[2];
                    }
                }
                
                searchResult.Add(mr);
            }
        }
    }
}