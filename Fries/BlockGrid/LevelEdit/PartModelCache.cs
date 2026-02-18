using System.Collections.Generic;
using System.Text;
using Fries.Pool;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    public class PartModelCache {
        private Dictionary<string, GameObject> prefabCache = new();
        private Dictionary<object, Stack<GameObject>> modelCache = new();

        public void Deactivate(GameObject elem) {
            elem.transform.position = Vector3.zero;
            elem.transform.rotation = Quaternion.identity;
            elem.SetActive(false);

            if (!modelCache.TryGetValue(elem, out var stack)) {
                stack = new Stack<GameObject>();
                modelCache.Add(elem, stack);
            }
            stack.Push(elem);
        }
        
        public GameObject Activate(object partEnum, out GameObject prefab, out int partId) {
            StringBuilder stringBuilder = LevelEditor.Inst.EverythingPool.ActivateObject<StringBuilder>();
            string prefabPath = PartRegistry.GetPath(partEnum, out partId, stringBuilder);

            if (!prefabCache.TryGetValue(prefabPath, out prefab)) {
                prefab = Resources.Load<GameObject>(prefabPath); 
                prefabCache.Add(prefabPath, prefab);
            }
            
            if (!modelCache.TryGetValue(partEnum, out var stack)) {
                stack = new Stack<GameObject>();
                modelCache.Add(partEnum, stack);
            }

            if (stack.Count == 0) {
                GameObject obj = Object.Instantiate(prefab, LevelEditor.Inst.BlockMap.transform);
                obj.AddComponent<PartInfoHolder>();
                stack.Push(obj);
            }
            
            return stack.Pop();
        }
    }
}