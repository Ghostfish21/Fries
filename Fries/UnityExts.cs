using System.Collections.Generic;
using System.Linq;
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
                
                if (!possibleRoots.Any()) root = new GameObject(name);
                else root = identicalNames[0].gameObject;
            }

            return root;
        }

        public static Transform[] findAll(this Transform parent, string name) {
            return parent.Cast<Transform>().Where(child => child.name == name).ToArray();
        }
    }
}