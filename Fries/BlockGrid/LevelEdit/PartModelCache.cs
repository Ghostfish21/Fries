using System.Collections.Generic;
using System.Text;
using Fries.Pool;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    public class PartModelCache {
        private Dictionary<string, GameObject> prefabCache = new();
        private Dictionary<object, Stack<GameObject>> modelCache = new();

        public void Deactivate(int partId, GameObject elem) {
            elem.transform.position = Vector3.zero;
            elem.transform.rotation = Quaternion.identity;
            elem.SetActive(false);

            object enumObj = PartRegistry.GetEnum(partId);
            if (!modelCache.TryGetValue(enumObj, out var stack)) {
                stack = new Stack<GameObject>();
                modelCache.Add(enumObj, stack);
            }
            stack.Push(elem);
        }
        
        public GameObject Activate(object partEnum, out GameObject prefab, out int partId) {
            StringBuilder stringBuilder = LevelEditor.Inst.EverythingPool.ActivateObject<StringBuilder>();
            string prefabPath = PartRegistry.GetPath(partEnum, out partId, stringBuilder);
            LevelEditor.Inst.EverythingPool.DeactivateObject(stringBuilder);

            if (!prefabCache.TryGetValue(prefabPath, out prefab)) {
                prefab = Resources.Load<GameObject>(prefabPath);
                if (!prefab) {
                    LevelEditor.writer.write($"Part prefab at {prefabPath} cannot be found!");
                    return null;
                }
                prefabCache.Add(prefabPath, prefab);
            }

            if (!prefab) {
                prefab = Resources.Load<GameObject>(prefabPath);
                if (!prefab) {
                    LevelEditor.writer.write($"Part prefab at {prefabPath} cannot be found!");
                    return null;
                }
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

            var gobj = stack.Pop();
            gobj.SetActive(true);
            return gobj;
        }
    }
}