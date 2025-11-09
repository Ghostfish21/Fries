# if UNITY_EDITOR
using UnityEditor;
# endif
using UnityEngine;

namespace Fries.Inspector {
    public class CreateSceneStructure {
# if UNITY_EDITOR
        [MenuItem("Fries/Create Scene Structure")]
        public static void Create() {
            new GameObject("==== System ====");
            new GameObject("==== Scene   ====");
            new GameObject("==== Render  ====");
            new GameObject("==== Ui          ====");
        }
# endif
    }
}