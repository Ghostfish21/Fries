using System;
using System.Collections.Generic;
using System.Linq;
using Fries.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fries {
    public static class UnityExts {
        public static string getPath(this GameObject obj) {
            string path = obj.name;
            Transform current = obj.transform;
            while (current.parent != null) {
                current = current.parent;
                path = current.name + "/" + path;
            }

            return path;
        }

        public static GameObject mkdirs(string hierarchyPath, string seperator = "/") {
            string[] nodes = hierarchyPath.Split(seperator);
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            List<GameObject> possibleRoots = roots.Where(gobj => gobj.name == nodes[0]).ToList();
            if (possibleRoots.Count() > 1) {
                Debug.LogError($"There are more than 1 GameObject with name {nodes[0]}, can not make meaningful directories");
                return null;
            }

            GameObject root;
            if (!possibleRoots.Any()) root = new GameObject(nodes[0]);
            else root = possibleRoots[0];

            string currentLeaf = nodes[0];
            for (int i = 1; i < nodes.Length; i++) {
                string name = nodes[i];
                currentLeaf += $"/{name}";
                Transform[] identicalNames = root.transform.findAll(name);
                if (identicalNames.Count() > 1) {
                    Debug.LogError($"There are more than 1 GameObject with name {currentLeaf}, can not make meaningful directories");
                    return null;
                }
                
                if (!identicalNames.Any()) root = new GameObject(name);
                else root = identicalNames[0].gameObject;
            }

            return root;
        }

        public static Transform[] findAll(this Transform parent, string name) {
            List<Transform> children = new();
            foreach (Transform child in parent) {
                if (child.name == name) children.Add(child);
                children.AddRange(child.findAll(name));
            }
            return children.ToArray();
        }

        // 只适用与标准 Unity朝向，X+朝东，Y+朝上，Z+朝北
        public static Vector3 getStdExtremePt(this MeshRenderer meshRenderer, Facing ofDirection) {
            float midX = meshRenderer.bounds.min.x + (meshRenderer.bounds.max.x - meshRenderer.bounds.min.x) / 2;
            float midY = meshRenderer.bounds.min.y + (meshRenderer.bounds.max.y - meshRenderer.bounds.min.y) / 2;
            float midZ = meshRenderer.bounds.min.z + (meshRenderer.bounds.max.z - meshRenderer.bounds.min.z) / 2;
            
            switch (ofDirection) {
                case Facing.up:
                    return midX.f__(meshRenderer.bounds.max.y, midZ);
                case Facing.down:
                    return midX.f__(meshRenderer.bounds.min.y, midZ);
                case Facing.east:
                    return meshRenderer.bounds.max.x.f__(midY, midZ);
                case Facing.west:
                    return meshRenderer.bounds.min.x.f__(midY, midZ);
                case Facing.north:
                    return midX.f__(midY, meshRenderer.bounds.max.z);
                case Facing.south:
                    return midX.f__(midY, meshRenderer.bounds.min.z);
            }

            throw new ArgumentException("Facing is illegal");
        }
    }
}