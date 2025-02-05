using System.IO;
using UnityEngine;

namespace Fries.Inspector {
    # if UNITY_EDITOR
    using UnityEditor;
    # endif
    
    [System.Serializable]
    public class KiiValuePair {
        public KiiValuePair(float keyWidth = 0, float valueWidth = 0) {
            string dataPath = Application.dataPath;
            // 获取Assets文件夹所在的父文件夹名称，即项目名称
            string projectName = new DirectoryInfo(dataPath).Parent.Name;
            Debug.Log("Project Name: " + projectName);
            # if UNITY_EDITOR
            EditorPrefs.SetFloat($"{projectName}.{this.GetType().Name}.Key_Width", keyWidth);
            EditorPrefs.SetFloat($"{projectName}.{this.GetType().Name}.Value_Width", valueWidth);
            # endif
        }
    }

    [System.Serializable]
    public class KiiValuePair<K, V> : KiiValuePair { 
        [SerializeField] public K key;
        [SerializeField] public V value;

        public KiiValuePair(float keyWidth = 0, float valueWidth = 0) : base(keyWidth, valueWidth) { }
    }
}