using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fries {
    public static class ComponentCache {
        private static Dictionary<GameObject, Dictionary<Type, Component>> compCache = new();
        private static Dictionary<GameObject, Dictionary<string, GameObject>> childrenCache = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void reset() {
            compCache = new();
            childrenCache = new();
        }
        
        
        public static GameObject find(this GameObject gameObject, string name, bool refresh = false) {
            bool newRow = childrenCache.TryAdd(gameObject, new Dictionary<string, GameObject>());
            if (refresh) childrenCache[gameObject] = new Dictionary<string, GameObject>();
            if (refresh || newRow) {
                foreach (Transform child in gameObject.transform) 
                    childrenCache[gameObject][child.name] = child.gameObject;
            }
            
            if (childrenCache[gameObject].TryGetValue(name, out var child1)) return child1;
            childrenCache[gameObject][name] = gameObject.transform.Find(name)?.gameObject;
            return childrenCache[gameObject][name];
        }
        
        public static GameObject find(this Transform transform, string name, bool refresh = false) {
            return transform.gameObject.find(name, refresh);
        }
        
        public static GameObject find(this Component component, string name, bool refresh = false) {
            return component.gameObject.find(name, refresh);
        }
        
        public static T getComponent<T>(this GameObject gameObject, bool refresh = false) where T : Component {
            compCache.TryAdd(gameObject, new Dictionary<Type, Component>());
            if (refresh) {
                compCache[gameObject][typeof(T)] = gameObject.GetComponent<T>();
                return (T)compCache[gameObject][typeof(T)];
            }

            if (compCache[gameObject].TryGetValue(typeof(T), out var content) && !content)
                compCache[gameObject].Remove(typeof(T));
                
            if (!compCache[gameObject].ContainsKey(typeof(T))) 
                compCache[gameObject][typeof(T)] = gameObject.GetComponent<T>();
            return (T)compCache[gameObject][typeof(T)];
        }
        
        public static T getComponent<T>(this Transform transform, bool refresh = false) where T : Component {
            return getComponent<T>(transform.gameObject, refresh);
        }
        
        public static T getComponent<T>(this Component component, bool refresh = false) where T : Component {
            return getComponent<T>(component.gameObject, refresh);
        }
    }
}