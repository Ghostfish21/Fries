using UnityEngine;

namespace Fries {
    public static class UnityExts {
        public static string getGameObjectPath(this GameObject obj) {
            string path = obj.name;
            Transform current = obj.transform;
            while (current.parent != null) {
                current = current.parent;
                path = current.name + "/" + path;
            }

            return path;
        }
    }
}